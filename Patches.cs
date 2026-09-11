using System.Linq;
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
    // Part of the cache key: place mode picks a different resolution rule (the WearNTear
    // bypass below), and the hover object can be the same reference on both sides of the
    // switch, so the mode has to invalidate the cache on its own.
    private static bool _lastPlaceMode;
    private static ILensReader? _lastReader;
    private static Component? _lastTarget;
    // Cached once after LensReaders.Register() runs; used by the place-mode bypass.
    private static ILensReader? _wearNTearReader;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateCrosshair), typeof(Player), typeof(float))]
    private static void UpdateCrosshairPostfix(Hud __instance, Player player)
    {
        try
        {
            // Spec 4.2 General item 2: ToggleKey flips the master switch at runtime.
            // UnityEngine.Input ignores UI focus and Hud.UpdateCrosshair keeps running while a
            // text field owns the keyboard, so an unguarded read of the default H binding fires
            // on every 'h' typed in chat, on a sign, in a rename box or in the console.
            KeyCode key = OttoLensPlugin.ToggleKey.Value;
            if (key != KeyCode.None && !KeyboardIsBusy() && Input.GetKeyDown(key))
            {
                OttoLensPlugin.Enabled.Value = !OttoLensPlugin.Enabled.Value;
            }

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
            Teardown();
        }
        catch (Exception ex)
        {
            LogOnce(ex);
        }
    }

    /// Full teardown: the panel, the per frame cache and every memoized world or language
    /// dependent string. Runs on Hud.OnDestroy (world unload) and again from the plugin's
    /// OnDestroy, which is the only path left once the patches above are gone.
    internal static void Teardown()
    {
        _lastHover = null;
        _lastPlaceMode = false;
        _lastReader = null;
        _lastTarget = null;
        LensPanel.Release();
        LensFormat.ClearCaches();
        LensReaders.ClearLocalizedCaches();
        ContainerReader.ClearOpenedChests();
    }

    /// Spec 5.4 item 4: record an opened world chest so ContainerReader can reveal its contents.
    /// The container argument is null when the player opens only their own inventory; guard it.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
    private static void InventoryGuiShowPostfix(Container container)
    {
        try
        {
            if (container == null)
            {
                return;
            }
            Piece? piece = container.m_piece;
            if (piece != null && piece.IsPlacedByPlayer())
            {
                return;
            }
            ContainerReader.RecordOpen(container);
        }
        catch (Exception ex)
        {
            LogOnce(ex);
        }
    }

    /// Drops the resolve cache so the next frame runs LensReaders.Resolve again. Plugin calls
    /// this whenever a Targets setting changes: the group gates are read only at resolve time
    /// and the resolve is keyed on the hover object, so a group turned back on while the
    /// crosshair still rests on the same object would otherwise not show until the player looks
    /// away and back - and a lower priority reader that claimed the object in the meantime
    /// (WearNTear under a chest with containers off) would keep the panel for itself.
    internal static void InvalidateHoverCache()
    {
        _lastHover = null;
        _lastReader = null;
        _lastTarget = null;
    }

    private static void Frame(Hud hud, Player player)
    {
        if (!OttoLensPlugin.Enabled.Value || player == null || !hud.IsVisible() || InventoryGui.IsVisible())
        {
            LensPanel.Instance?.SetTarget(null, null, null);
            return;
        }

        bool placeMode = player.InPlaceMode();
        GameObject? hover = ResolveHoverObject(player, placeMode);
        if (hover == null)
        {
            _lastHover = null;
            _lastPlaceMode = placeMode;
            _lastReader = null;
            _lastTarget = null;
            LensPanel.Instance?.SetTarget(null, null, null);
            return;
        }

        // The mode is part of the key: a single collider piece resolves to the same GameObject
        // through GetHoverObject and through GetHoveringPiece, so taking the hammer out or
        // putting it away without moving the crosshair would otherwise keep the stale reader.
        if (hover != _lastHover || placeMode != _lastPlaceMode)
        {
            _lastHover = hover;
            _lastPlaceMode = placeMode;
            if (placeMode)
            {
                // Spec 3.11 fix: in place mode bypass LensReaders.Resolve so that
                // WearNTearReader always wins over a more-specific reader (ContainerReader,
                // FireplaceReader, etc.) that would otherwise claim a piece that also
                // carries Container, Fireplace, or another multi-reader component.
                // Spec 3.11 item 2: a piece with no WearNTear is indestructible - show nothing.
                Piece? piece = player.GetHoveringPiece();
                WearNTear? wnt = piece?.GetComponentInParent<WearNTear>();
                _wearNTearReader ??= LensReaders.All.FirstOrDefault(r => r is WearNTearReader);
                if (wnt != null && _wearNTearReader != null)
                {
                    _lastReader = _wearNTearReader;
                    _lastTarget = wnt;
                }
                else
                {
                    _lastReader = null;
                    _lastTarget = null;
                }
            }
            else if (LensReaders.Resolve(hover, out ILensReader reader, out Component target))
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

        // Re-check the group gate every frame. LensReaders.Resolve only runs when the hover
        // object changes, so a gate that closes on the cached reader has to be caught here.
        // Config flips come through InvalidateHoverCache; this also covers a gate whose value
        // depends on game state (WearNTearReader under WithHammer). Clearing _lastHover forces
        // a full re-resolve next frame.
        if (_lastReader != null && !_lastReader.Enabled)
        {
            _lastHover = null;
            _lastPlaceMode = placeMode;
            _lastReader = null;
            _lastTarget = null;
            LensPanel.Instance?.SetTarget(null, null, null);
            return;
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

    /// Spec 1.5 and 3.11: interact hover first, then the build raycast piece in place mode.
    ///
    /// No extra raycast for the tree group (spec 3.12 option one). Player.FindHoverObject
    /// assigns m_hovering for the nearest in-range hit whether or not anything on it
    /// implements Hoverable, so a tree, log or destructible already arrives through
    /// GetHoverObject and TreeReader/DestructibleReader pick it up from there. A second ray
    /// with the same origin, mask and distance rule can only return an object when vanilla
    /// deliberately cleared the hover - while dead or steering a doodad (Player.UpdateHover) -
    /// which is exactly where the panel must stay silent.
    private static GameObject? ResolveHoverObject(Player player, bool placeMode)
    {
        GameObject? hover = player.GetHoverObject();
        if (hover != null)
        {
            return hover;
        }

        if (!placeMode || OttoLensPlugin.ShowBuildPieces.Value == OttoLensPlugin.PieceStatus.Off)
        {
            return null;
        }

        Piece? piece = player.GetHoveringPiece();
        return piece != null ? piece.gameObject : null;
    }

    /// True while a text field or a modal menu owns the keyboard. A raw UnityEngine.Input read
    /// bypasses every vanilla input gate, so any OttoLens binding has to check this first.
    private static bool KeyboardIsBusy()
        => global::Console.IsVisible()
           || TextInput.IsVisible()
           || Minimap.InTextInput()
           || Menu.IsVisible()
           || (Chat.instance != null && Chat.instance.HasFocus())
           // The build menu search box owns the keyboard while the piece selection UI is open,
           // and Hud.Update calls UpdateCrosshair through that whole time. Vanilla guards its own
           // bindings the same way (Minimap.UpdateInput tests m_buildUi.SearchFieldFocused).
           || (Hud.instance != null && Hud.instance.m_buildUi != null && Hud.instance.m_buildUi.SearchFieldFocused);

    private static void LogOnce(Exception ex)
    {
        if (LoggedExceptions.Add(ex.GetType()))
        {
            OttoLensPlugin.Log.LogWarning($"OttoLens hook failed with {ex.GetType().Name}: {ex.Message}. Further {ex.GetType().Name} reports are suppressed.\n{ex.StackTrace}");
        }
    }
}
