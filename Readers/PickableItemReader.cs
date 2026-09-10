using OttoLens.Model;

namespace OttoLens.Readers;

/// A placed item pickup such as a dungeon loot item (spec 3.9). The concrete prefab is
/// chosen at spawn and stored in m_itemPrefab, so the random list is never read.
internal sealed class PickableItemReader : ILensReader
{
    private static readonly Dictionary<string, Sprite?> SpriteCache = new(StringComparer.Ordinal);

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly LensItem _item = new();

    public Type TargetType => typeof(PickableItem);

    public bool Enabled => OttoLensPlugin.ShowPickables.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var pickable = (PickableItem)target;
        ZNetView view = pickable.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ItemDrop drop = pickable.m_itemPrefab;
        if (pickable.m_picked || drop == null)
        {
            return null;
        }

        string name = LensFormat.Name(drop.m_itemData.m_shared.m_name);

        LensReport report = _report;
        report.Reset();
        report.Title = name;

        _item.Sprite = IconFor(drop.name, drop);
        _item.Name = name;
        // GetStackSize applies the world resource rate and the max stack clamp, read only.
        _item.Count = pickable.GetStackSize();
        _item.Quality = 0;

        LensItemBlock block = _block;
        block.Clear();
        block.Items.Add(_item);
        report.Block0 = block;
        return report;
    }

    /// Icon through the item data (spec 2.4), cached per prefab name. A destroyed sprite
    /// reads as Unity null and is probed again.
    private static Sprite? IconFor(string prefabName, ItemDrop drop)
    {
        if (SpriteCache.TryGetValue(prefabName, out Sprite? cached) && cached != null)
        {
            return cached;
        }

        Sprite? sprite = null;
        try
        {
            sprite = drop.m_itemData.GetIcon();
        }
        catch (Exception)
        {
            // An item with no icons throws on the index; the row still renders without a tile.
        }

        SpriteCache[prefabName] = sprite;
        return sprite;
    }
}
