using BepInEx.Configuration;
using BepInEx.Logging;
using OttoLens.Readers;
using OttoLens.UI;

namespace OttoLens;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class OttoLensPlugin : BaseUnityPlugin
{
    internal const string ModName = "OttoLens";
    internal const string ModVersion = "1.0.0";
    internal const string Author = "potto007";
    internal const string ModGUID = $"{Author}.{ModName}";

    internal static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource(ModName);
    private readonly Harmony _harmony = new(ModGUID);

    private const string PanelSection = "OttoLens";
    private const string TargetSection = "Targets";

    public enum SortRows
    {
        Count,
        Slot,
    }

    // Spec 4.2 Targets item 13: build piece status is three state, not a toggle.
    public enum PieceStatus
    {
        Off,
        WithHammer,
        Always,
    }

    // Panel knobs, design section 7. Clamps are enforced by the AcceptableValueRange and again
    // by LensPanel at build, so a hand edited config file cannot push the panel over the crosshair.
    internal static ConfigEntry<bool> Enabled = null!;
    // Spec 4.2 General item 2: runtime toggle key, default H. KeyCode.None disables the binding.
    internal static ConfigEntry<KeyCode> ToggleKey = null!;
    internal static ConfigEntry<int> OffsetX = null!;
    internal static ConfigEntry<int> OffsetY = null!;
    internal static ConfigEntry<int> PanelWidth = null!;
    internal static ConfigEntry<float> GuiScale = null!;
    internal static ConfigEntry<int> MaxRows = null!;
    internal static ConfigEntry<SortRows> SortRowsBy = null!;
    internal static ConfigEntry<bool> ShowNames = null!;
    internal static ConfigEntry<bool> ShowFullHealth = null!;
    internal static ConfigEntry<bool> AvoidHoverText = null!;
    internal static ConfigEntry<int> RefreshHz = null!;
    internal static ConfigEntry<float> FadeSeconds = null!;
    internal static ConfigEntry<float> BackdropAlpha = null!;

    // Spec 4.2 ShowDays, used by LensFormat.TimeWithDays.
    internal static ConfigEntry<bool> ShowDays = null!;

    // Spec 4.2 Containers item 12: hide world chest contents until opened.
    internal static ConfigEntry<bool> HideUnopenedWorldChests = null!;

    // One gate per target group, spec 4.2. Each reader's Enabled property reads one of these.
    internal static ConfigEntry<bool> ShowContainers = null!;
    internal static ConfigEntry<bool> ShowFires = null!;
    internal static ConfigEntry<bool> ShowSmelters = null!;
    internal static ConfigEntry<bool> ShowCooking = null!;
    internal static ConfigEntry<bool> ShowFermenting = null!;
    internal static ConfigEntry<bool> ShowPlants = null!;
    internal static ConfigEntry<bool> ShowPickables = null!;
    // Spec 3.9 item 2: the picked respawn line is the optional half of that item, so it is
    // off by default and gated apart from ShowPickables, which also covers the unpicked yield.
    internal static ConfigEntry<bool> ShowRespawn = null!;
    internal static ConfigEntry<PieceStatus> ShowBuildPieces = null!;
    internal static ConfigEntry<bool> ShowMineables = null!;
    internal static ConfigEntry<bool> ShowTreesAndRocks = null!;
    internal static ConfigEntry<bool> ShowStands = null!;
    internal static ConfigEntry<bool> ShowCreatures = null!;
    internal static ConfigEntry<bool> ShowMisc = null!;

    public void Awake()
    {
        BindConfig();
        _harmony.PatchAll(typeof(OttoLensPlugin).Assembly);
        Log.LogInfo($"{ModName} {ModVersion} loaded.");
    }

