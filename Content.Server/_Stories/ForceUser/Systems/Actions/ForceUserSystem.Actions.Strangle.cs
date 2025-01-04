using Content.Shared.DoAfter;
using Content.Shared.Standing;
using Content.Shared.Gravity;
using Content.Shared.Speech.Muting;
using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Shared.Movement.Components;

namespace Content.Server._Stories.ForceUser;
public sealed partial class ForceUserSystem
{
    public const float DamageLimit = 100f;
    public void InitializeStrangle()
    {
        SubscribeLocalEvent<StrangleTargetEvent>(OnStrangleTargetEvent);
        SubscribeLocalEvent<StrangledEvent>(OnStrangledEvent);
    }
    private void OnStrangleTargetEvent(StrangleTargetEvent args)
    {
        if (args.Handled || _mobState.IsIncapacitated(args.Target))
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, seconds: args.DoAfterTime, new StrangledEvent(), args.Target, args.Target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            Broadcast = true,
            NeedHand = true
        };

        if (!_doAfterSystem.TryStartDoAfter(doAfterEventArgs))
            return;
        args.Handled = true;
    }
    private void OnStrangledEvent(StrangledEvent args)
    {
        if (args.Handled || args.Target == null)
            return;

        var uid = args.Target.Value;

        if (args.Cancelled)
        {
            Stop(uid);
            args.Handled = true;
            return;
        }

        if (_mobState.IsAlive(uid))
        {
            _movementSpeedModifier.ChangeBaseSpeed(uid, 0, 0, 0);
            RaiseLocalEvent(uid, new DropHandItemsEvent(), false);

            _statusEffect.TryRemoveStatusEffect(uid, "KnockedDown");
            _standingState.Stand(uid);

            EnsureComp<LiftingUpComponent>(uid);
            EnsureComp<MutedComponent>(uid);

            var dmg = _damageable.TryChangeDamage(uid, args.Damage, true);
            if (!_mobState.IsAlive(uid) || dmg != null && !_force.TryRemoveVolume(args.User, dmg.GetTotal().Float()))
            {
                Stop(uid);
                args.Handled = true;
                args.Repeat = false;
                return;
            }
        }

        args.Repeat = true;
        args.Handled = true;
    }
    private void Stop(EntityUid uid)
    {
        _movementSpeedModifier.ChangeBaseSpeed(uid, MovementSpeedModifierComponent.DefaultBaseWalkSpeed, MovementSpeedModifierComponent.DefaultBaseSprintSpeed, MovementSpeedModifierComponent.DefaultAcceleration);
        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
        RemComp<LiftingUpComponent>(uid);
        RemComp<MutedComponent>(uid);
    }
}
