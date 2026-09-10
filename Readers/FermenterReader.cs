using OttoLens.Model;

namespace OttoLens.Readers;

/// Fermentation barrel (spec 3.5). Primary meter is the time to the next product, draining;
/// block 0 holds the input item while it ferments, block 1 the mead it will produce.
internal sealed class FermenterReader : ILensReader
{
    private static readonly Dictionary<int, Sprite?> SpriteCache = new();

    private readonly LensReport _report = new();
    private readonly LensItemBlock _contents = new();
    private readonly LensItemBlock _output = new();

    public Type TargetType => typeof(Fermenter);

    public bool Enabled => OttoLensPlugin.ShowFermenting.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var fermenter = (Fermenter)target;
        ZNetView view = fermenter.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        if (!LensReaders.HasWardAccess(target))
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        int content = zdo.GetInt(ZDOVars.s_content);
        if (content == 0)
        {
            // Empty barrel: vanilla already says so and there is no timer to show.
            return null;
        }

        // Vanilla returns -1 when no start time is stored; treat that as just started.
        long startTicks = zdo.GetLong(ZDOVars.s_startTime, 0L);
        double elapsed = startTicks == 0L
            ? 0.0
            : (ZNet.instance.GetTime() - new DateTime(startTicks)).TotalSeconds;
        float duration = fermenter.m_fermentationDuration;
        bool ready = elapsed > duration;
        float remaining = ready ? 0f : Mathf.Max(0f, duration - (float)elapsed);
        // The owner resets the start time while the barrel is uncovered, so the countdown
        // freezes at full; colour says stalled without repeating vanilla's warning line.
        bool stalled = !ready && (!fermenter.m_hasRoof || fermenter.m_exposed);

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(fermenter.m_name);

        if (ready)
        {
            report.Headline = "READY";
            report.HeadlineColor = LensColor.Good;
        }

        float fraction = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;
        LensColor color = ready ? LensColor.Good : stalled ? LensColor.Warn : LensColor.Gold;
        report.PrimaryMeter = LensReport.Meter("Next", fraction, LensFormat.Time(remaining), color, drains: true);

        // A barrel holding a removed recipe has no conversion; vanilla null checks this too.
        Fermenter.ItemConversion? conversion = fermenter.GetItemConversion(content);
        if (conversion == null)
        {
            return report;
        }

        if (!ready && conversion.m_from != null)
        {
            _contents.Clear();
            _contents.Items.Add(MakeItem(conversion.m_from, 1));
            report.Block0 = _contents;
        }

        if (conversion.m_to != null)
        {
            _output.Clear();
            _output.Label = ready ? null : "Makes";
            _output.IsOutput = true;
            _output.Items.Add(MakeItem(conversion.m_to, conversion.m_producedItems));
            report.Block1 = _output;
        }

        return report;
    }

    private static LensItem MakeItem(ItemDrop drop, int count)
    {
        ItemDrop.ItemData data = drop.m_itemData;
        return new LensItem(GetSprite(drop.gameObject.name, data), LensFormat.Name(data.m_shared.m_name), count);
    }

    private static Sprite? GetSprite(string prefabName, ItemDrop.ItemData data)
    {
        int key = prefabName.GetStableHashCode();
        if (SpriteCache.TryGetValue(key, out Sprite? cached))
        {
            return cached;
        }

        Sprite? sprite = null;
        try
        {
            sprite = data.GetIcon();
        }
        catch (Exception)
        {
            // A missing icon array leaves the tile blank; the name still renders.
        }

        SpriteCache[key] = sprite;
        return sprite;
    }
}
