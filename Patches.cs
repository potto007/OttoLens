using OttoLens.Model;
using OttoLens.Readers;
using OttoLens.UI;

namespace OttoLens;

/// The two frame level hooks from spec 1.4 and 5.4. Postfixes only. The panel is the only
/// output; nothing here touches hover text.
[HarmonyPatch]
internal static class Patches
{
    private static readonly HashSet<Type> LoggedExceptions = new();

    // Per frame cache so a still hover costs no component walk (spec 6.4 rule 8).
    private static GameObject? _lastHover;
    private static ILensReader? _lastReader;
    private static Component? _lastTarget;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateCrosshair), typeof(Player), typeof(float))]
    private static void UpdateCrosshairPostfix(Hud __instance, Player player)
    {
        try
        {
            Frame(__instance, player);
        }
        catch (Exception ex)
        {
            LogOnce(ex);
            LensPanel.Instance?.SetTarget(null, null, null);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hud), nameof(Hud.OnDestroy))]
    private static void HudDestroyPostfix()
    {
        try
        {
            _lastHover = null;
            _lastReader = null;
            _lastTarget = null;
            LensPanel.Destroy();
            LensFormat.ClearCaches();
        }
        catch (Exception ex)
        {
            LogOnce(ex);
        }
    }

    private static void Frame(Hud hud, Player player)
    {
        if (!OttoLensPlugin.Enabled.Value || player == null || !hud.IsVisible() || InventoryGui.IsVisible())
        {
            LensPanel.Instance?.SetTarget(null, null, null);
            return;
        }

        GameObject? hover = ResolveHoverObject(player);
        if (hover == null)
        {
            _lastHover = null;
            _lastReader = null;
            _lastTarget = null;
            LensPanel.Instance?.SetTarget(null, null, null);
            return;
        }

        if (hover != _lastHover)
        {
            _lastHover = hover;
            if (LensReaders.Resolve(hover, out ILensReader reader, out Component target))
            {
                _lastReader = reader;
                _lastTarget = target;
            }
            else
            {
                _lastReader = null;
                _lastTarget = null;
            }
        }

        if (_lastReader == null || _lastTarget == null)
        {
            LensPanel.Instance?.SetTarget(null, null, null);
            return;
        }

        // Access and validity are checked every frame: a chest can lose ward access while hovered.
        if (!LensReaders.PassesCoreGuards(_lastTarget, hud.m_hoverName.text))
        {
            LensPanel.Instance?.SetTarget(null, null, null);
            return;
        }

        LensPanel? panel = LensPanel.Get();
        panel?.SetTarget(_lastTarget, _lastReader, hover);
    }

    /// Spec 1.5 and 3.11: interact hover first, then the build raycast piece in place mode,
    /// then the gated extra raycast for the tree group (spec 3.12 option one).
    private static GameObject? ResolveHoverObject(Player player)
    {
        GameObject? hover = player.GetHoverObject();
        if (hover != null)
        {
            return hover;
        }

        if (player.InPlaceMode())
        {
            if (!OttoLensPlugin.ShowBuildPieces.Value)
            {
                return null;
            }

            Piece? piece = player.GetHoveringPiece();
            return piece != null ? piece.gameObject : null;
        }

        if (!OttoLensPlugin.ShowTreesAndRocks.Value)
        {
            return null;
        }

        return TreeRaycast(player);
    }

    /// One ray, same origin, mask and distance rule as Player.FindHoverObject. Only runs when
    /// the vanilla hover is null and the tree group is on.
    private static GameObject? TreeRaycast(Player player)
    {
        GameCamera? camera = GameCamera.instance;
        if (camera == null || player.m_eye == null)
        {
            return null;
        }

        Transform cam = camera.transform;
        if (!Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 50f, player.m_interactMask))
        {
            return null;
        }

        if (Vector3.Distance(player.m_eye.position, hit.point) >= player.m_maxInteractDistance)
        {
            return null;
        }

        Rigidbody body = hit.collider.attachedRigidbody;
        if (body != null && body.gameObject == player.gameObject)
        {
            return null;
        }

        return hit.collider.gameObject;
    }

    private static void LogOnce(Exception ex)
    {
        if (LoggedExceptions.Add(ex.GetType()))
        {
            OttoLensPlugin.Log.LogWarning($"OttoLens hook failed with {ex.GetType().Name}: {ex.Message}. Further {ex.GetType().Name} reports are suppressed.\n{ex.StackTrace}");
        }
    }
}
