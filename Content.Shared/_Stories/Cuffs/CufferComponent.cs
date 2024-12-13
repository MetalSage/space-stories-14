namespace Content.Shared.Cuffs;

[RegisterComponent]
public sealed partial class CufferComponent : Component
{
    [DataField("timeModifier")]
    public float? TimeModifier = 1;
}
