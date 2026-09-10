using OttoLens.Model;

namespace OttoLens.Readers;

/// Chest, cart and ship hold (spec 3.1). Primary meter is slot usage, block 0 is the grouped
/// contents, footer is the free slot count. Grouping is cached per container on the ZDO data
/// revision because a chest hover holds still for seconds at a time.
internal sealed class ContainerReader : ILensReader
{
    private const string HiddenFooter = "Contents unknown";

    private static readonly Dictionary<string, Sprite?> IconCache = new(StringComparer.Ordinal);

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly Dictionary<string, LensItem> _groupIndex = new(StringComparer.Ordinal);
    private readonly List<LensItem> _groups = new();

    // Cache key for the grouped list. The item count rides along with the revision because a
    // chest that just came into view deserializes its inventory up to a second after the ZDO
    // arrives, without moving the revision.
    private Container? _cachedContainer;
    private uint _cachedRevision;
    private int _cachedCount = -1;
    private string _cachedSignature = "";

    // Hash of the vanilla "Discovered_<player>" ZDO flag, rebuilt only when the name changes.
    private static string? _discoveredName;
    private static int _discoveredHash;

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

        uint revision = zdo.DataRevision;
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
    /// Vanilla records that opening in the ZDO under "Discovered_<player>" only when the
    /// prefab has a discover stat; chests without one cannot be tracked, so they show.
    private static bool IsUnopenedWorldChest(Container container, ZDO zdo, Player player)
    {
        Piece piece = container.m_piece;
        if (piece != null && piece.IsPlacedByPlayer())
        {
            return false;
        }

        if (container.m_discoverStat == PlayerStatType.None)
        {
            return false;
        }

        string name = player.GetPlayerName();
        if (!string.Equals(name, _discoveredName, StringComparison.Ordinal))
        {
            _discoveredName = name;
            _discoveredHash = ("Discovered_" + name).GetStableHashCode();
        }

        return !zdo.GetBool(_discoveredHash);
    }

    // Group by shared name, sum stacks, sort by count descending then name so the order is
    // stable frame to frame. Every vanilla item defaults to m_quality 1, so 1 maps to 0 (no
    // upgrade) like ItemDropReader; the group keeps the highest upgrade level.
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

        _groups.Sort(CompareGroups);
    }

    private static int CompareGroups(LensItem a, LensItem b)
    {
        int byCount = b.Count.CompareTo(a.Count);
        return byCount != 0 ? byCount : string.CompareOrdinal(a.Name, b.Name);
    }

    // Icons are per variant (capes, shields), so the cache key carries the variant index.
    private static Sprite? Icon(ItemDrop.ItemData item, string token)
    {
        string key = item.m_variant == 0 ? token : string.Concat(token, "#", item.m_variant.ToString());
        if (IconCache.TryGetValue(key, out Sprite? cached))
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