    private void BindConfig()
    {
        Enabled = Config.Bind(PanelSection, "enabled", true, "Master switch. Off hides the panel and skips every reader.");
        ToggleKey = Config.Bind(PanelSection, "toggleKey", KeyCode.H, "Key that flips the master toggle at runtime. Set to None to disable.");
        OffsetX = Config.Bind(PanelSection, "offsetX", 150, new ConfigDescription("Left edge of the panel, pixels right of screen centre.", new AcceptableValueRange<int>(96, 600)));
        OffsetY = Config.Bind(PanelSection, "offsetY", -32, new ConfigDescription("Top edge of the panel, pixels below screen centre (negative).", new AcceptableValueRange<int>(-400, -16)));
        PanelWidth = Config.Bind(PanelSection, "panelWidth", 300, new ConfigDescription("Fixed panel width in pixels.", new AcceptableValueRange<int>(240, 420)));
        GuiScale = Config.Bind(PanelSection, "guiScale", 1.0f, new ConfigDescription("Scale of the whole panel. 1.15 is a good value at 1440p.", new AcceptableValueRange<float>(0.75f, 1.6f)));
        MaxRows = Config.Bind(PanelSection, "maxRows", 10, new ConfigDescription("Item rows per block before 'and N more'. Also clamped to what fits on screen.", new AcceptableValueRange<int>(1, 16)));
        SortRowsBy = Config.Bind(PanelSection, "sortRows", SortRows.Count, "Row order: Count sorts by count descending then name; Slot keeps the container's own order.");
        ShowNames = Config.Bind(PanelSection, "showNames", true, "Show the item name column. Off shows icon and count only.");
        ShowFullHealth = Config.Bind(PanelSection, "showFullHealth", false, "Show the Health row on a build piece at 100 percent.");
        AvoidHoverText = Config.Bind(PanelSection, "avoidHoverText", true, "Push the panel right when the vanilla hover line is wide enough to run under it.");
        RefreshHz = Config.Bind(PanelSection, "refreshHz", 4, new ConfigDescription("Value refresh rate while the panel is visible.", new AcceptableValueRange<int>(1, 10)));
        FadeSeconds = Config.Bind(PanelSection, "fadeSeconds", 0.08f, new ConfigDescription("Show and hide fade, in seconds.", new AcceptableValueRange<float>(0f, 0.5f)));
        BackdropAlpha = Config.Bind(PanelSection, "backdropAlpha", 0.88f, new ConfigDescription("Opacity of the panel plate.", new AcceptableValueRange<float>(0.5f, 1.0f)));
        ShowDays = Config.Bind(PanelSection, "showDays", true, "Add a game day figure to fuel and grow times that run longer than half a day.");

        HideUnopenedWorldChests = Config.Bind(TargetSection, "hideUnopenedWorldChests", true, "Hide the contents of world-placed chests until the player has opened them once.");
        ShowContainers = Config.Bind(TargetSection, "containers", true, "Chests, carts and ship holds.");
        ShowFires = Config.Bind(TargetSection, "fires", true, "Fireplaces, hearths, torches and braziers.");
        ShowSmelters = Config.Bind(TargetSection, "smelters", true, "Smelters, kilns, blast furnaces and windmill fed smelters.");
        ShowCooking = Config.Bind(TargetSection, "cooking", true, "Cooking stations and ovens.");
        ShowFermenting = Config.Bind(TargetSection, "fermenting", true, "Fermenters, beehives and sap collectors.");
        ShowPlants = Config.Bind(TargetSection, "plants", true, "Planted crops and saplings.");
        ShowPickables = Config.Bind(TargetSection, "pickables", true, "Pickable plants and item piles.");
        ShowRespawn = Config.Bind(TargetSection, "showRespawn", false, "Show the respawn countdown on an already picked pickable. Needs pickables on.");
        ShowBuildPieces = Config.Bind(TargetSection, "buildPieces", PieceStatus.WithHammer, "Build piece health and support: Off, WithHammer (place mode only) or Always (any hovered piece).");
        ShowMineables = Config.Bind(TargetSection, "mineables", true, "Mineable rocks (MineRock and MineRock5). On by default; rocks are Hoverable and cost no extra raycast.");
        ShowTreesAndRocks = Config.Bind(TargetSection, "treesAndRocks", false, "Trees, stumps and logs. Off by default: they are read from the vanilla hover, so this costs nothing while it is off and no extra raycast while it is on.");
        ShowStands = Config.Bind(TargetSection, "stands", true, "Item stands and armor stands.");
        ShowCreatures = Config.Bind(TargetSection, "creatures", true, "Tamed creatures and pets.");
        ShowMisc = Config.Bind(TargetSection, "misc", true, "Tombstones, wisp spawners, shield generators, feasts, ground items and crafting stations.");

        // Design section 7: geometry applies on the next rebuild; these force one now.
        // Row order is fixed at rebuild too, so sortRows needs the same push to take effect
        // while the player is still hovering the target they changed it for.
        MaxRows.SettingChanged += OnRebuildSettingChanged;
        ShowNames.SettingChanged += OnRebuildSettingChanged;
        SortRowsBy.SettingChanged += OnRebuildSettingChanged;
        // A master switch flip tears the panel down; the next frame with it on rebuilds lazily.
        Enabled.SettingChanged += OnEnabledChanged;
        // Patches caches the resolved reader per hover object, and the target gates are read
        // only during that resolve, so flipping one while the crosshair rests on the target has
        // to drop the cache. One handler on the file covers every entry in the Targets section.
        Config.SettingChanged += OnTargetSettingChanged;
    }

    private static void OnRebuildSettingChanged(object sender, EventArgs e) => LensPanel.RequestRebuild();

    private static void OnTargetSettingChanged(object sender, SettingChangedEventArgs e)
    {
        if (e.ChangedSetting.Definition.Section == TargetSection)
        {
            Patches.InvalidateHoverCache();
        }
    }

    private static void OnEnabledChanged(object sender, EventArgs e)
    {
        if (!Enabled.Value)
        {
            LensPanel.Destroy();
        }
    }

    /// Unpatching removes the Hud hooks, so Hud.OnDestroy can no longer tear the panel down:
    /// without this the panel would stay parented under the Hud with its tick loop running and
    /// the static Localization.OnLanguageChange handlers would keep this assembly reachable.
    private void OnDestroy()
    {
        try
        {
            Patches.Teardown();
            LensFormat.UnhookLanguage();
            LensReaders.UnhookLanguage();
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Teardown failed: {ex}");
        }

        _harmony.UnpatchSelf();
    }
}
