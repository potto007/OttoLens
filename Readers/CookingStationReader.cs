using OttoLens.Model;

namespace OttoLens.Readers;

/// Cooking station and oven (spec 3.4). The oven's hover object is one of its two Switch
/// children, which GetComponentInParent resolves to the station; both switches get the same
/// report. Slots are read by hand with cached hashes because GetSlot builds four strings per
/// call and its Status enum is a private nested type.
internal sealed class CookingStationReader : ILensReader
{
    // CookingStation.Status order: NotDone, Done, Burnt.
    private const int StatusDone = 1;
    private const int StatusBurnt = 2;
    private const int MaxSlots = 16;

    // "slot" + i carries both the item name (string map) and the cooked seconds (float map).
    private static readonly int[] SlotKeys = BuildKeys("slot");
    private static readonly int[] SlotStatusKeys = BuildKeys("slotstatus");
    private static readonly Dictionary<string, Sprite?> SpriteCache = new(StringComparer.Ordinal);

    private readonly LensReport _report = new();
    private readonly LensItemBlock _cooking = new();
    private readonly LensItemBlock _done = new();

    public Type TargetType => typeof(CookingStation);

    public bool Enabled => OttoLensPlugin.ShowCooking.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var station = (CookingStation)target;
        ZNetView view = station.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        if (zdo == null)
        {
            return null;
        }

        LensReport report = _report;
        report.Reset();
        _cooking.Clear();
        _done.Clear();

        int slotCount = station.m_slots != null ? Mathf.Min(station.m_slots.Length, MaxSlots) : 0;
        int occupied = 0;
        bool anyNotDone = false;
        bool anyDone = false;
        bool anyBurnt = false;
        float nextRemaining = float.MaxValue;
        float nextTotal = 0f;
        float soonestBurn = float.MaxValue;

        for (int i = 0; i < slotCount; i++)
        {
            string itemName = zdo.GetString(SlotKeys[i]);
            if (itemName.Length == 0)
            {
                continue;
            }

            occupied++;
            float cooked = zdo.GetFloat(SlotKeys[i]);
            int status = zdo.GetInt(SlotStatusKeys[i]);

            if (status == StatusBurnt)
            {
                anyBurnt = true;
                AddSlot(_done, station.m_overCookedItem, itemName);
                continue;
            }

            CookingStation.ItemConversion? conversion = FindConversion(station, itemName);
            if (conversion == null)
            {
                // Not in any conversion: an item from a removed mod or a stale slot.
                AddSlot(_cooking, null, itemName);
                anyNotDone = true;
                continue;
            }

            if (status == StatusDone)
            {
                anyDone = true;
                AddSlot(_done, conversion.m_to, itemName);
                if (station.m_canOvercookItems)
                {
                    soonestBurn = Mathf.Min(soonestBurn, conversion.m_cookTime * 2f - cooked);
                }

                continue;
            }

            anyNotDone = true;
            ItemDrop source = itemName == conversion.m_from.gameObject.name ? conversion.m_from : conversion.m_to;
            AddSlot(_cooking, source, itemName);
            float remaining = conversion.m_cookTime - cooked;
            if (remaining < nextRemaining)
            {
                nextRemaining = remaining;
                nextTotal = conversion.m_cookTime;
            }
        }

        // An empty station without a fuel gauge has nothing worth a panel.
        if (occupied == 0 && !station.m_useFuel)
        {
            return null;
        }

        bool noFire = station.m_requireFire && !station.IsFireLit();
        float fuel = station.m_useFuel ? station.GetFuel() : 0f;
        bool noFuel = station.m_useFuel && fuel <= 0f;

