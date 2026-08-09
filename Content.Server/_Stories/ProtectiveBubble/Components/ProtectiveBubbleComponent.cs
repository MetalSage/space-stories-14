namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class ProtectiveBubbleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public float CurrentLifeTime = 240f;

    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> ProtectedEntities = new();

    [DataField("temperatureCoefficient")]
    public float TemperatureCoefficient;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField] 
    public EntityUid? User;
}
