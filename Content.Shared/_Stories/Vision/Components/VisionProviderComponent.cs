using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Vision.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class VisionProviderComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsActive = true;

    [DataField, AutoNetworkedField]
    public bool DrawLighting = true;

    [DataField, AutoNetworkedField]
    public bool DrawShadows = true;

    [DataField, AutoNetworkedField]
    public bool DrawFov = true;

    [DataField, AutoNetworkedField]
    public bool ThermalVision;

    [DataField, AutoNetworkedField]
    public string? Shader;

    [DataField, AutoNetworkedField]
    public string? ThermalShader;

    [DataField, AutoNetworkedField]
    public Color? AmbientColor;

    [DataField, AutoNetworkedField]
    public Color? ThermalAmbientColor;

    [DataField, AutoNetworkedField]
    public int Priority;

    [DataField]
    public EntProtoId? ToggleAction;

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [DataField]
    public ProtoId<AlertPrototype>? ToggleAlert;
}
