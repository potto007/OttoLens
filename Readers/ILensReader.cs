using OttoLens.Model;

namespace OttoLens.Readers;

/// One reader per vanilla component type. The registry resolves the hover GameObject to a
/// component of TargetType with GetComponentInParent, then calls Read.
public interface ILensReader
{
    /// The vanilla component this reader handles (typeof(Container), typeof(Smelter), ...).
    Type TargetType { get; }

    /// Config gate for the target group. Checked before any component walk, so a disabled
    /// group costs one bool read per frame.
    bool Enabled { get; }

    /// Return null to show nothing. target is the resolved component of TargetType; hover is
    /// the raw GameObject under the crosshair (collider or rigidbody object, may be a child).
    /// Never write a ZDO. Never call the getters listed in spec 6.2.
    LensReport? Read(Component target, GameObject hover);
}
