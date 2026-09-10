using System.Text;
using OttoLens.Model;

namespace OttoLens.Readers;

/// Generic destructible (small rocks, stumps, roots, debris). Spec 3.12: health lives under
/// ZDOVars.s_health with the world level scaled maximum as the default. Registered last in the
/// harvest group because Destructible also sits on many more specific prefabs.
internal sealed class DestructibleReader : ILensReader
{
    private readonly LensReport _report = new();

    public Type TargetType => typeof(Destructible);

    public bool Enabled => OttoLensPlugin.ShowTreesAndRocks.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var destructible = (Destructible)target;
        ZNetView view = destructible.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        float max = ScaledMax(destructible.m_health);
        float current = view.GetZDO().GetFloat(ZDOVars.s_health, max);
        if (max <= 0f || current <= 0f)
        {
            return null;
        }

        LensReport report = _report;
        report.Reset();
        report.Title = TitleFor(destructible.gameObject);
        report.PrimaryMeter = HealthMeter(current, max);
        report.Secondary1 = ToolTierMeter(destructible.m_minToolTier);
        return report;
    }

    // Shared by the tree and rock readers: same scaling, same rows, same title fallback.

    /// Vanilla's ZDO default: m_health plus the world level mine bonus.
    internal static float ScaledMax(float baseHealth)
        => baseHealth + Game.m_worldLevel * baseHealth * Game.instance.m_worldLevelMineHPMultiplier;

    internal static LensMeter HealthMeter(float current, float max)
    {
        float fraction = Mathf.Clamp01(current / max);
        return LensReport.Meter("Health", fraction, LensFormat.Percent(fraction), LensFormat.Ramp(fraction));
    }

    /// Tool tier row. Hidden for tier zero targets, so the common case stays one row; Bad
    /// when the held tool cannot damage the target at all (vanilla shows "too hard").
    internal static LensMeter? ToolTierMeter(int minToolTier)
    {
        if (minToolTier <= 0)
        {
            return null;
        }

        int held = HeldToolTier();
        if (held < minToolTier)
        {
            string value = held < 0 ? $"{minToolTier} needed" : $"{minToolTier} needed, holding {held}";
            return LensReport.Meter("Tool tier", null, value, LensColor.Bad);
        }

        return LensReport.Meter("Tool tier", null, minToolTier.ToString(), LensColor.Dim);
    }

    /// Tier of the item vanilla would swing (Humanoid.GetCurrentWeapon), -1 when none.
    private static int HeldToolTier()
    {
        Player? player = Player.m_localPlayer;
        ItemDrop.ItemData? weapon = player != null ? player.GetCurrentWeapon() : null;
        return weapon?.m_shared?.m_toolTier ?? -1;
    }

    private static readonly Dictionary<string, string> TitleCache = new(StringComparer.Ordinal);
    private static readonly StringBuilder TitleBuilder = new(32);

    /// Trees, logs and plain destructibles carry no display name, so the prefab name stands
    /// in: a Piece name when one exists, else "Beech1" becomes "Beech" and "_" becomes a space.
    internal static string TitleFor(GameObject go)
    {
        Piece? piece = go.GetComponent<Piece>();
        if (piece != null && !string.IsNullOrEmpty(piece.m_name))
        {
            return LensFormat.Name(piece.m_name);
        }

        string prefab = Utils.GetPrefabName(go);
        if (TitleCache.TryGetValue(prefab, out string cached))
        {
            return cached;
        }

        StringBuilder sb = TitleBuilder;
        sb.Clear();
        foreach (string word in prefab.Split('_'))
        {
            string trimmed = word.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            sb.Append(trimmed);
        }

        string title = sb.Length > 0 ? sb.ToString() : prefab;
        TitleCache[prefab] = title;
        return title;
    }
}
