using OttoLens.Model;

namespace OttoLens.Readers;

/// Registry of readers in priority order. Register() is the only place readers are listed.
internal static class LensReaders
{
    private static readonly List<ILensReader> Readers = new();
    private static bool _registered;
    private static string? _noAccessText;
    // _noAccessText holds Localize() output, which changes when the player switches language in
    // the settings menu. Localization.SetLanguage fires the static OnLanguageChange
    // (assembly_guiutils 1.0.7), so hook it once when the string is first cached and keep it for
    // the life of the assembly (UnhookLanguage() detaches it on plugin unload), the same way
    // LensFormat guards its name cache. Without this the EndsWith test in
    // PassesCoreGuards compares the new language's hover text against the old language's string
    // and can never match again, leaving the no access guard of spec 6.1.3 dead for the session.
    private static bool _languageHooked;

    /// Bumped by ClearLocalizedCaches. Readers that memoize localized text inside a cache keyed
    /// on game state - the grouped container rows of ContainerReader and CraftingStationReader -
    /// fold this into that key, so a language change both rebuilds the rows and moves the report
    /// signature LensPanel.Refresh compares. Dropping the rows alone would not: those readers
    /// publish a ContentSignature built from the container revision and item count, which does
    /// not move on a language change, so Refresh would take the Tick path and Tick never
    /// re-pushes row names.
    internal static int LocalizationEpoch { get; private set; }

    internal static IReadOnlyList<ILensReader> All
    {
        get
        {
            Register();
            return Readers;
        }
    }

    /// One line per reader. Order is priority: the first reader whose TargetType is found on
    /// the hover object or a parent wins, so put specific types before general ones (a
    /// Container before the WearNTear that sits on the same piece).
    internal static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        Readers.Add(new TameableReader());
        Readers.Add(new TombStoneReader());
        Readers.Add(new ContainerReader());
        Readers.Add(new ItemStandReader());
        Readers.Add(new ArmorStandReader());
        Readers.Add(new PlantReader());
        Readers.Add(new PickableReader());
        Readers.Add(new PickableItemReader());
        Readers.Add(new FireplaceReader());
        Readers.Add(new SmelterReader());
        Readers.Add(new CookingStationReader());
        Readers.Add(new FermenterReader());
        Readers.Add(new BeehiveReader());
        Readers.Add(new SapCollectorReader());
        Readers.Add(new WispSpawnerReader());
        Readers.Add(new ShieldGeneratorReader());
        Readers.Add(new FeastReader());
        Readers.Add(new ItemDropReader());
        Readers.Add(new CraftingStationReader());
        Readers.Add(new MineRockReader(legacy: false));
        Readers.Add(new MineRockReader(legacy: true));
        Readers.Add(new TreeReader(logs: false));
        Readers.Add(new TreeReader(logs: true));
        Readers.Add(new WearNTearReader());
        Readers.Add(new DestructibleReader());
    }

    /// Walk the hover object upward once per enabled reader. Returns the reader and the
    /// component it will read, or false when nothing claims the object.
    internal static bool Resolve(GameObject hover, out ILensReader reader, out Component target)
    {
        Register();
        for (int i = 0; i < Readers.Count; i++)
        {
            ILensReader candidate = Readers[i];
            if (!candidate.Enabled)
            {
                // A shadowing reader that is gated off has to suppress its target, not hand it
                // down the list, so it still pays one component walk while disabled.
                if (Shadows(candidate) && hover.GetComponentInParent(candidate.TargetType) != null)
                {
                    break;
                }

                continue;
            }

            Component? found = hover.GetComponentInParent(candidate.TargetType);
            if (found != null)
            {
                reader = candidate;
                target = found;
                return true;
            }
        }

        reader = null!;
        target = null!;
        return false;
    }

    /// Readers that sit this high in the list only to win the race for a component a later
    /// reader also claims: TombStone.Awake puts the grave's Container on the grave itself, and a
    /// pet piece carries the ItemStand its Tameable reads. For these the group gate means "show
    /// nothing", never "let the next reader have it" - misc off would otherwise turn every grave
    /// into a container plate listing a dead player's whole inventory, the output TombStoneReader
    /// exists to prevent, and creatures off would turn a pet into an item stand.
    private static bool Shadows(ILensReader reader) => reader is TombStoneReader or TameableReader;

    /// Guards the core applies before Read runs: placement ghost (spec 6.1.5), a ZNetView
    /// that is invalid or has no ZDO (spec 6.1.4), and a vanilla hover text that ended in the
    /// no access line (spec 6.1.3). A target with no ZNetView at all passes; the reader
    /// decides what to do with it.
    internal static bool PassesCoreGuards(Component target, string hoverText)
    {
        GameObject go = target.gameObject;
        if (Player.IsPlacementGhost(go))
        {
            return false;
        }

        ZNetView? view = target.GetComponentInParent<ZNetView>();
        if (view != null && (!view.IsValid() || view.GetZDO() == null))
        {
            return false;
        }

        if (hoverText.Length > 0)
        {
            if (_noAccessText == null)
            {
                if (!_languageHooked)
                {
                    _languageHooked = true;
                    Localization.OnLanguageChange += ClearLocalizedCaches;
                }

                _noAccessText = Localization.instance.Localize("$piece_noaccess");
            }

            if (_noAccessText.Length > 0 && hoverText.EndsWith(_noAccessText, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// Drops every piece of localized text the readers memoize: the no access string above, the
    /// pet stand item names, and - through the epoch - the grouped container rows. Runs on a
    /// language change and from Hud.OnDestroy, which is the same pair LensFormat.ClearCaches
    /// covers for the name cache.
    internal static void ClearLocalizedCaches()
    {
        _noAccessText = null;
        TameableReader.ClearStandItems();
        LocalizationEpoch++;
    }

    /// Plugin unload only. Detaches the language hook so the next cached string re-registers it,
    /// which is what a hot reload of the assembly needs.
    internal static void UnhookLanguage()
    {
        if (_languageHooked)
        {
            Localization.OnLanguageChange -= ClearLocalizedCaches;
            _languageHooked = false;
        }
    }

    /// Ward access without the flash (spec 3.0). Readers for pieces that vanilla ward-checks
    /// call this themselves; carts and ships skip it.
    internal static bool HasWardAccess(Component target)
        => PrivateArea.CheckAccess(target.transform.position, 0f, flash: false);
}
