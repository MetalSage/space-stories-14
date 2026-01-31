using Robust.Shared.GameStates;

namespace Content.Shared._Stories.TargetingTeleporter;

[RegisterComponent] [NetworkedComponent] [AutoGenerateComponentState(true)]
public sealed partial class TargetingTeleporterEyeComponent : Component
{
    [DataField] [AutoNetworkedField]
    public EntityUid? Teleporter;

    [DataField] [AutoNetworkedField]
    public EntityUid? User;
}
