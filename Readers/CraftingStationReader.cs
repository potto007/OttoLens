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

    // Grouping cache, keyed on the ZDO revision plus the item count (the inventory can
    // deserialize up to a second after the revision moves).
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
        else if (station.m_craftRequireRoof && station.m_roofCheckPoint != null && !station.CheckUsable(player, showMessage: false))
        {
            report.Headline = "NO ROOF";
            report.HeadlineColor = LensColor.Bad;
        }
        else
        {
            report.Headline = "READY";
            report.HeadlineColor = LensColor.Good;
        }

        int level = station.GetLevel();
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
        uint revision = zdo.DataRevision;
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

        _groups.Sort(CompareGroups);
    }

    private static int CompareGroups(LensItem a, LensItem b)
    {
        int byCount = b.Count.CompareTo(a.Count);
        return byCount != 0 ? byCount : string.CompareOrdinal(a.Name, b.Name);
    }

    private static Sprite? Icon(ItemDrop.ItemData item, string token)
    {
        if (IconCache.TryGetValue(token, out Sprite? cached))
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
