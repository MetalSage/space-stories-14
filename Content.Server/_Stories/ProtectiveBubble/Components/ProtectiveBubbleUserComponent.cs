using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class ProtectiveBubbleUserComponent : Component
{
    [DataField]
    public EntProtoId StopProtectiveBubbleAction = "ActionStopProtectiveBubble";

    [DataField, AutoNetworkedField]
    public EntityUid? StopProtectiveBubbleActionEntity;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ProtectiveBubble;

    [DataField]
    public float VolumeCost = 5f;

    [DataField]
    public DamageSpecifier Regeneration = new()
    {
        DamageDict = {
        { "Blunt", -2.5f },
        { "Slash", -2.5f },
        { "Piercing", -5f },
        { "Heat", -2.5f }
        }
    };
}
