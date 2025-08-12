using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Holy;

[RegisterComponent, NetworkedComponent]
public sealed partial class HolyComponent : Component
{
    [DataField]
    public DamageSpecifier ProtectionDamage = new()
    {
        DamageDict = new()
        {
            { "Holy", 5 },
            { "Heat", 5 }
        }
    };

    [DataField]
    public ProtoId<DamageModifierSetPrototype> ProtectionDamageDamageModifierSet = "STHoly";

    [DataField]
    public SoundPathSpecifier? ProtectionSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    [DataField]
    public float ProtectionImpulseSpeed = 5f;

    [DataField]
    public float ProtectionImpulseLengthModifier = 5f;

    [DataField]
    public TimeSpan ProtectionKnockdownTime = TimeSpan.FromSeconds(3);
}
