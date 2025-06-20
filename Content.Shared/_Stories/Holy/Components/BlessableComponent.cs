namespace Content.Shared._Stories.Holy;

[RegisterComponent]
public sealed partial class BlessableComponent : Component
{
    [DataField]
    public float TimeModifier = 1f;
}
