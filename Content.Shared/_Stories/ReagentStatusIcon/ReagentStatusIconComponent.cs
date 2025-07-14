using Content.Shared.Chemistry.Reagent;
using Content.Shared.Mobs;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.ReagentStatusIcon;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReagentStatusIconComponent : Component
{
    [DataField]
    public List<MobState> AllowedStates = new();

    [DataField]
    public string Solution = "chemicals";

    [DataField]
    public ReagentId Reagent;

    [DataField]
    public ProtoId<ReagentIconPrototype> StatusIcon;
}
