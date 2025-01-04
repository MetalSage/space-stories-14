using Content.Server.Maps;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Prison;

[RegisterComponent]
public sealed partial class StationPrisonComponent : Component
{
    [DataField]
    public ProtoId<GameMapPrototype> GameMap = "StoriesPrison";

    /// <summary>
    /// Тюрьма, приписанная к станции.
    /// </summary>
    [DataField]
    public EntityUid? Prison;
}
