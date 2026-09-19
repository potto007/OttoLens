using System.IO;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using BepInEx.Logging;
using OttoLens.Readers;
using OttoLens.UI;

namespace OttoLens;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class OttoLensPlugin : BaseUnityPlugin
{
    internal const string ModName = "OttoLens";
    internal const string ModVersion = "1.0.3";
    internal const string Author = "potto007";
    internal const string ModGUID = $"{Author}.{ModName}";

    internal static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource(ModName);
    private readonly Harmony _harmony = new(ModGUID);

    private const string PanelSection = "OttoLens";
    private const string CameraSection = "Camera";
    private const string TargetSection = "Targets";

    // v1.0.2 made ToggleKey unbound by default, but BepInEx had already written H into every
    // config file an earlier version saved. Those files move to None once: the header names
    // the version that last saved the file, and the first save by this version rewrites it.
    // assembly_valheim declares its own global Version type.
    private static readonly System.Version UnboundToggleKeySince = new(1, 0, 2);
    private static readonly Regex SavedByHeader = new(@"^## Settings file was created by plugin .+ v(\d+(?:\.\d+){1,3})");

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
    // Spec 4.2 General item 2: runtime toggle key, default None (unbound). KeyCode.None disables the binding.
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

    // The Eye of Odin makes all things clear: bloom lens dirt is off unless the player wants it.
    // Independent of Enabled, so the toggle key does not flash the smudges on and off.
    internal static ConfigEntry<bool> RemoveLensDirt = null!;

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
        // A late load (script reload) finds the camera already awake; a normal start finds none.
        LensDirt.ApplyCurrent();
        Log.LogInfo($"{ModName} {ModVersion} loaded.");
    }

    /// Null when the file does not exist yet or its header does not name a version.
    private System.Version? ReadSavedByVersion()
    {
        try
        {
            if (!File.Exists(Config.ConfigFilePath))
            {
                return null;
            }

            using StreamReader reader = new(Config.ConfigFilePath);
            Match match = SavedByHeader.Match(reader.ReadLine() ?? string.Empty);
            return match.Success ? new System.Version(match.Groups[1].Value) : null;
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Could not read the version header of {Path.GetFileName(Config.ConfigFilePath)}: {ex.Message}");
            return null;
        }
    }

    private static void MigrateToggleKey(System.Version? savedBy)
    {
        if (savedBy == null || savedBy >= UnboundToggleKeySince || ToggleKey.Value != KeyCode.H)
        {
            return;
        }

        ToggleKey.Value = KeyCode.None;
        Log.LogInfo($"ToggleKey changed from H to None (unbound), the default since v{UnboundToggleKeySince.ToString(3)}. The config was last saved by v{savedBy}.");
    }

    private void BindConfig()
    {
        // Read before the first Bind, which saves the file under this version's header.
        System.Version? savedBy = ReadSavedByVersion();

        Enabled = Config.Bind(PanelSection, "Enabled", true, "Master switch. Off hides the panel and skips every reader.");
        ToggleKey = Config.Bind(PanelSection, "ToggleKey", KeyCode.None, "Key that flips the master toggle at runtime. None (default) leaves it unbound.");
        OffsetX = Config.Bind(PanelSection, "OffsetX", 150, new ConfigDescription("Left edge of the panel, pixels right of screen centre.", new AcceptableValueRange<int>(96, 600)));
        OffsetY = Config.Bind(PanelSection, "OffsetY", -32, new ConfigDescription("Top edge of the panel, pixels below screen centre (negative).", new AcceptableValueRange<int>(-400, -16)));
        PanelWidth = Config.Bind(PanelSection, "PanelWidth", 300, new ConfigDescription("Fixed panel width in pixels.", new AcceptableValueRange<int>(240, 420)));
        GuiScale = Config.Bind(PanelSection, "GuiScale", 1.0f, new ConfigDescription("Scale of the whole panel. 1.15 is a good value at 1440p.", new AcceptableValueRange<float>(0.75f, 1.6f)));
        MaxRows = Config.Bind(PanelSection, "MaxRows", 10, new ConfigDescription("Item rows per block before 'and N more'. Also clamped to what fits on screen.", new AcceptableValueRange<int>(1, 16)));
        SortRowsBy = Config.Bind(PanelSection, "SortRows", SortRows.Count, "Row order: Count sorts by count descending then name; Slot keeps the container's own order.");
        ShowNames = Config.Bind(PanelSection, "ShowNames", true, "Show the item name column. Off shows icon and count only.");
        ShowFullHealth = Config.Bind(PanelSection, "ShowFullHealth", false, "Show the Health row on a build piece at 100 percent.");
        AvoidHoverText = Config.Bind(PanelSection, "AvoidHoverText", true, "Push the panel right when the vanilla hover line is wide enough to run under it.");
        RefreshHz = Config.Bind(PanelSection, "RefreshHz", 4, new ConfigDescription("Value refresh rate while the panel is visible.", new AcceptableValueRange<int>(1, 10)));
        FadeSeconds = Config.Bind(PanelSection, "FadeSeconds", 0.08f, new ConfigDescription("Show and hide fade, in seconds.", new AcceptableValueRange<float>(0f, 0.5f)));
        BackdropAlpha = Config.Bind(PanelSection, "BackdropAlpha", 0.88f, new ConfigDescription("Opacity of the panel plate.", new AcceptableValueRange<float>(0.5f, 1.0f)));
        ShowDays = Config.Bind(PanelSection, "ShowDays", true, "Add a game day figure to fuel and grow times that run longer than half a day.");

        RemoveLensDirt = Config.Bind(CameraSection, "RemoveLensDirt", true, "The Eye of Odin makes all things clear. Removes the smudges that bloom draws over bright light.");

        HideUnopenedWorldChests = Config.Bind(TargetSection, "HideUnopenedWorldChests", true, "Hide the contents of world-placed chests until the player has opened them once.");
        ShowContainers = Config.Bind(TargetSection, "Containers", true, "Chests, carts and ship holds.");
        ShowFires = Config.Bind(TargetSection, "Fires", true, "Fireplaces, hearths, torches and braziers.");
        ShowSmelters = Config.Bind(TargetSection, "Smelters", true, "Smelters, kilns, blast furnaces and windmill fed smelters.");
        ShowCooking = Config.Bind(TargetSection, "Cooking", true, "Cooking stations and ovens.");
        ShowFermenting = Config.Bind(TargetSection, "Fermenting", true, "Fermenters, beehives and sap collectors.");
        ShowPlants = Config.Bind(TargetSection, "Plants", true, "Planted crops and saplings.");
        ShowPickables = Config.Bind(TargetSection, "Pickables", true, "Pickable plants and item piles.");
        ShowRespawn = Config.Bind(TargetSection, "ShowRespawn", false, "Show the respawn countdown on an already picked pickable. Needs Pickables on.");
        ShowBuildPieces = Config.Bind(TargetSection, "BuildPieces", PieceStatus.WithHammer, "Build piece health and support: Off, WithHammer (place mode only) or Always (any hovered piece).");
        ShowMineables = Config.Bind(TargetSection, "Mineables", true, "Mineable rocks (MineRock and MineRock5). On by default; rocks are Hoverable and cost no extra raycast.");
        ShowTreesAndRocks = Config.Bind(TargetSection, "TreesAndRocks", false, "Trees, stumps and logs. Off by default: they are read from the vanilla hover, so this costs nothing while it is off and no extra raycast while it is on.");
        ShowStands = Config.Bind(TargetSection, "Stands", true, "Item stands and armor stands.");
        ShowCreatures = Config.Bind(TargetSection, "Creatures", true, "Tamed creatures and pets.");
        ShowMisc = Config.Bind(TargetSection, "Misc", true, "Tombstones, wisp spawners, shield generators, feasts, ground items and crafting stations.");

        MigrateToggleKey(savedBy);
        // Bind saves only when it adds an entry, so a file with nothing new keeps the old header
        // and a later hand edit to H would be cleared again. Stamp this version now.
        if (savedBy != null && savedBy < new System.Version(ModVersion))
        {
            Config.Save();
        }

        // Design section 7: geometry applies on the next rebuild; these force one now.
        // Row order is fixed at rebuild too, so SortRows needs the same push to take effect
        // while the player is still hovering the target they changed it for.
        MaxRows.SettingChanged += OnRebuildSettingChanged;
        ShowNames.SettingChanged += OnRebuildSettingChanged;
        SortRowsBy.SettingChanged += OnRebuildSettingChanged;
        // A master switch flip tears the panel down; the next frame with it on rebuilds lazily.
        Enabled.SettingChanged += OnEnabledChanged;
        // CameraEffects.Awake covers each new camera; a flip mid-session has to reach the live one.
        RemoveLensDirt.SettingChanged += OnLensDirtChanged;
        // Patches caches the resolved reader per hover object, and the target gates are read
        // only during that resolve, so flipping one while the crosshair rests on the target has
        // to drop the cache. One handler on the file covers every entry in the Targets section.
        Config.SettingChanged += OnTargetSettingChanged;
    }

    private static void OnRebuildSettingChanged(object sender, EventArgs e) => LensPanel.RequestRebuild();

    private static void OnLensDirtChanged(object sender, EventArgs e) => LensDirt.ApplyCurrent();

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
    /// The camera profile is a shared asset that outlives the plugin, so its lens dirt goes back.
    private void OnDestroy()
    {
        try
        {
            Patches.Teardown();
            LensFormat.UnhookLanguage();
            LensReaders.UnhookLanguage();
            LensDirt.Restore();
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Teardown failed: {ex}");
        }

        _harmony.UnpatchSelf();
    }
}
