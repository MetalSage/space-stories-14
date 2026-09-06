namespace Content.Shared._Stories.Holy;

[RegisterComponent]
public sealed partial class UnholyComponent : Component
{
    [DataField]
    public bool Detectable = true;

    [DataField]
    public bool IgnoreProtectionImpulse;

    [DataField]
    public float ResistanceCoefficient = 1f;
}
