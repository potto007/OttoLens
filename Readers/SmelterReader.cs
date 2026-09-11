using OttoLens.Model;

namespace OttoLens.Readers;

/// Smelter, kiln, blast furnace and windmill fed smelter (spec 3.3). The hover object is one
/// of the Switch children, so the registry resolves the Smelter through the parent walk.
/// Every figure is read straight from the ZDO; GetDeltaTime and UpdateState write to it and
/// are never called here (spec 6.2).
internal sealed class SmelterReader : ILensReader
{
    private readonly LensReport _report = new();
    private readonly LensItemBlock _queue = new();
    private readonly LensItemBlock _output = new();

    // Distinct ore rows in queue order, keyed by prefab name; reused every tick.
    private readonly List<string> _oreOrder = new();
    private readonly Dictionary<string, int> _oreCounts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Sprite?> _sprites = new(StringComparer.Ordinal);

    // Queue entry keys "item0", "item1", ... hashed once and grown on demand.
    private static int[] _itemKeys = { ZDOVars.s_item0 };

    public Type TargetType => typeof(Smelter);

    public bool Enabled => OttoLensPlugin.ShowSmelters.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var smelter = (Smelter)target;
        ZNetView view = smelter.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        if (zdo == null)
        {
            return null;
        }

        float fuel = zdo.GetFloat(ZDOVars.s_fuel);
        int queued = zdo.GetInt(ZDOVars.s_queued);
        float bakeTimer = zdo.GetFloat(ZDOVars.s_bakeTimer);
        int outputCount = zdo.GetInt(ZDOVars.s_spawnAmount);
        string outputOre = zdo.GetString(ZDOVars.s_spawnOre);

        int maxFuel = smelter.m_maxFuel;
        int maxOre = smelter.m_maxOre;
        bool needsRoof = smelter.m_requiresRoof && !smelter.m_haveRoof;
        bool active = smelter.IsActive();

        // UpdateSmelter scales its bake step by the windmill power output, so a becalmed or
        // covered mill advances neither the bake timer nor the fuel. IsActive() does not look
        // at wind, so the stop has to be detected here or the countdowns would count nothing.
        float power = 1f;
        bool becalmed = false;
        if (smelter.m_windmill != null)
        {
            power = smelter.m_windmill.GetPowerOutput();
            becalmed = power <= 0f;
        }

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(smelter.m_name);

