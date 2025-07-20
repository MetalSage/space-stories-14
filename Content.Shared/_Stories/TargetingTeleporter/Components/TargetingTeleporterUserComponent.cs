using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.TargetingTeleporter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class TargetingTeleporterUserComponent : Component
{

    [DataField, AutoNetworkedField]
    public EntityUid? Teleporter;

    [DataField, AutoNetworkedField]
    public EntityUid? Eye;

    [DataField, AutoNetworkedField]
    public EntProtoId SetExitAction = "STActionTargettingTeleporterSetExitPortal";

    [DataField, AutoNetworkedField]
    public EntProtoId ExitAction = "STActionTargettingTeleporterExit";

    [DataField, AutoNetworkedField]
    public EntityUid? SetExitActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? ExitActionEntity;

}
