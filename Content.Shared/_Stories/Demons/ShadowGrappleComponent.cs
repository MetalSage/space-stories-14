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
    public TimeSpan SlowdownDuration = TimeSpan.FromSeconds(3);

    [DataField]
    public float SlowdownWalkModifier = 0.35f;

    [DataField]
    public float SlowdownSprintModifier = 0.35f;

    [DataField(required: true)]
    public DamageSpecifier HitDamage = default!;
}
