namespace Content.Shared._Stories.LightSensitivity;

[RegisterComponent]
public sealed partial class LightSensitivityComponent : Component
{
    [DataField]
    public float LookupRange = 8f;

    [DataField]
    public float DarknessThreshold = 0.2f;
}
