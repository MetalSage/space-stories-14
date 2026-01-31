using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Holy;

[RegisterComponent] [NetworkedComponent]
public sealed partial class HolyComponent : Component
{
    [DataField]
    public DamageSpecifier ProtectionDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Holy", 5 },
            { "Heat", 5 },
        },
    };

    [DataField]
    public ProtoId<DamageModifierSetPrototype> ProtectionDamageDamageModifierSet = "STHoly";

    [DataField]
    public float ProtectionImpulseLengthModifier = 5f;

    [DataField]
    public float ProtectionImpulseSpeed = 5f;

    [DataField]
    public TimeSpan ProtectionKnockdownTime = TimeSpan.FromSeconds(3);

    [DataField]
    public SoundPathSpecifier? ProtectionSound = new("/Audio/Effects/lightburn.ogg");
}
