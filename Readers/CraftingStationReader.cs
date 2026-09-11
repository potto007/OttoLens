using System.Globalization;
using OttoLens.Model;

namespace OttoLens.Readers;

/// Crafting station (spec 3.21). Headline says whether the station can be used (fire, roof),
/// the Level row shows the extension level, and when a Container hangs off the station its
/// grouped contents are appended with the Slots meter and a Free footer. Nothing shows out of
/// use range, where vanilla shows too far.
internal sealed class CraftingStationReader : ILensReader
{
    private static readonly Dictionary<string, Sprite?> IconCache = new(StringComparer.Ordinal);

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly Dictionary<string, LensItem> _groupIndex = new(StringComparer.Ordinal);
    private readonly List<LensItem> _groups = new();

    // The container lookup walks children, so it is remembered per station instance.
    private CraftingStation? _cachedStation;
    private Container? _cachedContainer;

    // CraftingStation.CheckUsable runs Cover.GetCoverForPoint for a roof requiring station:
    // one 100 m spherecast plus seventeen 30 m raycasts, eighteen physics queries per call.
    // Vanilla only pays that on an interact or a craft press, never on hover; the panel would
    // pay it at refreshHz. Hold the verdict for a couple of seconds per station (spec 6.4
    // rule 7), the same way BeehiveReader.ReadState holds its cover check. Cover changes no
    // faster than the player builds.
    private const float RoofCacheSeconds = 2f;
    private CraftingStation? _roofStation;
    private float _roofUntil;
    private bool _roofOk;

    // Grouping cache, keyed on Container.m_lastRevision, not ZDO.DataRevision: m_lastRevision
    // is the revision the inventory was actually deserialized from, while DataRevision moves as
    // soon as the ZDO arrives and the 1 s CheckForChanges tick reloads m_inventory up to a
    // second later (same reasoning as ContainerReader, lines 19-25). The item count is a cheap
    // second guard.
    private Container? _groupedContainer;
    private uint _groupedRevision;
    private int _groupedCount = -1;

    public Type TargetType => typeof(CraftingStation);

    public bool Enabled => OttoLensPlugin.ShowMisc.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var station = (CraftingStation)target;
        ZNetView view = station.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        Player player = Player.m_localPlayer;
        if (player == null || !station.InUseDistance(player))
        {
            return null;
        }

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(station.m_name);

        if (station.m_craftRequireFire && !station.m_haveFire)
        {
            report.Headline = "NO FIRE";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (station.m_craftRequireRoof && station.m_roofCheckPoint != null && !HasRoof(station, player))
        {
            report.Headline = "NO ROOF";
            report.HeadlineColor = LensColor.Bad;
        }
        else
        {
            report.Headline = "READY";
            report.HeadlineColor = LensColor.Good;
        }

        // checkExtensions: false keeps this a read. The default true walks GetExtentionCount ->
        // GetExtensions, which rebuilds m_attachedExtensions, resets the vanilla 2 s refresh
        // timer and rewrites the station's effect area collider radius (spec 6.2 item 5).
        int level = station.GetLevel(checkExtensions: false);
        LensMeter levelMeter = LensReport.Meter("Level", null, level.ToString(CultureInfo.InvariantCulture), LensColor.Gold);

        Container? container = FindContainer(station);
        if (container == null || !CanReadContainer(container, player))
        {
            report.PrimaryMeter = levelMeter;
            return report;
        }

        Inventory? inventory = container.GetInventory();
        ZDO? zdo = container.m_nview != null && container.m_nview.IsValid() ? container.m_nview.GetZDO() : null;
        if (inventory == null || zdo == null)
        {
            report.PrimaryMeter = levelMeter;
            return report;
        }

        int capacity = inventory.GetWidth() * inventory.GetHeight();
        int free = inventory.GetEmptySlots();
        int used = capacity - free;
        float fill = capacity > 0 ? Mathf.Clamp01((float)used / capacity) : 0f;

        report.PrimaryMeter = LensReport.Meter("Slots", capacity > 0 ? fill : (float?)null, LensFormat.Count(used, capacity), LensFormat.FillRamp(fill));
        report.Secondary1 = levelMeter;
        report.Footer = LensFormat.Free(free);
        if (capacity > 0 && free == 0)
        {
            report.Headline = "FULL";
            report.HeadlineColor = LensColor.Bad;
        }

        List<ItemDrop.ItemData> items = inventory.GetAllItems();
        uint revision = container.m_lastRevision;
        if (_groupedContainer != container || _groupedRevision != revision || _groupedCount != items.Count)
        {
            _groupedContainer = container;
            _groupedRevision = revision;
            _groupedCount = items.Count;
            Regroup(items);
        }

        if (_groups.Count > 0)
        {
            LensItemBlock block = _block;
            block.Clear();
            block.Label = "Contents";
            block.Items.AddRange(_groups);
            report.Block0 = block;
        }

        report.ContentSignature = string.Concat(report.Title, "|", revision.ToString(CultureInfo.InvariantCulture), "|", items.Count.ToString(CultureInfo.InvariantCulture), "|", report.Headline);
        return report;
    }

