using Robust.Shared.Map;

namespace Content.Server._Stories.Demons;

[RegisterComponent]
public sealed partial class ShadowDemonSpawnLocationComponent : Component
{
    [DataField]
    public int Attempts = 20;

    [DataField]
    public float DarkRadius = 5f;

    [DataField]
    public EntityCoordinates? Coords;
}
