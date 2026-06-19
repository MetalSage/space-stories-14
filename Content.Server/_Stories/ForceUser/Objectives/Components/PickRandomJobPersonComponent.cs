namespace Content.Server.Objectives.Components;

[RegisterComponent]
public sealed partial class PickRandomJobPersonComponent : Component
{
    public bool Handled = false;

    public EntityUid MindId;

    [DataField("jobID")]
    public string JobID { get; private set; } = "GuardianNt";
}
