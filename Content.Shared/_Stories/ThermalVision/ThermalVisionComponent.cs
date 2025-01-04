using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._Stories.ThermalVision;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ThermalVisionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled"), AutoNetworkedField]
    public bool Enabled { get; set; } = false;
    [ViewVariables(VVAccess.ReadWrite), DataField("innate"), AutoNetworkedField]
    public bool Innate { get; set; } = false;
    [DataField]
    public string ToggleAction = "ToggleThermalVisionAction";
    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;
}
public sealed partial class ToggleThermalVisionEvent : InstantActionEvent { }
