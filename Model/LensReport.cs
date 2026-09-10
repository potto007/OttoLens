using System.Text;

namespace OttoLens.Model;

/// Colour role for a meter fill, a value, or the headline. The panel maps roles to the
/// palette so readers never pick hex values.
public enum LensColor
{
    Good,
    Warn,
    Bad,
    Gold,
    Dim,
}

/// One status row: label, an optional 0..1 fraction for the bar, and the exact value text.
/// A null fraction renders no bar and right-aligns the value into the value column.
public readonly struct LensMeter
{
    public readonly string Label;
    public readonly float? Fraction;
    public readonly string ValueText;
    public readonly LensColor Color;
    /// True when the value counts down (time to next product). Informational for the panel.
    public readonly bool Drains;

    public LensMeter(string label, float? fraction, string valueText, LensColor color, bool drains = false)
    {
        Label = label;
        Fraction = fraction;
        ValueText = valueText;
        Color = color;
        Drains = drains;
    }
}

/// One row inside an item block. Sprite may be null; the panel keeps the tile and name.
public sealed class LensItem
{
    public Sprite? Sprite;
    public string Name = "";
    public int Count;
    public int Quality;

    public LensItem() { }

    public LensItem(Sprite? sprite, string name, int count, int quality = 0)
    {
        Sprite = sprite;
        Name = name;
        Count = count;
        Quality = quality;
    }
}

/// A group of item rows. Block 0 is contents or queue, block 1 is finished output.
public sealed class LensItemBlock
{
    public string? Label;
    /// Output rows show their count in the Good colour.
    public bool IsOutput;
    public readonly List<LensItem> Items = new();

    public void Clear()
    {
        Label = null;
        IsOutput = false;
        Items.Clear();
    }
}

/// Everything the panel renders for one target. Readers may keep one instance and
/// call Reset() at entry; the panel copies what it needs and does not hold the report.
public sealed class LensReport
{
    public string Title = "";
    public string? Headline;
    public LensColor HeadlineColor = LensColor.Gold;
    public LensMeter? PrimaryMeter;
    public LensMeter? Secondary1;
    public LensMeter? Secondary2;
    public LensItemBlock? Block0;
    public LensItemBlock? Block1;
    public string? Footer;
    /// Optional. When null the core derives one from title, headline, block shape and
    /// item names. Set it when a cheaper key exists, for example a ZDO data revision.
    public string? ContentSignature;

    private readonly StringBuilder _signature = new(128);

    public static LensMeter Meter(string label, float? fraction, string valueText, LensColor color, bool drains = false)
        => new(label, fraction, valueText, color, drains);

    public void Reset()
    {
        Title = "";
        Headline = null;
        HeadlineColor = LensColor.Gold;
        PrimaryMeter = null;
        Secondary1 = null;
        Secondary2 = null;
        Block0?.Clear();
        Block1?.Clear();
        Block0 = null;
        Block1 = null;
        Footer = null;
        ContentSignature = null;
    }

    /// Design section 7: a panel with no data stays hidden rather than showing a plate.
    public bool HasContent
        => PrimaryMeter.HasValue || Secondary1.HasValue || Secondary2.HasValue
           || (Block0 != null && Block0.Items.Count > 0)
           || (Block1 != null && Block1.Items.Count > 0)
           || !string.IsNullOrEmpty(Footer);

    /// Rebuild key: changes when rows, item names, block presence or headline change.
    public string Signature()
    {
        if (ContentSignature != null)
        {
            return ContentSignature;
        }

        StringBuilder sb = _signature;
        sb.Clear();
        sb.Append(Title).Append('|').Append(Headline).Append('|');
        AppendMeter(sb, PrimaryMeter);
        AppendMeter(sb, Secondary1);
        AppendMeter(sb, Secondary2);
        AppendBlock(sb, Block0);
        sb.Append('|');
        AppendBlock(sb, Block1);
        sb.Append('|').Append(Footer != null ? '1' : '0');
        return sb.ToString();
    }

    // Bar presence is part of the shape: a meter that gains a fraction needs a rebuild.
    private static void AppendMeter(StringBuilder sb, LensMeter? meter)
    {
        if (meter.HasValue)
        {
            sb.Append(meter.Value.Label).Append(meter.Value.Fraction.HasValue ? '#' : '-');
        }

        sb.Append('|');
    }

    private static void AppendBlock(StringBuilder sb, LensItemBlock? block)
    {
        if (block == null)
        {
            return;
        }

        sb.Append(block.Label).Append(':');
        List<LensItem> items = block.Items;
        for (int i = 0; i < items.Count; i++)
        {
            sb.Append(items[i].Name).Append(',');
        }
    }
}
