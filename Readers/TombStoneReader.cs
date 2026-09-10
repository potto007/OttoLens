using OttoLens.Model;

namespace OttoLens.Readers;

/// Tombstone (spec 3.15). Shows the owner and how many item slots the grave holds. The
/// contents are not listed because a grave usually holds a whole inventory, which would only
/// overflow the panel. Registered before ContainerReader, which would otherwise claim the
/// Container that sits on the same object.
internal sealed class TombStoneReader : ILensReader
{
    private readonly LensReport _report = new();

    private TombStone? _cachedStone;
    private uint _cachedRevision;
    private string _cachedSignature = "";

    public Type TargetType => typeof(TombStone);

    public bool Enabled => OttoLensPlugin.ShowMisc.Value;

    public LensReport? Read(Component target, GameObject hover)
    {
        var stone = (TombStone)target;
        ZNetView view = stone.m_nview;
        if (view == null || !view.IsValid())
        {
            return null;
        }

        ZDO? zdo = view.GetZDO();
        if (zdo == null)
        {
            return null;
        }

        Container container = stone.m_container;
        if (container == null)
        {
            return null;
        }

        Inventory? inventory = container.GetInventory();
        if (inventory == null)
        {
            return null;
        }

        // Vanilla hides an empty grave (its hover text is empty); so do we.
        int used = inventory.NrOfItems();
        if (used == 0)
        {
            return null;
        }

        int capacity = inventory.GetWidth() * inventory.GetHeight();
        float fraction = capacity > 0 ? Mathf.Clamp01((float)used / capacity) : 0f;

        LensReport report = _report;
        report.Reset();
        report.Title = LensFormat.Name(stone.m_text);

        string owner = stone.GetOwnerName();
        if (!string.IsNullOrEmpty(owner))
        {
            report.Headline = owner;
            report.HeadlineColor = stone.IsOwner() ? LensColor.Good : LensColor.Gold;
        }

        report.PrimaryMeter = LensReport.Meter("Items", fraction, LensFormat.Count(used, capacity), LensColor.Dim);

        uint revision = zdo.DataRevision;
        if (!ReferenceEquals(stone, _cachedStone) || revision != _cachedRevision)
        {
            _cachedStone = stone;
            _cachedRevision = revision;
            _cachedSignature = string.Concat("tomb:", revision.ToString(), ":", owner);
        }

        report.ContentSignature = _cachedSignature;
        return report;
    }
}
