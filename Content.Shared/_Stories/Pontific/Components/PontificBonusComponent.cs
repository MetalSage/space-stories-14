namespace Content.Shared._Stories.Pontific;

[RegisterComponent]
public sealed partial class PontificFlameComponent : Component
{
    [DataField]
    public float DamageMultiplier = 1.25f;

    [DataField]
    public float SpeedMultiplier = 1.25f;
}
