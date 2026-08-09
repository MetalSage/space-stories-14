using Robust.Shared.GameStates;

namespace Content.Shared._Stories.PrisonerCollar;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]  
public sealed partial class PrisonerCollarConsoleComponent : Component
{
    [DataField, AutoNetworkedField] 
    public List<NetEntity> LinkedCollars = new();
}
