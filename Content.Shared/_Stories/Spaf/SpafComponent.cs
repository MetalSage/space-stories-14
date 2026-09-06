using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Spaf;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafSystem))]
public sealed partial class SpafComponent : Component
{
    [DataField]
    public HashSet<string> Actions = new();

    [DataField]
    public HashSet<EntityUid> GrantedActions = new();
}