        report.Title = LensFormat.Name(station.m_name);
        if (noFire)
        {
            report.Headline = "NO FIRE";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (noFuel)
        {
            report.Headline = "NO FUEL";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (anyNotDone)
        {
            report.Headline = "COOKING";
            report.HeadlineColor = LensColor.Gold;
        }
        else if (anyDone)
        {
            report.Headline = "READY";
            report.HeadlineColor = LensColor.Good;
        }
        else if (anyBurnt)
        {
            report.Headline = "BURNT";
            report.HeadlineColor = LensColor.Bad;
        }

        LensMeter? next = null;
        if (anyNotDone && nextRemaining < float.MaxValue)
        {
            float remaining = Mathf.Max(nextRemaining, 0f);
            float fraction = nextTotal > 0f ? Mathf.Clamp01(remaining / nextTotal) : 0f;
            next = LensReport.Meter("Next", fraction, LensFormat.Time(remaining), remaining > 0f ? LensColor.Gold : LensColor.Good, drains: true);
        }
        else if (anyDone)
        {
            next = LensReport.Meter("Next", 0f, "Ready", LensColor.Good, drains: true);
        }

        LensMeter? burns = null;
        if (soonestBurn < float.MaxValue)
        {
            string text = soonestBurn > 0f ? LensFormat.Time(soonestBurn) : "Now";
            burns = LensReport.Meter("Burns", null, text, LensColor.Warn, drains: true);
        }

        if (station.m_useFuel)
        {
            int maxFuel = Mathf.Max(station.m_maxFuel, 1);
            float fraction = Mathf.Clamp01(fuel / maxFuel);
            string value = LensFormat.Count(fuel, maxFuel);
            if (fuel > 0f)
            {
                value = value + " " + LensFormat.TimeWithDays(fuel * station.m_secPerFuel);
            }

            string label = station.m_fuelItem != null ? LensFormat.Name(station.m_fuelItem.m_itemData.m_shared.m_name) : "Fuel";
            report.PrimaryMeter = LensReport.Meter(label, fraction, value, LensFormat.Ramp(fraction));
            report.Secondary1 = next;
            report.Secondary2 = burns;
        }
        else
        {
            report.PrimaryMeter = next;
            report.Secondary1 = burns;
        }

        if (_cooking.Items.Count > 0)
        {
            _cooking.Label = "Cooking";
            report.Block0 = _cooking;
        }

        if (_done.Items.Count > 0)
        {
            _done.Label = "Done";
            _done.IsOutput = true;
            report.Block1 = _done;
        }

        if (slotCount > 0)
        {
            report.Footer = LensFormat.Free(slotCount - occupied);
        }

        return report;
    }

    // Slot order is stable, so grouping by first appearance keeps rows steady between ticks.
    private static void AddSlot(LensItemBlock block, ItemDrop? drop, string prefabName)
    {
        List<LensItem> items = block.Items;
        string name = drop != null ? LensFormat.Name(drop.m_itemData.m_shared.m_name) : LensFormat.Name(prefabName);
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Name == name)
            {
                items[i].Count++;
                return;
            }
        }

        items.Add(new LensItem(GetSprite(prefabName, drop), name, 1));
    }

    private static CookingStation.ItemConversion? FindConversion(CookingStation station, string itemName)
    {
        List<CookingStation.ItemConversion> conversions = station.m_conversion;
        if (conversions == null)
        {
            return null;
        }

        for (int i = 0; i < conversions.Count; i++)
        {
            CookingStation.ItemConversion c = conversions[i];
            if (c == null || c.m_from == null || c.m_to == null)
            {
                continue;
            }

            if (c.m_from.gameObject.name == itemName || c.m_to.gameObject.name == itemName)
            {
                return c;
            }
        }

        return null;
    }

    // The cache outlives a world unload: a destroyed sprite reads as Unity null and is fetched
    // again, so a stale entry cannot hand the panel a fake-null sprite that blanks the row art.
    private static Sprite? GetSprite(string prefabName, ItemDrop? drop)
    {
        if (SpriteCache.TryGetValue(prefabName, out Sprite? cached) && cached != null)
        {
            return cached;
        }

        if (drop == null)
        {
            return null;
        }

        Sprite? sprite;
        try
        {
            sprite = drop.m_itemData.GetIcon();
        }
        catch (Exception)
        {
            sprite = null;
        }

        SpriteCache[prefabName] = sprite;
        return sprite;
    }

    private static int[] BuildKeys(string prefix)
    {
        var keys = new int[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
        {
            keys[i] = (prefix + i).GetStableHashCode();
        }

        return keys;
    }
}
