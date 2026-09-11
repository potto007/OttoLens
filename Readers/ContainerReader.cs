using OttoLens.Model;

namespace OttoLens.Readers;

/// Chest, cart and ship hold (spec 3.1). Primary meter is slot usage, block 0 is the grouped
/// contents, footer is the free slot count. Grouping is cached per container on the revision
/// the inventory was loaded from, because a chest hover holds still for seconds at a time.
internal sealed class ContainerReader : ILensReader
{
    private const string HiddenFooter = "Contents unknown";

    private static readonly Dictionary<string, Sprite?> IconCache = new(StringComparer.Ordinal);

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly Dictionary<string, LensItem> _groupIndex = new(StringComparer.Ordinal);
    private readonly List<LensItem> _groups = new();

    // Cache key for the grouped list. The revision is Container.m_lastRevision, not
    // ZDO.DataRevision: m_lastRevision is the revision the inventory was actually deserialized
    // from (Container.Load sets it, Container.Save sets it on the owner), while DataRevision
    // moves as soon as the ZDO arrives and the 1 s CheckForChanges tick reloads m_inventory up
    // to a second later. Keying on DataRevision would cache a group list built from the stale
    // inventory and never invalidate it when the counts change without the slot count moving.
    // The item count rides along as a cheap second guard.
    private Container? _cachedContainer;
    private uint _cachedRevision;
    private int _cachedCount = -1;
    private string _cachedSignature = "";

    // Hash of the vanilla "Discovered_<player>" ZDO flag, rebuilt only when the name changes.
    private static string? _discoveredName;
    private static int _discoveredHash;

    // Session record of world chests the player has opened (spec 5.3 item 5, spec 5.4 item 4).
    // Covers containers that carry no m_discoverStat and so never get the vanilla ZDO flag.
    // Cleared by ClearOpenedChests() on world unload (Hud.OnDestroy).
    private static readonly HashSet<ZDOID> _openedChests = new();

    public Type TargetType => typeof(Container);

    public bool Enabled => OttoLensPlugin.ShowContainers.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var container = (Container)target;
        ZNetView view = container.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ZDO? zdo = view.GetZDO();
        if (zdo == null)
        {
            return null;
        }

        Player player = Player.m_localPlayer;
        if (player == null)
        {
            return null;
        }

        // A cart or a ship is never inside its own ward and carries no creator on the hold.
        bool mobile = container.m_wagon != null || container.GetComponentInParent<Ship>() != null;
        if (!mobile && container.m_checkGuardStone && !LensReaders.HasWardAccess(container))
        {
            return null;
        }

        if (container.m_privacy != Container.PrivacySetting.Public)
        {
            // Vanilla CheckAccess dereferences m_piece for Private; a missing piece means deny.
            if (container.m_piece == null || !container.CheckAccess(player.GetPlayerID()))
            {
                return null;
            }
        }

        Inventory? inventory = container.GetInventory();
        if (inventory == null)
        {
            return null;
        }

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(container.m_name);

        if (!mobile && IsUnopenedWorldChest(container, zdo, player))
        {
            report.Footer = HiddenFooter;
            return report;
        }

        int used = inventory.NrOfItems();
        int capacity = inventory.GetWidth() * inventory.GetHeight();
        float fraction = capacity > 0 ? (float)used / capacity : 0f;
        LensColor fill = LensFormat.FillRamp(fraction);

        report.PrimaryMeter = LensReport.Meter("Slots", fraction, LensFormat.Count(used, capacity), fill);
        report.Footer = LensFormat.Free(Math.Max(0, capacity - used));

        if (used == 0)
        {
            report.Headline = "EMPTY";
            report.HeadlineColor = LensColor.Dim;
        }
        else if (fraction >= 1f)
        {
            report.Headline = "FULL";
            report.HeadlineColor = LensColor.Bad;
        }

        uint revision = container.m_lastRevision;
        if (!ReferenceEquals(container, _cachedContainer) || revision != _cachedRevision || used != _cachedCount)
        {
            Regroup(inventory);
            _cachedContainer = container;
            _cachedRevision = revision;
            _cachedCount = used;
            _cachedSignature = string.Concat(revision.ToString(), ":", used.ToString());
        }

