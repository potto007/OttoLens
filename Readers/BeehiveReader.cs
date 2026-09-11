using OttoLens.Model;

namespace OttoLens.Readers;

/// Beehive (spec 3.6). Honey stock is the primary meter, time to the next honey the secondary,
/// and the headline carries the hive's state the same way vanilla picks its message.
internal sealed class BeehiveReader : ILensReader
{
    private static readonly Dictionary<int, Sprite?> SpriteCache = new();

    private readonly LensReport _report = new();
    private readonly LensItemBlock _output = new();

    // CheckBiome costs a Heightmap.FindBiome and HaveFreeSpace costs a 100 m spherecast plus
    // seventeen 30 m raycasts through Cover.GetCoverForPoint. Vanilla pays that once per ten
    // seconds from UpdateBees; the panel would pay it at refreshHz. Hold the pair for a
    // couple of seconds per hive so a resting crosshair costs nothing (spec 6.4 rule 7).
    private const float StateCacheSeconds = 2f;
    private Beehive? _cachedHive;
    private float _cachedUntil;
    private bool _cachedBiomeOk;
    private bool _cachedSpaceOk;

    public Type TargetType => typeof(Beehive);

    public bool Enabled => OttoLensPlugin.ShowFermenting.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var hive = (Beehive)target;
        ZNetView view = hive.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        if (!LensReaders.HasWardAccess(target))
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        int honey = zdo.GetInt(ZDOVars.s_level);
        int max = hive.m_maxHoney;
        bool full = max > 0 && honey >= max;

        // Same order vanilla uses: biome, then cover, then daylight, otherwise happy.
        ReadState(hive, out bool biomeOk, out bool spaceOk);
        bool working = biomeOk && spaceOk;
        bool asleep = working && hive.m_effectOnlyInDaylight && !EnvMan.IsDaylight();

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(hive.m_name);

        if (full)
        {
            report.Headline = "FULL";
            report.HeadlineColor = LensColor.Good;
        }
        else if (!biomeOk)
        {
            report.Headline = "WRONG BIOME";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (!spaceOk)
        {
            report.Headline = "NO SPACE";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (asleep)
        {
            report.Headline = "ASLEEP";
            report.HeadlineColor = LensColor.Dim;
        }
        else
        {
            report.Headline = "HAPPY";
            report.HeadlineColor = LensColor.Gold;
        }

        float stock = max > 0 ? Mathf.Clamp01((float)honey / max) : 0f;
        report.PrimaryMeter = LensReport.Meter("Honey", max > 0 ? stock : null, LensFormat.Count(honey, max),
            full ? LensColor.Good : LensColor.Gold);

        // Progress only advances while the owner ticks a working hive; a stalled hive shows a
        // frozen countdown, which is honest, and the headline says why.
        if (!full && working && hive.m_secPerUnit > 0f)
        {
            float progress = zdo.GetFloat(ZDOVars.s_product);
            float remaining = Mathf.Max(0f, hive.m_secPerUnit - progress);
            report.Secondary1 = LensReport.Meter("Next", Mathf.Clamp01(remaining / hive.m_secPerUnit),
                LensFormat.Time(remaining), asleep ? LensColor.Dim : LensColor.Gold, drains: true);
        }

        if (honey > 0 && hive.m_honeyItem != null)
        {
            ItemDrop.ItemData data = hive.m_honeyItem.m_itemData;
            _output.Clear();
            _output.IsOutput = true;
            _output.Items.Add(new LensItem(GetSprite(hive.m_honeyItem.gameObject.name, data),
                LensFormat.Name(data.m_shared.m_name), honey));
            report.Block1 = _output;
        }

        return report;
    }

    /// Biome and cover for one hive, recomputed at most once per StateCacheSeconds. One slot:
    /// only one hive is hovered at a time, and switching hives just recomputes once.
    private void ReadState(Beehive hive, out bool biomeOk, out bool spaceOk)
    {
        float now = Time.unscaledTime;
        if (!ReferenceEquals(hive, _cachedHive) || now >= _cachedUntil)
        {
            _cachedHive = hive;
            _cachedUntil = now + StateCacheSeconds;
            _cachedBiomeOk = hive.CheckBiome();
            _cachedSpaceOk = _cachedBiomeOk && hive.HaveFreeSpace();
        }

        biomeOk = _cachedBiomeOk;
        spaceOk = _cachedSpaceOk;
    }

    // The cache outlives a world unload: a destroyed sprite reads as Unity null and is fetched
    // again, so a stale entry cannot hand the panel a fake-null sprite that blanks the row art.
    private static Sprite? GetSprite(string prefabName, ItemDrop.ItemData data)
    {
        int key = prefabName.GetStableHashCode();
        if (SpriteCache.TryGetValue(key, out Sprite? cached) && cached != null)
        {
            return cached;
        }

        Sprite? sprite = null;
        try
        {
            sprite = data.GetIcon();
        }
        catch (Exception)
        {
            // A missing icon array leaves the tile blank; the name still renders.
        }

        SpriteCache[key] = sprite;
        return sprite;
    }
}
