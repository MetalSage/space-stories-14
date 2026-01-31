using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Empire.Components;

[RegisterComponent] [NetworkedComponent]
public sealed partial class EmpireComponent : Component
{
    [DataField] [ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "EmpireFaction";
}
