using OttoLens.Model;

namespace OttoLens.Readers;

/// Armor stand: one tile per occupied slot, weapons first, then shield, helmet, body, legs,
/// cape and utility (spec 3.14). The crosshair hits a slot Switch, so the stand is resolved
/// from a parent.
internal sealed class ArmorStandReader : ILensReader
{
    // Vanilla stores each slot under "<index>_item" and "<index>_variant"; the hashes are
    // built once and grown only when a stand has more slots than seen before.
    private static int[] s_itemKeys = Array.Empty<int>();
    private static int[] s_variantKeys = Array.Empty<int>();

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly List<LensItem> _pool = new();
    private readonly List<int> _order = new();

    public Type TargetType => typeof(ArmorStand);

    public bool Enabled => OttoLensPlugin.ShowStands.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var stand = (ArmorStand)target;
        ZNetView view = stand.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        if (!LensReaders.HasWardAccess(stand))
        {
            return null;
        }

        List<ArmorStand.ArmorStandSlot> slots = stand.m_slots;
        int slotCount = slots.Count;
        if (slotCount == 0)
        {
            return null;
        }

        EnsureKeys(slotCount);
        ZDO zdo = view.GetZDO();

        // Sort key packs the type rank above the slot index so equal ranks keep slot order.
        List<int> order = _order;
        order.Clear();
        int occupied = 0;
        for (int i = 0; i < slotCount; i++)
        {
            int hash = zdo.GetInt(s_itemKeys[i]);
            if (hash == 0)
            {
                continue;
            }

            int variant = zdo.GetInt(s_variantKeys[i]);
            string name;
            Sprite? sprite;
            if (!ItemStandReader.TryResolveItem(hash, variant, out name, out sprite, out ItemDrop.ItemData.ItemType type))
            {
                name = slots[i].m_currentItemName;
                if (name.Length == 0)
                {
                    continue;
                }
            }

            LensItem item = Pooled(occupied);
            item.Sprite = sprite;
            item.Name = LensFormat.Name(name);
            item.Count = 1;
            item.Quality = 0;
            order.Add((Rank(type) << 8) | occupied);
            occupied++;
        }

        if (occupied == 0)
        {
            return null;
        }

        order.Sort();

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(stand.m_name);

        LensItemBlock block = _block;
        block.Clear();
        for (int i = 0; i < order.Count; i++)
        {
            block.Items.Add(_pool[order[i] & 0xFF]);
        }

        report.Block0 = block;
        report.Footer = LensFormat.Free(slotCount - occupied);
        return report;
    }

    private LensItem Pooled(int index)
    {
        while (_pool.Count <= index)
        {
            _pool.Add(new LensItem());
        }

        return _pool[index];
    }

    private static void EnsureKeys(int count)
    {
        if (s_itemKeys.Length >= count)
        {
            return;
        }

        var itemKeys = new int[count];
        var variantKeys = new int[count];
        for (int i = 0; i < count; i++)
        {
            itemKeys[i] = (i + "_item").GetStableHashCode();
            variantKeys[i] = (i + "_variant").GetStableHashCode();
        }

        s_itemKeys = itemKeys;
        s_variantKeys = variantKeys;
    }

    private static int Rank(ItemDrop.ItemData.ItemType type)
    {
        switch (type)
        {
            case ItemDrop.ItemData.ItemType.OneHandedWeapon:
            case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
            case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
            case ItemDrop.ItemData.ItemType.Bow:
            case ItemDrop.ItemData.ItemType.Tool:
            case ItemDrop.ItemData.ItemType.Attach_Atgeir:
            case ItemDrop.ItemData.ItemType.Torch:
                return 0;
            case ItemDrop.ItemData.ItemType.Shield:
                return 1;
            case ItemDrop.ItemData.ItemType.Helmet:
                return 2;
            case ItemDrop.ItemData.ItemType.Chest:
                return 3;
            case ItemDrop.ItemData.ItemType.Legs:
                return 4;
            case ItemDrop.ItemData.ItemType.Shoulder:
                return 5;
            case ItemDrop.ItemData.ItemType.Utility:
                return 6;
            default:
                return 7;
        }
    }
}
