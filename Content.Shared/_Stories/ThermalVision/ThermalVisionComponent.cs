using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._Stories.ThermalVision;

[RegisterComponent] [NetworkedComponent] [AutoGenerateComponentState(true)]
public sealed partial class ThermalVisionComponent : Component
{
    [DataField]
    public string ToggleAction = "ToggleThermalVisionAction";

    [DataField] [AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [ViewVariables(VVAccess.ReadWrite)] [DataField("enabled")] [AutoNetworkedField]
    public bool Enabled { get; set; }

    [ViewVariables(VVAccess.ReadWrite)] [DataField("innate")] [AutoNetworkedField]
    public bool Innate { get; set; }
}

public sealed partial class ToggleThermalVisionEvent : InstantActionEvent
{
}
