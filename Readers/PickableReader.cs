using OttoLens.Model;

namespace OttoLens.Readers;

/// Berry bush, mushroom, flower, and every other Pickable (spec 3.9). Unpicked shows the
/// item and yield; picked shows the respawn countdown when showRespawn is on and the pickable
/// respawns at all.
internal sealed class PickableReader : ILensReader
{
    private static readonly Dictionary<string, Sprite?> SpriteCache = new(StringComparer.Ordinal);

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly LensItem _item = new();

    public Type TargetType => typeof(Pickable);

    public bool Enabled => OttoLensPlugin.ShowPickables.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var pickable = (Pickable)target;
        ZNetView view = pickable.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        // Zero means the game switched it off (seasonal spawn): vanilla hides it too.
        if (pickable.GetEnabled == 0)
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        if (zdo == null)
        {
            return null;
        }

        ItemDrop? drop = pickable.m_itemPrefab != null ? pickable.m_itemPrefab.GetComponent<ItemDrop>() : null;
        string name = !string.IsNullOrEmpty(pickable.m_overrideName)
            ? LensFormat.Name(pickable.m_overrideName)
            : drop != null ? LensFormat.Name(drop.m_itemData.m_shared.m_name) : LensFormat.Name(pickable.name);

        // The ZDO is the synced truth; the field catches the RPC that lands before the ZDO does.
        bool picked = pickable.GetPicked() || zdo.GetBool(ZDOVars.s_picked, pickable.m_defaultPicked);

        LensReport report = _report;
        report.Reset();
        report.Title = name;

        if (!picked)
        {
            _item.Sprite = drop != null ? IconFor(drop.gameObject.name, drop) : null;
            _item.Name = name;
            _item.Count = Yield(pickable);
            _item.Quality = 0;

            LensItemBlock block = _block;
            block.Clear();
            block.Items.Add(_item);
            report.Block0 = block;
            return report;
        }

        // Spec 3.9 item 2: the picked panel is opt in, so a picked patch stays quiet by default
        // while the unpicked yield readout keeps working.
        if (!OttoLensPlugin.ShowRespawn.Value || pickable.m_respawnTimeMinutes <= 0f)
        {
            return null;
        }

        float total = pickable.m_respawnTimeMinutes * 60f;
        long pickedTicks = zdo.GetLong(ZDOVars.s_pickedTime, 0L);
        float remaining;
        if (pickedTicks == 0L)
        {
            // Owner has not started the timer yet (it ticks once a minute): full wait.
            remaining = total;
        }
        else if (pickedTicks == 1L)
        {
            // Vanilla sentinel for an initial time that was already due.
            remaining = 0f;
        }
        else
        {
            double elapsed = (ZNet.instance.GetTime() - new DateTime(pickedTicks)).TotalSeconds;
            remaining = total - (float)elapsed;
        }

        bool ready = remaining <= 0f;
        float fraction = total > 0f ? Mathf.Clamp01(1f - remaining / total) : 1f;
        report.Headline = ready ? "READY" : "PICKED";
        report.HeadlineColor = ready ? LensColor.Good : LensColor.Dim;
        report.PrimaryMeter = LensReport.Meter(
            "Respawn",
            fraction,
            LensFormat.TimeWithDays(remaining),
            ready ? LensColor.Good : LensColor.Gold,
            drains: true);
        return report;
    }

    /// Mirrors Pickable.RPC_Pick: the world resource rate scales the drop unless the prefab
    /// opts out, and the scaled count never falls under m_minAmountScaled. Game.ScaleDrops is
    /// a pure read of Game.m_resourceRate and m_nonScaledDropTypes.
    private static int Yield(Pickable pickable)
    {
        if (pickable.m_dontScale || Game.instance == null || pickable.m_itemPrefab == null)
        {
            return pickable.m_amount;
        }

        return Mathf.Max(pickable.m_minAmountScaled, Game.instance.ScaleDrops(pickable.m_itemPrefab, pickable.m_amount));
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
