using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Bioluminescence;

[RegisterComponent]
public sealed partial class BioluminescenceComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)] [DataField("action")]
    public EntProtoId Action = "TurnBioluminescenceAction";
}

public sealed partial class TurnBioluminescenceEvent : InstantActionEvent
{
}
