namespace Content.Server._Stories.ForceUser.Components;

[RegisterComponent]
public sealed partial class PassiveGhostBooComponent : Component
{
    [DataField("range")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Range = 15f;

    [DataField("seconds")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Seconds = 5f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float ActiveSeconds = 5f;

    [DataField("maxTargets"), ViewVariables(VVAccess.ReadWrite)]
    public int MaxTargets = 10;
}
