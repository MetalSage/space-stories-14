namespace Content.Server._Stories.Objectives.Components;

[RegisterComponent]
public sealed partial class STSlaughterFluffConditionComponent : Component
{
    [DataField(required: true)]
    public List<LocId> Lines = new();
}
