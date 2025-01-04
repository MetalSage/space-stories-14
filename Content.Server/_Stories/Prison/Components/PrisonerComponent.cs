namespace Content.Server._Stories.Prison;

[RegisterComponent]
public sealed partial class PrisonerComponent : Component
{
    [DataField]
    public float PacifiedTime = 600f;
}
