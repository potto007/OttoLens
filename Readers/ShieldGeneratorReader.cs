using System.Globalization;
using OttoLens.Model;

namespace OttoLens.Readers;

/// Shield generator (spec 3.18). Primary meter is fuel over the maximum labelled with the fuel
/// item name, Next is the time until the pulse attack is charged, and the dome radius rides as
/// a bare value. Fuel and the charge start are read straight from the ZDO.
internal sealed class ShieldGeneratorReader : ILensReader
{
    private readonly LensReport _report = new();

    public Type TargetType => typeof(ShieldGenerator);

    public bool Enabled => OttoLensPlugin.ShowMisc.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var generator = (ShieldGenerator)target;
        ZNetView view = generator.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ZDO? zdo = view.GetZDO();
        if (zdo == null)
        {
            return null;
        }

        int maxFuel = generator.m_maxFuel;
        float fuel = Mathf.Max(zdo.GetFloat(ZDOVars.s_fuel, generator.m_defaultFuel), 0f);
        float ratio = maxFuel > 0 ? Mathf.Clamp01(fuel / maxFuel) : 0f;

        string fuelLabel = "Fuel";
        List<ItemDrop> fuelItems = generator.m_fuelItems;
        if (fuelItems != null && fuelItems.Count > 0 && fuelItems[0] != null)
        {
            fuelLabel = LensFormat.Name(fuelItems[0].m_itemData.m_shared.m_name);
        }

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(generator.m_name);
        report.PrimaryMeter = LensReport.Meter(fuelLabel, maxFuel > 0 ? ratio : (float?)null, LensFormat.Count(fuel, maxFuel), LensFormat.Ramp(ratio));

        if (fuel <= 0f)
        {
            report.Headline = "NO FUEL";
            report.HeadlineColor = LensColor.Bad;
        }
        else
        {
            report.Headline = "ACTIVE";
            report.HeadlineColor = LensColor.Good;
        }

        // The attack timer only starts once the tank is full; a zero start means it is waiting.
        if (generator.m_enableAttack)
        {
            long startTicks = zdo.GetLong(ZDOVars.s_startTime, 0L);
            if (startTicks > 0 && ZNet.instance != null)
            {
                float chargeTime = generator.m_attackChargeTime;
                float elapsed = (float)(ZNet.instance.GetTime() - new DateTime(startTicks)).TotalSeconds;
                float remaining = chargeTime - elapsed;
                // The row drains, so the bar carries the remaining fraction and falls with the text.
                float fraction = chargeTime > 0f ? Mathf.Clamp01(Mathf.Max(remaining, 0f) / chargeTime) : 0f;
                bool ready = remaining <= 0f;
                report.Secondary1 = LensReport.Meter("Next", fraction, LensFormat.Time(remaining), ready ? LensColor.Good : LensColor.Gold, drains: true);
                if (ready && fuel > 0f)
                {
                    report.Headline = "READY";
                    report.HeadlineColor = LensColor.Good;
                }
            }
        }

        float radius = generator.m_minShieldRadius + ratio * (generator.m_maxShieldRadius - generator.m_minShieldRadius);
        report.Secondary2 = LensReport.Meter("Radius", null, string.Format(CultureInfo.InvariantCulture, "{0:0} m", radius), LensColor.Dim);
        return report;
    }
}
