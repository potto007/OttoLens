using OttoLens.Model;

namespace OttoLens.Readers;

/// Crop and sapling progress (spec 3.8). Status comes from the plant's own slow update;
/// vanilla recomputes it every ten seconds and the reader never recomputes it.
internal sealed class PlantReader : ILensReader
{
    private readonly LensReport _report = new();

    public Type TargetType => typeof(Plant);

    public bool Enabled => OttoLensPlugin.ShowPlants.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var plant = (Plant)target;
        ZNetView view = plant.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        Plant.Status status = plant.GetStatus();
        // Both are read only (spec 6.2). GetGrowTime restores the global random state itself.
        float growTime = plant.GetGrowTime();
        float elapsed = (float)plant.TimeSincePlanted();
        float fraction = growTime > 0f ? Mathf.Clamp01(elapsed / growTime) : 1f;
        float remaining = growTime - elapsed;

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(plant.m_name);

        if (status == Plant.Status.Healthy)
        {
            bool ready = remaining <= 0f;
            report.Headline = ready ? "READY" : "GROWING";
            report.HeadlineColor = ready ? LensColor.Good : LensColor.Gold;
            report.PrimaryMeter = LensReport.Meter(
                "Grown",
                fraction,
                LensFormat.TimeWithDays(remaining),
                ready ? LensColor.Good : LensColor.Gold,
                drains: true);
            return report;
        }

        // An unhealthy plant does not finish, so no countdown: progress only.
        report.Headline = StatusWord(status);
        report.HeadlineColor = LensColor.Warn;
        report.PrimaryMeter = LensReport.Meter("Grown", fraction, LensFormat.Percent(fraction), LensColor.Bad);
        return report;
    }

    private static string StatusWord(Plant.Status status) => status switch
    {
        Plant.Status.NoSun => "NO SUN",
        Plant.Status.NoSpace => "NO SPACE",
        Plant.Status.WrongBiome => "WRONG BIOME",
        Plant.Status.NotCultivated => "NOT CULTIVATED",
        Plant.Status.NoAttachPiece => "NO WALL",
        Plant.Status.TooHot => "TOO HOT",
        Plant.Status.TooCold => "TOO COLD",
        _ => "NOT GROWING",
    };
}
