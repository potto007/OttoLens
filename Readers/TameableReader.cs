using OttoLens.Model;

namespace OttoLens.Readers;

/// Tamed creatures, creatures being tamed, and pet pieces (spec 3.16). Taming and hunger come
/// straight from the ZDO so nothing here advances a timer; a pet piece has no Character, so it
/// only contributes the item on its own stand.
internal sealed class TameableReader : ILensReader
{
    private sealed class StandItem
    {
        public string Name = "";
        public Sprite? Sprite;
    }

    private readonly LensReport _report = new();
    private readonly LensItemBlock _block = new();
    private readonly Dictionary<long, StandItem> _standItems = new();

    public Type TargetType => typeof(Tameable);

    public bool Enabled => OttoLensPlugin.ShowCreatures.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var tameable = (Tameable)target;
        ZNetView view = tameable.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ZDO zdo = view.GetZDO();
        if (zdo == null)
        {
            return null;
        }

        LensReport report = _report;
        report.Reset();

        Character? character = tameable.m_character;
        MonsterAI? ai = tameable.m_monsterAI;
        bool frightened = ai != null && ai.IsAlerted();
        bool tamed = tameable.IsTamed();

        report.Title = ResolveName(tameable, zdo, tamed);

        int stars = character != null ? character.GetLevel() - 1 : 0;

        if (character == null)
        {
            // Pet piece: vanilla already words the status, only the stand item is new.
            FillStandItem(tameable, zdo, report);
            return report.HasContent ? report : null;
        }

        // Hunger mirrors Tameable.IsHungry without calling into anything that could tick.
        long feedTicks = Math.Max(0L, zdo.GetLong(ZDOVars.s_tameLastFeeding, 0L));
        double sinceFed = (ZNet.instance.GetTime() - new DateTime(feedTicks)).TotalSeconds;
        bool hungry = sinceFed > tameable.m_fedDuration;

        if (tamed)
        {
            float fedLeft = hungry ? 0f : (float)(tameable.m_fedDuration - sinceFed);
            float fraction = tameable.m_fedDuration > 0f ? Mathf.Clamp01(fedLeft / tameable.m_fedDuration) : 0f;
            report.PrimaryMeter = LensReport.Meter(
                "Hunger",
                fraction,
                hungry ? "Hungry" : LensFormat.Time(fedLeft),
                hungry ? LensColor.Bad : LensFormat.Ramp(fraction),
                drains: true);
        }
        else
        {
            float remaining = zdo.GetFloat(ZDOVars.s_tameTimeLeft, tameable.m_tamingTime);
            float progress = tameable.m_tamingTime > 0f ? 1f - Mathf.Clamp01(remaining / tameable.m_tamingTime) : 0f;
            // A frozen countdown is the cue that something blocks taming; the headline names it.
            LensColor color = hungry ? LensColor.Bad : frightened ? LensColor.Warn : LensColor.Gold;
            report.PrimaryMeter = LensReport.Meter("Tame", progress, LensFormat.Percent(progress), color);
            report.Secondary1 = LensReport.Meter("Next", null, LensFormat.Time(remaining), color, drains: true);
        }

        if (frightened)
        {
            report.Headline = "FRIGHTENED";
            report.HeadlineColor = LensColor.Warn;
        }
        else if (hungry)
        {
            report.Headline = "HUNGRY";
            report.HeadlineColor = LensColor.Bad;
        }
        else if (stars > 0)
        {
            report.Headline = stars == 1 ? "1 STAR" : stars + " STARS";
            report.HeadlineColor = LensColor.Gold;
        }

        FillStandItem(tameable, zdo, report);
        return report.HasContent ? report : null;
    }

    // Tameable.GetHoverName goes through GetText, which can write the author key when this
    // client owns the creature, so the custom name is read raw instead.
    private static string ResolveName(Tameable tameable, ZDO zdo, bool tamed)
    {
        if (tamed)
        {
            string custom = zdo.GetString(ZDOVars.s_tamedName);
            if (!string.IsNullOrEmpty(custom))
            {
                return StripTags(custom);
            }
        }

        return LensFormat.Name(tameable.GetName());
    }

    private static string StripTags(string text)
    {
        if (text.IndexOf('<') < 0)
        {
            return text;
        }

        var sb = new System.Text.StringBuilder(text.Length);
        bool inTag = false;
        foreach (char c in text)
        {
            if (c == '<')
            {
                inTag = true;
            }
            else if (c == '>')
            {
                inTag = false;
            }
            else if (!inTag)
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    // Only a Pet piece carries an ItemStand on the same object (Pet.Awake uses GetComponent).
    private void FillStandItem(Tameable tameable, ZDO zdo, LensReport report)
    {
        if (tameable.GetComponent<Pet>() == null)
        {
            return;
        }

        ItemStand? stand = tameable.GetComponent<ItemStand>();
        if (stand == null || !stand.HaveAttachment())
        {
            return;
        }

        // ItemStand.Interact ward-checks the take; the pet hover never carries the no access
        // line, so the stand item is gated here like ItemStandReader does.
        if (!LensReaders.HasWardAccess(stand))
        {
            return;
        }

        int hash = stand.GetAttachedItem();
        // The stand sits on the pet's own object, so it shares the tameable's ZDO.
        int variant = zdo.GetInt(ZDOVars.s_variant);
        int quality = zdo.GetInt(ZDOVars.s_quality, 1);

        StandItem? item = Resolve(hash, variant);
        if (item == null)
        {
            return;
        }

        LensItemBlock block = _block;
        block.Clear();
        block.Label = "Holding";
        block.Items.Add(new LensItem(item.Sprite, item.Name, 1, quality));
        report.Block0 = block;
    }

    private StandItem? Resolve(int hash, int variant)
    {
        long key = ((long)hash << 32) | (uint)variant;
        // A sprite that is C# null never existed; one that is only Unity null was destroyed
        // with its scene and is resolved again.
        if (_standItems.TryGetValue(key, out StandItem cached) && (cached.Sprite is null || cached.Sprite != null))
        {
            return cached;
        }

        ObjectDB db = ObjectDB.instance;
        if (db == null || !db.TryGetItemPrefab(hash, out GameObject prefab) || prefab == null)
        {
            return null;
        }

        ItemDrop? drop = prefab.GetComponent<ItemDrop>();
        if (drop == null || drop.m_itemData == null || drop.m_itemData.m_shared == null)
        {
            return null;
        }

        ItemDrop.ItemData.SharedData shared = drop.m_itemData.m_shared;
        var item = new StandItem { Name = LensFormat.Name(shared.m_name) };
        try
        {
            Sprite[] icons = shared.m_icons;
            if (icons != null && icons.Length > 0)
            {
                item.Sprite = icons[Mathf.Clamp(variant, 0, icons.Length - 1)];
            }
        }
        catch (Exception)
        {
            item.Sprite = null;
        }

        _standItems[key] = item;
        return item;
    }
}
