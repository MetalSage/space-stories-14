using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Vision.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class VisionComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsActive;

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
}
