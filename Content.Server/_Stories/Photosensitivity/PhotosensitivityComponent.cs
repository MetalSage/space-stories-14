using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Photosensitivity;

[RegisterComponent]
public sealed partial class PhotosensitivityComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)] [DataField("damage")]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Heat", 1 },
        },
    };


    [ViewVariables(VVAccess.ReadWrite)] [DataField("damageInSpace")]
    public DamageSpecifier DamageInSpace = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Heat", 5 },
        },
    };

    [DataField("darknessHealing")]
    public DamageSpecifier DarknessHealing = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Blunt", -5 },
            { "Slash", -5 },
            { "Piercing", -5 },
            { "Heat", -5 },
            { "Shock", -5 },
        },
    };

    [ViewVariables(VVAccess.ReadWrite)] [DataField("enabled")]
    public bool Enabled = true;
}
