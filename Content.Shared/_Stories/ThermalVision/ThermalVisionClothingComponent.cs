using Robust.Shared.GameStates;

namespace Content.Shared._Stories.ThermalVision;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ThermalVisionClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled = true;
}
