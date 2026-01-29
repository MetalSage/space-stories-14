namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class ProtectedByProtectiveBubbleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ProtectiveBubble;

    [DataField("temperatureCoefficient")]
    public float TemperatureCoefficient;
}
