using OttoLens.Model;

namespace OttoLens.Readers;

/// Wisp lure (spec 3.17). Headline is the vanilla status string on its own line, the primary
/// meter is wisps present over the spawn cap, and the spawn chance rides as a bare value.
internal sealed class WispSpawnerReader : ILensReader
{
    private readonly LensReport _report = new();

    public Type TargetType => typeof(WispSpawner);

    public bool Enabled => OttoLensPlugin.ShowMisc.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var spawner = (WispSpawner)target;
        ZNetView view = spawner.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        if (spawner.m_spawnPoint == null)
        {
            return null;
        }

        // GetStatus is what the vanilla hover calls; it caches its answer for four seconds and
        // touches no ZDO, so it stays cheap at the refresh rate.
        WispSpawner.Status status = spawner.GetStatus();
        string statusToken;
        LensColor color;
        switch (status)
        {
            case WispSpawner.Status.NoSpace:
                statusToken = spawner.m_noSpaceString;
                color = LensColor.Bad;
                break;
            case WispSpawner.Status.TooBright:
                statusToken = spawner.m_tooMuchLightString;
                color = LensColor.Warn;
                break;
            case WispSpawner.Status.Full:
                statusToken = spawner.m_fullString;
                color = LensColor.Gold;
                break;
            default:
                statusToken = spawner.m_wispsAreComingString;
                color = LensColor.Good;
                break;
        }

        int max = spawner.m_maxSpawned;
        int present = LuredWisp.GetWispsInArea(spawner.m_spawnPoint.position, spawner.m_maxSpawnedArea);
        float fraction = max > 0 ? Mathf.Clamp01((float)present / max) : 0f;

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(spawner.m_name);
        report.Headline = LensFormat.Upper(LensFormat.Name(statusToken));
        report.HeadlineColor = color;
        report.PrimaryMeter = LensReport.Meter("Wisps", max > 0 ? fraction : (float?)null, LensFormat.Count(present, max), color);
        report.Secondary1 = LensReport.Meter("Chance", null, LensFormat.Percent(spawner.m_spawnChance), LensColor.Dim);
        return report;
    }
}
