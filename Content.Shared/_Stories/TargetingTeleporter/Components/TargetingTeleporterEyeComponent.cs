using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.TargetingTeleporter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class TargetingTeleporterEyeComponent : Component
{

    [DataField, AutoNetworkedField]
    public EntityUid? Teleporter;

    [DataField, AutoNetworkedField]
    public EntityUid? User;

}
