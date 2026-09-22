using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Stories.Fireman;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(StoriesFiremanCarrySystem))]
public sealed partial class CanFiremanCarryComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Carrying;

    [DataField, AutoNetworkedField]
    public bool AggressiveGrab;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan PullTime;

    [DataField, AutoNetworkedField]
    public TimeSpan AggressiveGrabDelay = TimeSpan.FromSeconds(2);
}
