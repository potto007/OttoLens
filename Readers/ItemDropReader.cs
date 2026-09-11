using OttoLens.Model;

namespace OttoLens.Readers;

/// Item lying on the ground (spec 3.20). One block row carries the icon, name, stack count and
/// quality; stackable items also get a Stack meter so the panel has a shape. This reader fires
/// on every dropped item around a base, so it stays at one lookup and one sprite fetch.
internal sealed class ItemDropReader : ILensReader
{
    private static readonly Dictionary<string, Sprite?> IconCache = new(StringComparer.Ordinal);

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly LensItem _item = new();

    public Type TargetType => typeof(ItemDrop);

    public bool Enabled => OttoLensPlugin.ShowMisc.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var drop = (ItemDrop)target;
        ZNetView view = drop.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        // Vanilla GetHoverText calls Load() before reading m_itemData, but Hud.UpdateCrosshair
        // skips GetHoverText while TextViewer is visible. Call Load() here so the panel tick
        // coroutine and the TextViewer gap both see a current stack count.
        drop.Load();
        ItemDrop.ItemData data = drop.m_itemData;
        if (data == null || data.m_shared == null)
        {
            return null;
        }

        string token = data.m_shared.m_name;
        string name = LensFormat.Name(token);
        int stack = Mathf.Max(data.m_stack, 1);
        int maxStack = data.m_shared.m_maxStackSize;

        LensReport report = _report;
        report.Reset();
        report.Title = name;
        if (maxStack > 1)
        {
            float fraction = Mathf.Clamp01((float)stack / maxStack);
            report.PrimaryMeter = LensReport.Meter("Stack", fraction, LensFormat.Count(stack, maxStack), LensColor.Dim);
        }

        LensItem item = _item;
        item.Sprite = Icon(data, token);
        item.Name = name;
        item.Count = stack;
        item.Quality = data.m_quality > 1 ? data.m_quality : 0;

        LensItemBlock block = _block;
        block.Clear();
        block.Items.Add(item);
        report.Block0 = block;
        return report;
    }

    // The cache outlives a world unload: a destroyed sprite reads as Unity null and is fetched
    // again, so a stale entry cannot hand the panel a fake-null sprite that blanks the row art.
    private static Sprite? Icon(ItemDrop.ItemData item, string token)
    {
        string key = item.m_variant > 0 ? token + "#" + item.m_variant : token;
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
