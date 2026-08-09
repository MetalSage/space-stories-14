using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.PrisonerCollar;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]  
public sealed partial class PrisonerCollarComponent : Component
{
    [DataField]
    public float FailChance = 0.25f;

    [DataField]
    public DamageSpecifier FailDamage = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Blunt", 15 },
            { "Slash", 20 },
            { "Heat", 10 },
        },
    };

    [DataField, AutoNetworkedField] 
    public NetEntity? LinkedConsole;

    [DataField, AutoNetworkedField] 
    public PrisonerCollarState State = PrisonerCollarState.Active;
}

[Serializable, NetSerializable] 
public enum PrisonerCollarState : byte
{
    Active = 0,
    EmpDisabled = 1,
    Disarmed = 2,
}