        if (_groups.Count > 0)
        {
            _block.Clear();
            _block.Items.AddRange(_groups);
            report.Block0 = _block;
        }

        report.ContentSignature = _cachedSignature;
        return report;
    }

    /// A chest the world placed keeps its contents hidden until the player has opened it.
    /// Fast path: chests with a discover stat use the persistent "Discovered_<player>" ZDO flag
    /// that vanilla writes in Container.Interact (lines 235-240). This survives a relog.
    /// Fallback: chests without a discover stat (modded or non-tracked loot containers) use the
    /// session record in _openedChests, written by the InventoryGui.Show postfix.
    private static bool IsUnopenedWorldChest(Container container, ZDO zdo, Player player)
    {
        if (!OttoLensPlugin.HideUnopenedWorldChests.Value)
        {
            return false;
        }

        Piece piece = container.m_piece;
        if (piece != null && piece.IsPlacedByPlayer())
        {
            return false;
        }

        if (container.m_discoverStat != PlayerStatType.None)
        {
            string name = player.GetPlayerName();
            if (!string.Equals(name, _discoveredName, StringComparison.Ordinal))
            {
                _discoveredName = name;
                _discoveredHash = ("Discovered_" + name).GetStableHashCode();
            }
            return !zdo.GetBool(_discoveredHash);
        }

        // No discover stat: hide until the session record says the player has opened it.
        return !_openedChests.Contains(zdo.m_uid);
    }

    /// Called from the InventoryGui.Show postfix to record that a world chest has been opened.
    internal static void RecordOpen(Container container)
    {
        ZNetView? view = container.m_nview;
        if (view == null || !view.IsValid())
        {
            return;
        }
        ZDO? zdo = view.GetZDO();
        if (zdo != null)
        {
            _openedChests.Add(zdo.m_uid);
        }
    }

    /// Called from HudDestroyPostfix to clear the session record on world unload (spec 5.3 item 5).
    internal static void ClearOpenedChests() => _openedChests.Clear();

    // Group by shared name and sum stacks, keeping first-appearance order, which is the
    // container's own slot order. Row order belongs to the panel (design section 7): sorting
    // here as well would make `sortRows = slot` indistinguishable from `count`, because
    // LensPanel.ItemBlockView.Apply only reorders for `count` and otherwise keeps this order.
    // Every vanilla item defaults to m_quality 1, so 1 maps to 0 (no upgrade) like
    // ItemDropReader; the group keeps the highest upgrade level.
    private void Regroup(Inventory inventory)
    {
        _groupIndex.Clear();
        _groups.Clear();

        List<ItemDrop.ItemData> items = inventory.GetAllItems();
        for (int i = 0; i < items.Count; i++)
        {
            ItemDrop.ItemData item = items[i];
            if (item == null || item.m_shared == null)
            {
                continue;
            }

            string token = item.m_shared.m_name ?? "";
            if (_groupIndex.TryGetValue(token, out LensItem group))
            {
                group.Count += item.m_stack;
                int upgraded = item.m_quality > 1 ? item.m_quality : 0;
                if (upgraded > group.Quality)
                {
                    group.Quality = upgraded;
                }

                continue;
            }

            int quality = item.m_quality > 1 ? item.m_quality : 0;
            var created = new LensItem(Icon(item, token), LensFormat.Name(token), item.m_stack, quality);
            _groupIndex[token] = created;
            _groups.Add(created);
        }
    }

    // Icons are per variant (capes, shields), so the cache key carries the variant index.
    // A destroyed sprite reads as Unity null and is probed again: the cache outlives a world
    // unload, and a stale entry would hand the panel a fake-null sprite that silently disables
    // the row art for the rest of the session.
    private static Sprite? Icon(ItemDrop.ItemData item, string token)
    {
        string key = item.m_variant == 0 ? token : string.Concat(token, "#", item.m_variant.ToString());
        if (IconCache.TryGetValue(key, out Sprite? cached) && cached != null)
        {
            return cached;
        }

        Sprite? sprite;
        try
        {
            sprite = item.GetIcon();
        }
        catch (Exception)
        {
            sprite = null;
        }

        IconCache[key] = sprite;
        return sprite;
    }
}
