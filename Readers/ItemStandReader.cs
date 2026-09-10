using OttoLens.Model;

namespace OttoLens.Readers;

/// Item stand: one tile for the attached item (spec 3.13). Boss offering stones show
/// nothing because vanilla already prints the power text there.
internal sealed class ItemStandReader : ILensReader
{
    // Keyed on hash and variant together so a variant sprite is not confused with the base one.
    private static readonly Dictionary<long, Sprite?> SpriteCache = new();

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly LensItem _item = new();

    public Type TargetType => typeof(ItemStand);

    public bool Enabled => OttoLensPlugin.ShowStands.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var stand = (ItemStand)target;
        ZNetView view = stand.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        if (!stand.m_canBeRemoved && stand.m_guardianPower != null)
        {
            return null;
        }

        if (!LensReaders.HasWardAccess(stand))
        {
            return null;
        }

        int hash = stand.GetAttachedItem();
        if (hash == 0)
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        int variant = zdo.GetInt(ZDOVars.s_variant);
        int quality = zdo.GetInt(ZDOVars.s_quality, 1);
        if (!TryResolveItem(hash, variant, out string name, out Sprite? sprite, out _))
        {
            // The visual name is a fallback for a prefab the database no longer knows.
            name = stand.m_currentItemName;
            if (name.Length == 0)
            {
                return null;
            }
        }

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(stand.m_name);

        LensItem item = _item;
        item.Sprite = sprite;
        item.Name = LensFormat.Name(name);
        item.Count = 1;
        item.Quality = quality;

        LensItemBlock block = _block;
        block.Clear();
        block.Items.Add(item);
        report.Block0 = block;
        return report;
    }

    /// Resolves a stable item hash to its display token, variant sprite and item type through
    /// the object database. Shared with the armor stand reader. Returns false when the
    /// database does not know the hash.
    internal static bool TryResolveItem(int hash, int variant, out string name, out Sprite? sprite, out ItemDrop.ItemData.ItemType type)
    {
        name = "";
        sprite = null;
        type = ItemDrop.ItemData.ItemType.None;

        ObjectDB db = ObjectDB.instance;
        if (db == null || !db.TryGetItemPrefab(hash, out GameObject prefab) || prefab == null)
        {
            return false;
        }

        ItemDrop? drop = prefab.GetComponent<ItemDrop>();
        if (drop == null || drop.m_itemData == null || drop.m_itemData.m_shared == null)
        {
            return false;
        }

        ItemDrop.ItemData.SharedData shared = drop.m_itemData.m_shared;
        name = shared.m_name;
        type = shared.m_itemType;
        sprite = VariantSprite(hash, variant, shared);
        return true;
    }

    // Unity fakes null for a destroyed sprite, so a cached hit is re-checked before reuse.
    private static Sprite? VariantSprite(int hash, int variant, ItemDrop.ItemData.SharedData shared)
    {
        long key = ((long)hash << 32) | (uint)variant;
        if (SpriteCache.TryGetValue(key, out Sprite? cached) && cached != null)
        {
            return cached;
        }

        Sprite? sprite = null;
        try
        {
            Sprite[] icons = shared.m_icons;
            if (icons != null && icons.Length > 0)
            {
                sprite = icons[variant >= 0 && variant < icons.Length ? variant : 0];
            }
        }
        catch (Exception)
        {
            sprite = null;
        }

        SpriteCache[key] = sprite;
        return sprite;
    }
}
