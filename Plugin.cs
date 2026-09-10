using BepInEx.Logging;

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

    public void Awake()
    {
        _harmony.PatchAll(typeof(OttoLensPlugin).Assembly);
        Log.LogInfo($"{ModName} {ModVersion} loaded.");
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
    }
}
