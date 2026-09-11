using OttoLens.Model;

namespace OttoLens.Readers;

/// Standing tree (TreeBase) or felled log (TreeLog). Neither is hoverable, but Player's own
/// hover ray still hands us the collider object. One instance per type; both read a float under
/// ZDOVars.s_health (spec 3.12), but their pools differ. TreeLog writes the world level scaled
/// maximum into the ZDO at Awake, so it matches Destructible. TreeBase applies the scaling only
/// in its Awake destroy check: RPC_Damage reads s_health with the unscaled m_health as its
/// default and stores the result, so a standing tree's real pool is m_health.
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
        float max;
        int minToolTier;
        if (_logs)
        {
            var log = (TreeLog)target;
            view = log.m_nview;
            max = DestructibleReader.ScaledMax(log.m_health);
            minToolTier = log.m_minToolTier;
        }
        else
        {
            var tree = (TreeBase)target;
            view = tree.m_nview;
            max = tree.m_health;
            minToolTier = tree.m_minToolTier;
        }

        if (view == null || !view.IsValid())
        {
            return null;
        }

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
