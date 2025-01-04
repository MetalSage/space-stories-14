namespace Content.Server.Objectives.Components;

[RegisterComponent]
public sealed partial class PickRandomJobPersonComponent : Component
{
    [DataField("jobID")]
    public string JobID { get; private set; } = "JediNt";
}
