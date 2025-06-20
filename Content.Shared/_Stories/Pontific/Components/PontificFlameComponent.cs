using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Pontific;

[RegisterComponent, NetworkedComponent]
public sealed partial class PontificFlameComponent : Component
{
    [DataField]
    public float DamageMultiplier = 1.25f;

    [DataField]
    public float SpeedMultiplier = 1.25f;
}
