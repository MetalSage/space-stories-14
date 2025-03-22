using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Nightvision;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NightvisionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled"), AutoNetworkedField]
    public bool Enabled { get; set; } = false;

    [DataField]
    public string ToggleAction = "ToggleNightvisionAction";

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [DataField("toggleOnSound")]
    public SoundSpecifier? ToggleOnSound;
}

[RegisterComponent]
[NetworkedComponent]
public sealed partial class NightvisionClothingComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled")]
    public bool Enabled { get; set; } = true;

    [DataField("toggleOnSound")]
    public SoundSpecifier? ToggleOnSound;
}
public sealed partial class ToggleNightvisionEvent : InstantActionEvent { }
