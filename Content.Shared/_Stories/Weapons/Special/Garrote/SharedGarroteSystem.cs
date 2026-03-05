using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Weapons.Special.Garrote;

public abstract class SharedGarroteSystem : EntitySystem
{
    private static readonly EntProtoId MutedStatusEffect = "Muted";

    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] protected readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GarroteComponent, GarroteDoAfterEvent>(OnGarroteDoAfter);
    }

    private void OnGarroteDoAfter(EntityUid uid, GarroteComponent comp, GarroteDoAfterEvent args)
    {
        if (args.Target == null || !TryComp<MobStateComponent>(args.Target, out var mobState))
            return;

        if (args.Cancelled || mobState.CurrentState != MobState.Alive)
            return;

        _damageable.TryChangeDamage(args.Target.Value, comp.Damage, origin: args.User);

        _stun.TryAddStunDuration(args.Target.Value, comp.DurationStatusEffects);
        _statusEffect.TrySetStatusEffectDuration(args.Target.Value, MutedStatusEffect, comp.DurationStatusEffects);

        args.Repeat = true;
    }

    /// <summary>
    /// Checking whether the distance from the user to the target is set correctly.
    /// </summary>
    /// <remarks>
    /// Does not check for the presence of TransformComponent.
    /// </remarks>
    public bool IsRightTargetDistance(TransformComponent user, TransformComponent target, float maxUseDistance)
    {
        var userPosition = _transformSystem.GetWorldPositionRotation(user).WorldPosition;
        var targetPosition = _transformSystem.GetWorldPositionRotation(target).WorldPosition;

        return Math.Abs(userPosition.X - targetPosition.X) <= maxUseDistance
               && Math.Abs(userPosition.Y - targetPosition.Y) <= maxUseDistance;
    }

    /// <remarks>
    /// Does not check for the presence of TransformComponent.
    /// </remarks>
    public Direction GetEntityDirection(TransformComponent entityTransform)
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
            _ => Direction.South,
        };
    }
}
