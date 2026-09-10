using OttoLens.Model;

namespace OttoLens.Readers;

/// Feast (spec 3.19). Primary meter is servings left over the full count and block 0 carries
/// the food item with the servings as its count. Nothing shows out of use range or when the
/// feast is finished, matching where vanilla shows too far or nothing at all.
internal sealed class FeastReader : ILensReader
{
    private static readonly Dictionary<string, Sprite?> IconCache = new(StringComparer.Ordinal);

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly LensItem _item = new();

    public Type TargetType => typeof(Feast);

    public bool Enabled => OttoLensPlugin.ShowMisc.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var feast = (Feast)target;
        ZNetView view = feast.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ItemDrop food = feast.m_foodItem;
        if (food == null || food.m_itemData == null || food.m_itemData.m_shared == null)
        {
            return null;
        }

        Player player = Player.m_localPlayer;
        if (player == null || !feast.InUseDistance(player))
        {
            return null;
        }

        int stack = feast.GetStack();
        if (stack <= 0)
        {
            return null;
        }

        int max = feast.m_eatStacks;
        float fraction = max > 0 ? Mathf.Clamp01((float)stack / max) : 1f;
        string token = food.m_itemData.m_shared.m_name;
        string name = LensFormat.Name(token);

        LensReport report = _report;
        report.Reset();
        report.Title = name;
        report.PrimaryMeter = LensReport.Meter("Servings", max > 0 ? fraction : (float?)null, LensFormat.Count(stack, max), LensFormat.Ramp(fraction));

        LensItem item = _item;
        item.Sprite = Icon(food.m_itemData, token);
        item.Name = name;
        item.Count = stack;
        item.Quality = 0;

        LensItemBlock block = _block;
        block.Clear();
        block.Label = "Food";
        block.Items.Add(item);
        report.Block0 = block;
        return report;
    }

    private static Sprite? Icon(ItemDrop.ItemData item, string token)
    {
        if (IconCache.TryGetValue(token, out Sprite? cached))
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

        IconCache[token] = sprite;
        return sprite;
    }
}
