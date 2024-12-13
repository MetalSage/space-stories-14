using Content.Shared.Actions;
namespace Content.Shared._Stories.Bioluminescence;

[RegisterComponent]
public sealed partial class BioluminescenceComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("action")]
    public string Action = "TurnBioluminescenceAction";
}

public sealed partial class TurnBioluminescenceEvent : InstantActionEvent
{
}