        // Headline: the reason it is not running wins over the running word.
        if (active)
        {
            report.Headline = becalmed ? "NO WIND" : "SMELTING";
            report.HeadlineColor = becalmed ? LensColor.Warn : LensColor.Gold;
        }
        else if (maxFuel > 0 && fuel <= 0f)
        {
            report.Headline = "NO FUEL";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (maxOre > 0 && queued <= 0)
        {
            report.Headline = "NO ORE";
            report.HeadlineColor = LensColor.Dim;
        }
        else if (needsRoof)
        {
            report.Headline = "NO ROOF";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (smelter.m_blockedSmoke)
        {
            report.Headline = "SMOKE BLOCKED";
            report.HeadlineColor = LensColor.Warn;
        }

        // Primary: fuel when the piece burns any; a fuel free piece promotes the queue.
        LensMeter? queueMeter = null;
        if (maxOre > 0)
        {
            float queueFraction = Mathf.Clamp01(queued / (float)maxOre);
            queueMeter = LensReport.Meter("Ore", queueFraction, LensFormat.Count(queued, maxOre), LensColor.Dim);
        }

        if (maxFuel > 0)
        {
            float fuelFraction = Mathf.Clamp01(fuel / maxFuel);
            string fuelLabel = smelter.m_fuelItem != null
                ? LensFormat.Name(smelter.m_fuelItem.m_itemData.m_shared.m_name)
                : "Fuel";
            report.PrimaryMeter = LensReport.Meter(fuelLabel, fuelFraction, LensFormat.Count(fuel, maxFuel), LensFormat.Ramp(fuelFraction));
        }
        else
        {
            report.PrimaryMeter = queueMeter;
        }

        // Countdowns only while the owner is ticking the bake timer (spec 3.3 edge cases).
        // A becalmed windmill smelter is one of those stops: no figure is honest there.
        if (active && !becalmed && queued > 0 && smelter.m_secPerProduct > 0f)
        {
            float nextSeconds = Mathf.Max(0f, smelter.m_secPerProduct - bakeTimer) / power;
            float queueSeconds = Mathf.Max(0f, queued * smelter.m_secPerProduct - bakeTimer) / power;

            // Fuel runs out before the queue empties: the stop time is the fuel time.
            bool fuelLimited = false;
            if (maxFuel > 0 && smelter.m_fuelPerProduct > 0)
            {
                float fuelSeconds = fuel * (smelter.m_secPerProduct / smelter.m_fuelPerProduct) / power;
                if (fuelSeconds < queueSeconds)
                {
                    queueSeconds = fuelSeconds;
                    fuelLimited = true;
                }
            }

            report.Secondary1 = LensReport.Meter("Next", null, LensFormat.Time(nextSeconds), LensColor.Gold, drains: true);
            report.Secondary2 = LensReport.Meter("Done", null, LensFormat.TimeWithDays(queueSeconds), fuelLimited ? LensColor.Warn : LensColor.Gold, drains: true);
        }

        FillQueue(smelter, zdo, queued);
        if (_queue.Items.Count > 0)
        {
            _queue.Label = "Queue";
            report.Block0 = _queue;
        }

        if (outputCount > 0)
        {
            _output.Label = "Output";
            _output.IsOutput = true;
            // Vanilla Spawn resolves the product with GetItemConversion, where a null m_from
            // is a wildcard, so the output row follows the same rule.
            Smelter.ItemConversion? conversion = FindConversion(smelter, outputOre, allowWildcard: true);
            ItemDrop? product = conversion?.m_to;
            string name = product != null ? LensFormat.Name(product.m_itemData.m_shared.m_name) : LensFormat.Name(outputOre);
            _output.Items.Add(new LensItem(GetSprite(outputOre, product), name, outputCount));
            report.Block1 = _output;
        }

        if (maxOre > 0)
        {
            report.Footer = LensFormat.Free(Mathf.Max(0, maxOre - queued));
        }

        return report.HasContent ? report : null;
    }

    // One row per distinct ore, first appearance order. An empty entry means the stored
    // count and the stored names disagree; it is skipped rather than shown as an item.
    private void FillQueue(Smelter smelter, ZDO zdo, int queued)
    {
        _oreOrder.Clear();
        _oreCounts.Clear();
        for (int i = 0; i < queued; i++)
        {
            string ore = zdo.GetString(ItemKey(i));
            if (ore.Length == 0)
            {
                continue;
            }

            if (_oreCounts.TryGetValue(ore, out int count))
            {
                _oreCounts[ore] = count + 1;
            }
            else
            {
                _oreCounts[ore] = 1;
                _oreOrder.Add(ore);
            }
        }

        for (int i = 0; i < _oreOrder.Count; i++)
        {
            string ore = _oreOrder[i];
            Smelter.ItemConversion? conversion = FindConversion(smelter, ore, allowWildcard: false);
            ItemDrop? source = conversion?.m_from;
            string name = source != null ? LensFormat.Name(source.m_itemData.m_shared.m_name) : LensFormat.Name(ore);
            _queue.Items.Add(new LensItem(GetSprite(ore, source), name, _oreCounts[ore]));
        }
    }

    // Mirrors Smelter.GetItemConversion (first match in list order). Vanilla treats a null
    // m_from as a wildcard; queue rows disable that because a wildcard conversion cannot
    // describe the ore itself (its sprite and name would be wrong), output rows keep it
    // because that is how vanilla picks the product.
    private static Smelter.ItemConversion? FindConversion(Smelter smelter, string prefabName, bool allowWildcard)
    {
        List<Smelter.ItemConversion> conversions = smelter.m_conversion;
        for (int i = 0; i < conversions.Count; i++)
        {
            Smelter.ItemConversion conversion = conversions[i];
            if (conversion.m_from == null)
            {
                if (allowWildcard)
                {
                    return conversion;
                }

                continue;
            }

            if (conversion.m_from.gameObject.name == prefabName)
            {
                return conversion;
            }
        }

        return null;
    }

    // The cache outlives a world unload: a destroyed sprite reads as Unity null and is fetched
    // again, so a stale entry cannot hand the panel a fake-null sprite that blanks the row art.
    private Sprite? GetSprite(string prefabName, ItemDrop? drop)
    {
        if (_sprites.TryGetValue(prefabName, out Sprite? cached) && cached != null)
        {
            return cached;
        }

        Sprite? sprite = null;
        if (drop != null)
        {
            try
            {
                sprite = drop.m_itemData.GetIcon();
            }
            catch (Exception)
            {
                sprite = null;
            }
        }

        _sprites[prefabName] = sprite;
        return sprite;
    }

    private static int ItemKey(int index)
    {
        int[] keys = _itemKeys;
        if (index >= keys.Length)
        {
            var grown = new int[Mathf.Max(index + 1, keys.Length * 2)];
            Array.Copy(keys, grown, keys.Length);
            for (int i = keys.Length; i < grown.Length; i++)
            {
                grown[i] = ("item" + i).GetStableHashCode();
            }

            _itemKeys = grown;
            keys = grown;
        }

        return keys[index];
    }
}
