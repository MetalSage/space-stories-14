namespace Content.Server.Objectives.Components;

[RegisterComponent]
public sealed partial class PickRandomJobPersonComponent : Component
{
    [DataField("jobID")]
    public string JobID { get; private set; } = "GuardianNt";

    public EntityUid MindId;

    public bool Handled = false;
}
