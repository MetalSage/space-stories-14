using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class GeneratedProtectiveBubbleComponent : Component
{
    [DataField]
    public EntityUid? Generator;
}
