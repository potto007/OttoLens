using OttoLens.Model;

namespace OttoLens.Readers;

/// Fire, hearth, torch, brazier (spec 3.2). Fuel comes straight from the ZDO; the burn
/// clock is fuel times m_secPerFuel, never touched through GetTimeSinceLastUpdate (spec 6.2).
internal sealed class FireplaceReader : ILensReader
{
    private const string FallbackFuelLabel = "Fuel";

    private readonly LensReport _report = new();

    public Type TargetType => typeof(Fireplace);

    public bool Enabled => OttoLensPlugin.ShowFires.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var fire = (Fireplace)target;
        ZNetView view = fire.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        // Vanilla shows nothing for an eternal flame, so neither do we.
        if (fire.m_infiniteFuel)
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        float fuel = Mathf.Max(0f, zdo.GetFloat(ZDOVars.s_fuel));
        float maxFuel = fire.m_maxFuel;
        float? fraction = maxFuel > 0f ? Mathf.Clamp01(fuel / maxFuel) : null;
        LensColor fuelColor = fraction.HasValue ? LensFormat.Ramp(fraction.Value) : LensColor.Dim;

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(fire.m_name);
        report.PrimaryMeter = LensReport.Meter(FuelLabel(fire), fraction, LensFormat.Count(fuel, (int)maxFuel), fuelColor);

        bool switchedOn = zdo.GetInt(ZDOVars.s_state, 1) == 1;
        bool burning = fire.IsBurning();
        if (fuel <= 0f)
        {
            report.Headline = "NO FUEL";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (!switchedOn)
        {
            report.Headline = "OFF";
            report.HeadlineColor = LensColor.Dim;
        }
        else if (!burning)
        {
            // Wet, blocked or submerged: fuel sits in the pit and does not drain.
            report.Headline = "OUT";
            report.HeadlineColor = LensColor.Warn;
        }
        else
        {
            report.Headline = "BURNING";
            report.HeadlineColor = LensColor.Gold;
            if (fire.m_secPerFuel > 0f)
            {
                float seconds = fuel * fire.m_secPerFuel;
                report.Secondary1 = LensReport.Meter("Burns", null, LensFormat.TimeWithDays(seconds), LensColor.Gold, drains: true);
            }
        }

        return report;
    }

    // The fuel item name doubles as the meter label so the row reads "Wood 7/10".
    private static string FuelLabel(Fireplace fire)
    {
        ItemDrop fuelItem = fire.m_fuelItem;
        if (fuelItem == null || fuelItem.m_itemData?.m_shared == null)
        {
            return FallbackFuelLabel;
        }

        string name = LensFormat.Name(fuelItem.m_itemData.m_shared.m_name);
        return name.Length > 0 ? name : FallbackFuelLabel;
    }
}
