using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class ProtectiveBubbleUserComponent : Component
{
    [DataField]
    public EntProtoId StopAction = "ActionStopProtectiveBubble";

    [DataField]
    public EntityUid? StopActionEntity;

    [DataField]
    public EntityUid? ProtectiveBubble;
}
