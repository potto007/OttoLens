using BepInEx.Configuration;
using BepInEx.Logging;
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

    // Panel knobs, design section 7. Clamps are enforced by the AcceptableValueRange and again
    // by LensPanel at build, so a hand edited config file cannot push the panel over the crosshair.
    internal static ConfigEntry<bool> Enabled = null!;
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

    // One gate per target group, spec 4.2. Each reader's Enabled property reads one of these.
    internal static ConfigEntry<bool> ShowContainers = null!;
    internal static ConfigEntry<bool> ShowFires = null!;
    internal static ConfigEntry<bool> ShowSmelters = null!;
    internal static ConfigEntry<bool> ShowCooking = null!;
    internal static ConfigEntry<bool> ShowFermenting = null!;
    internal static ConfigEntry<bool> ShowPlants = null!;
    internal static ConfigEntry<bool> ShowPickables = null!;
    internal static ConfigEntry<bool> ShowBuildPieces = null!;
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

        ShowContainers = Config.Bind(TargetSection, "containers", true, "Chests, carts and ship holds.");
        ShowFires = Config.Bind(TargetSection, "fires", true, "Fireplaces, hearths, torches and braziers.");
        ShowSmelters = Config.Bind(TargetSection, "smelters", true, "Smelters, kilns, blast furnaces and windmill fed smelters.");
        ShowCooking = Config.Bind(TargetSection, "cooking", true, "Cooking stations and ovens.");
        ShowFermenting = Config.Bind(TargetSection, "fermenting", true, "Fermenters, beehives and sap collectors.");
        ShowPlants = Config.Bind(TargetSection, "plants", true, "Planted crops and saplings.");
        ShowPickables = Config.Bind(TargetSection, "pickables", true, "Pickable plants and item piles.");
        ShowBuildPieces = Config.Bind(TargetSection, "buildPieces", true, "Build piece health and support, with the hammer out.");
        ShowTreesAndRocks = Config.Bind(TargetSection, "treesAndRocks", false, "Trees, stumps, logs and mineable rocks. Off by default because trees cost one extra raycast per frame while nothing else is hovered.");
        ShowStands = Config.Bind(TargetSection, "stands", true, "Item stands and armor stands.");
        ShowCreatures = Config.Bind(TargetSection, "creatures", true, "Tamed creatures and pets.");
        ShowMisc = Config.Bind(TargetSection, "misc", true, "Tombstones, wisp spawners, shield generators, feasts, ground items and crafting stations.");

        // Design section 7: geometry applies on the next rebuild; these two force one now.
        MaxRows.SettingChanged += OnRebuildSettingChanged;
        ShowNames.SettingChanged += OnRebuildSettingChanged;
        // A master switch flip tears the panel down; the next frame with it on rebuilds lazily.
        Enabled.SettingChanged += OnEnabledChanged;
    }

    private static void OnRebuildSettingChanged(object sender, EventArgs e) => LensPanel.RequestRebuild();

    private static void OnEnabledChanged(object sender, EventArgs e)
    {
        if (!Enabled.Value)
        {
            LensPanel.Destroy();
        }
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
    }
}
