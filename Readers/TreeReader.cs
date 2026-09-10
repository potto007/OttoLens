using OttoLens.Model;

namespace OttoLens.Readers;

/// Standing tree (TreeBase) or felled log (TreeLog). Neither is hoverable, so the core's tree
/// raycast hands us the collider object. One instance per type; both read the same shape:
/// a float under ZDOVars.s_health defaulting to the world level scaled maximum (spec 3.12).
internal sealed class TreeReader : ILensReader
{
    private readonly LensReport _report = new();
    private readonly bool _logs;

    /// logs = false reads TreeBase, logs = true reads TreeLog.
    public TreeReader(bool logs)
    {
        _logs = logs;
    }

    public Type TargetType => _logs ? typeof(TreeLog) : typeof(TreeBase);

    public bool Enabled => OttoLensPlugin.ShowTreesAndRocks.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        ZNetView view;
        float baseHealth;
        int minToolTier;
        if (_logs)
        {
            var log = (TreeLog)target;
            view = log.m_nview;
            baseHealth = log.m_health;
            minToolTier = log.m_minToolTier;
        }
        else
        {
            var tree = (TreeBase)target;
            view = tree.m_nview;
            baseHealth = tree.m_health;
            minToolTier = tree.m_minToolTier;
        }

        if (view == null || !view.IsValid())
        {
            return null;
        }

        float max = DestructibleReader.ScaledMax(baseHealth);
        float current = view.GetZDO().GetFloat(ZDOVars.s_health, max);
        if (max <= 0f || current <= 0f)
        {
            return null;
        }

        LensReport report = _report;
        report.Reset();
        report.Title = DestructibleReader.TitleFor(target.gameObject);
        report.PrimaryMeter = DestructibleReader.HealthMeter(current, max);
        report.Secondary1 = DestructibleReader.ToolTierMeter(minToolTier);
        return report;
    }
}
