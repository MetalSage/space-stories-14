using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Weapons.Special.Garrote;

public abstract class SharedGarroteSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GarroteComponent, GarroteDoAfterEvent>(OnGarroteDoAfter);
    }

    private void OnGarroteDoAfter(EntityUid uid, GarroteComponent comp, GarroteDoAfterEvent args)
    {
        if (args.Target == null
            || !TryComp<MobStateComponent>(args.Target, out var mobState)
            || !TryComp<StatusEffectsComponent>(args.Target, out var statusEffectsComp))
            return;

        if (args.Cancelled || mobState.CurrentState != MobState.Alive)
            return;

        _damageable.TryChangeDamage(args.Target, comp.Damage, origin: args.User);

        _stun.TryStun(args.Target.Value, comp.DurationStatusEffects, true);
        _statusEffect.TryAddStatusEffect<MutedComponent>(args.Target.Value, "Muted", comp.DurationStatusEffects, refresh: true);
        Dirty(args.Target.Value, statusEffectsComp);

        args.Repeat = true;
    }

    /// <summary>
    ///     Checking whether the distance from the user to the target is set correctly.
    /// </summary>
    /// <remarks>
    ///     Does not check for the presence of TransformComponent.
    /// </remarks>
    public static bool IsRightTargetDistance(TransformComponent user, TransformComponent target, float maxUseDistance)
    {
        return (Math.Abs(user.LocalPosition.X - target.LocalPosition.X) <= maxUseDistance
            && Math.Abs(user.LocalPosition.Y - target.LocalPosition.Y) <= maxUseDistance);
    }

    /// <remarks>
    ///     Does not check for the presence of TransformComponent.
    /// </remarks>
    public static Direction GetEntityDirection(TransformComponent entityTransform)
    {
        double entityLocalRotation;

        // Checking that the number is positive
        if (entityTransform.LocalRotation.Degrees < 0)
            entityLocalRotation = 360 - Math.Abs(entityTransform.LocalRotation.Degrees);
        else
            entityLocalRotation = entityTransform.LocalRotation.Degrees;

        return entityLocalRotation switch
        {
            > 43.5d and < 136.5d => Direction.East,
            >= 136.5d and <= 223.5d => Direction.North,
            > 223.5d and < 316.5d => Direction.West,
            _ => Direction.South
        };
    }
}
