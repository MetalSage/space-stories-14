using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class ProtectiveBubbleComponent : Component
{
    [DataField]
    public Color Color = Color.White;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? User;

    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> ProtectedEntities = new();

    [DataField("temperatureCoefficient")]
    public float TemperatureCoefficient = 0f;
}
