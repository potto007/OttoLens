using OttoLens.Model;

namespace OttoLens.Readers;

/// Sap extractor (spec 3.7). Mirrors the beehive: sap stock as the primary meter, time to the
/// next unit as a secondary, and the root's remaining level as a second secondary.
internal sealed class SapCollectorReader : ILensReader
{
    private static readonly Dictionary<int, Sprite?> SpriteCache = new();

    private readonly LensReport _report = new();
    private readonly LensItemBlock _output = new();

    public Type TargetType => typeof(SapCollector);

    public bool Enabled => OttoLensPlugin.ShowFermenting.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var collector = (SapCollector)target;
        ZNetView view = collector.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        int level = zdo.GetInt(ZDOVars.s_level);
        int max = collector.m_maxLevel;
        bool full = max > 0 && level >= max;

        // The root's level lives in its own ZDO; an unloaded root reads as absent.
        ResourceRoot? root = collector.m_root;
        if (root != null && (root.m_nview == null || !root.m_nview.IsValid()))
        {
            root = null;
        }

        bool draining = root != null && root.CanDrain(1f);
        bool slow = root != null && root.IsLevelLow();

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(collector.m_name);

        if (full)
        {
            report.Headline = "FULL";
            report.HeadlineColor = LensColor.Good;
        }
        else if (root == null)
        {
            report.Headline = "NOT CONNECTED";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (!draining)
        {
            report.Headline = "ROOT EMPTY";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (slow)
        {
            report.Headline = "DRAINING SLOW";
            report.HeadlineColor = LensColor.Warn;
        }
        else
        {
            report.Headline = "DRAINING";
            report.HeadlineColor = LensColor.Gold;
        }

        float stock = max > 0 ? Mathf.Clamp01((float)level / max) : 0f;
        report.PrimaryMeter = LensReport.Meter("Sap", max > 0 ? stock : null, LensFormat.Count(level, max),
            full ? LensColor.Good : LensColor.Gold);

        // Sap only flows while attached to a root that can still drain (spec 3.7 edge case).
        if (!full && draining && collector.m_secPerUnit > 0f)
        {
            float progress = zdo.GetFloat(ZDOVars.s_product);
            float remaining = Mathf.Max(0f, collector.m_secPerUnit - progress);
            report.Secondary1 = LensReport.Meter("Next", Mathf.Clamp01(remaining / collector.m_secPerUnit),
                LensFormat.Time(remaining), slow ? LensColor.Warn : LensColor.Gold, drains: true);
        }

        if (root != null && root.m_maxLevel > 0f)
        {
            float rootFraction = Mathf.Clamp01(root.GetLevel() / root.m_maxLevel);
            report.Secondary2 = LensReport.Meter("Root", rootFraction, LensFormat.Percent(rootFraction),
                LensFormat.Ramp(rootFraction));
        }

        if (level > 0 && collector.m_spawnItem != null)
        {
            ItemDrop.ItemData data = collector.m_spawnItem.m_itemData;
            _output.Clear();
            _output.IsOutput = true;
            _output.Items.Add(new LensItem(GetSprite(collector.m_spawnItem.gameObject.name, data),
                LensFormat.Name(data.m_shared.m_name), level));
            report.Block1 = _output;
        }

        return report;
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
