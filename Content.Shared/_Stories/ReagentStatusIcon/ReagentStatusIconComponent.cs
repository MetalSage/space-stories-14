using Content.Shared.Chemistry.Reagent;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.ReagentStatusIcon;

[RegisterComponent]
public sealed partial class ReagentStatusIconComponent : Component
{
    [DataField]
    public string Solution = "chemicals";

    [DataField]
    public ReagentId Reagent;

    [DataField]
    public ProtoId<ReagentIconPrototype> StatusIcon;
}
