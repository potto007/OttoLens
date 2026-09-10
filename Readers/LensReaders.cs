using OttoLens.Model;

namespace OttoLens.Readers;

/// Registry of readers in priority order. Register() is the only place readers are listed.
internal static class LensReaders
{
    private static readonly List<ILensReader> Readers = new();
    private static bool _registered;
    private static string? _noAccessText;

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
            _noAccessText ??= Localization.instance.Localize("$piece_noaccess");
            if (_noAccessText.Length > 0 && hoverText.EndsWith(_noAccessText, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// Ward access without the flash (spec 3.0). Readers for pieces that vanilla ward-checks
    /// call this themselves; carts and ships skip it.
    internal static bool HasWardAccess(Component target)
        => PrivateArea.CheckAccess(target.transform.position, 0f, flash: false);
}
