using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Spaf;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSpafSystem))]
public sealed partial class SpafComponent : Component
{
    // TODO: Add spaf status icon
    // [DataField]
    // public ProtoId<FactionIconPrototype> StatusIcon = "SpafFaction";

    [DataField]
    public HashSet<string> Actions = new();

    [DataField]
    public HashSet<EntityUid> GrantedActions = new();
}
