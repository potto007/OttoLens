using OttoLens.Model;

namespace OttoLens.Readers;

/// Build piece condition (spec 3.10 and 3.11). Registered last: WearNTear sits on almost every
/// piece, so any more specific reader on the same object must win first. The core hands the
/// piece over in place mode too (player.GetHoveringPiece()), so one reader covers both modes.
internal sealed class WearNTearReader : ILensReader
{
    private readonly LensReport _report = new();

    public Type TargetType => typeof(WearNTear);

    // Off never reads; WithHammer is the default because a plain wall is a hover target in its
    // own right (Player.FindHoverObject takes the first hit collider, Hoverable or not), so
    // Always would pop a panel on every wall the player walks past.
    public bool Enabled => OttoLensPlugin.ShowBuildPieces.Value switch
    {
        OttoLensPlugin.PieceStatus.Always => true,
        OttoLensPlugin.PieceStatus.WithHammer => Player.m_localPlayer != null && Player.m_localPlayer.InPlaceMode(),
        _ => false,
    };

    public LensReport? Read(Component target, GameObject hover)
    {
        var wear = (WearNTear)target;
        ZNetView view = wear.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ZDO? zdo = view.GetZDO();
        if (zdo == null)
        {
            return null;
        }

        float maxHealth = wear.m_health;
        if (maxHealth <= 0f)
        {
            return null;
        }

        // The ZDO is the value the vanilla piece bar is driven from; reading it directly keeps
        // remote pieces as fresh as the owner's last tick without waiting for the broadcast.
        float health = Mathf.Clamp(zdo.GetFloat(ZDOVars.s_health, maxHealth), 0f, maxHealth);
        float healthFraction = Mathf.Clamp01(health / maxHealth);
        bool showHealth = healthFraction < 1f || OttoLensPlugin.ShowFullHealth.Value;

        // Support only means something on pieces that lose health without it; the others keep
        // the material maximum forever and would read a flat 100 percent.
        LensMeter? support = null;
        if (wear.m_noSupportWear)
        {
            float maxSupport = wear.GetMaxSupport();
            if (maxSupport > 0f)
            {
                float supportFraction = Mathf.Clamp01(wear.GetSupport() / maxSupport);
                support = LensReport.Meter("Support", supportFraction, LensFormat.Percent(supportFraction), LensFormat.Ramp(supportFraction));
            }
        }

        if (!showHealth && !support.HasValue)
        {
            return null;
        }

        LensReport report = _report;
        report.Reset();
        report.Title = TitleFor(wear, hover);

        if (showHealth)
        {
            LensColor color = LensFormat.Ramp(healthFraction);
            report.Headline = WearWord(healthFraction);
            report.HeadlineColor = color;
            report.PrimaryMeter = LensReport.Meter("Health", healthFraction, LensFormat.Percent(healthFraction), color);
            report.Secondary1 = support;
        }
        else
        {
            report.PrimaryMeter = support;
        }

        // Shape-only signature: structural changes (target swap, row presence) drive Rebuild;
        // per-tick values (percent, wear word) update through Tick without layout work.
        report.ContentSignature = string.Concat(report.Title, "|", showHealth ? "h" : "", "|", support.HasValue ? "s" : "");

        return report;
    }

    // Spec 3.10: new above 0.75, worn above 0.25, broken at or below 0.25.
    private static string WearWord(float healthFraction)
    {
        if (healthFraction > 0.75f) return "New";
        if (healthFraction > 0.25f) return "Worn";
        return "Broken";
    }

    private static string TitleFor(WearNTear wear, GameObject hover)
    {
        Piece? piece = wear.m_piece != null ? wear.m_piece : wear.GetComponent<Piece>();
        if (piece != null && !string.IsNullOrEmpty(piece.m_name))
        {
            return LensFormat.Name(piece.m_name);
        }

        string name = hover.name;
        int clone = name.IndexOf("(Clone)", StringComparison.Ordinal);
        return clone > 0 ? name.Substring(0, clone) : name;
    }
}
