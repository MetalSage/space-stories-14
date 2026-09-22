using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Explosion.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StoriesExplosionEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId? Explosion = "STExplosionEffectGrenade";

    [DataField, AutoNetworkedField]
    public EntProtoId? ShockWave = "STExplosionEffectGrenadeShockWave";
}
