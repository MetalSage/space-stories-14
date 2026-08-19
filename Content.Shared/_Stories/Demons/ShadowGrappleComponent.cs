using Content.Shared.Damage;

namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class ShadowGrappleComponent : Component
{
    [DataField]
    public float PullDuration = 1f;

    [DataField]
    public float ExtinguishRange = 3f;

    [DataField]
    public TimeSpan ImmobilizeDuration = TimeSpan.FromSeconds(4);

    [DataField(required: true)]
    public DamageSpecifier HitDamage = default!;
}