    private Container? FindContainer(CraftingStation station)
    {
        // Most stations have no container, so the miss is cached too, per station instance.
        if (_cachedStation != station)
        {
            _cachedStation = station;
            _cachedContainer = station.GetComponentInChildren<Container>();
        }

        return _cachedContainer;
    }

    /// The roof verdict for one station, recomputed at most once per RoofCacheSeconds. One slot:
    /// only one station is hovered at a time, and switching stations just recomputes once.
    private bool HasRoof(CraftingStation station, Player player)
    {
        float now = Time.unscaledTime;
        if (!ReferenceEquals(station, _roofStation) || now >= _roofUntil)
        {
            _roofStation = station;
            _roofUntil = now + RoofCacheSeconds;
            _roofOk = station.CheckUsable(player, showMessage: false);
        }

        return _roofOk;
    }

    /// Same denial rules as the chest reader: ward, then the private setting. A locked
    /// container contributes nothing rather than a hint of its contents.
    private static bool CanReadContainer(Container container, Player player)
    {
        if (container.m_checkGuardStone && !LensReaders.HasWardAccess(container))
        {
            return false;
        }

        if (container.m_privacy != Container.PrivacySetting.Public)
        {
            if (container.m_piece == null || !container.CheckAccess(player.GetPlayerID()))
            {
                return false;
            }
        }

        return true;
    }

    /// Group by shared name and sum stacks, keeping first-appearance order, which is the
    /// container's own slot order. Row order belongs to the panel (design section 7): sorting
    /// here as well would make `sortRows = slot` indistinguishable from `count`, because
    /// LensPanel.ItemBlockView.Apply only reorders for `count` and otherwise keeps this order.
    private void Regroup(List<ItemDrop.ItemData> items)
    {
        _groupIndex.Clear();
        _groups.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            ItemDrop.ItemData data = items[i];
            if (data == null || data.m_shared == null)
            {
                continue;
            }

            string token = data.m_shared.m_name;
            if (_groupIndex.TryGetValue(token, out LensItem group))
            {
                group.Count += data.m_stack;
                continue;
            }

            group = new LensItem(Icon(data, token), LensFormat.Name(token), data.m_stack);
            _groupIndex[token] = group;
            _groups.Add(group);
        }
    }

    // The cache outlives a world unload: a destroyed sprite reads as Unity null and is fetched
    // again, so a stale entry cannot hand the panel a fake-null sprite that blanks the row art.
    private static Sprite? Icon(ItemDrop.ItemData item, string token)
    {
        if (IconCache.TryGetValue(token, out Sprite? cached) && cached != null)
        {
            return cached;
        }

        Sprite? sprite;
        try
        {
            sprite = item.GetIcon();
        }
        catch (Exception)
        {
            sprite = null;
        }

        IconCache[token] = sprite;
        return sprite;
    }
}
