using Content.Shared.Flash;
using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Debuff;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedFlashSystem))]
public sealed partial class FlashDebuffComponent : Component
{
    [DataField, AutoNetworkedField] 
    public bool BlockFlashImmunity;

    [DataField, AutoNetworkedField] 
    public float CoefficientDuration = 2f;

    [DataField, AutoNetworkedField] 
    public bool Enabled { get; set; } = true;
}
