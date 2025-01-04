using Content.Shared._Stories.ForceUser.Actions.Events;
using Robust.Shared.Map;
using System.Numerics;
using Content.Shared.DoAfter;
using Content.Shared.Standing;
using Content.Shared.Gravity;
using Content.Server.Speech.Muting;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Movement.Components;
using Content.Shared.Damage;
using Robust.Shared.Random;

namespace Content.Server._Stories.ForceUser;
public sealed partial class ForceUserSystem
{
    public void InitializeSteal()
    {
        SubscribeLocalEvent<StealLifeTargetEvent>(OnSteal);
        SubscribeLocalEvent<LifeStolenEvent>(OnStolen);
    }
    private void OnSteal(StealLifeTargetEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, seconds: args.DoAfterTime, new LifeStolenEvent(), args.Target, args.Target)
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
    private void OnStolen(LifeStolenEvent args)
    {
        if (args.Handled || args.Target == null || args.Cancelled || _mobState.IsDead(args.Target.Value))
            return;

        var target = args.Target.Value;
        var user = args.User;

        var dmg = _damageable.TryChangeDamage(target, args.Damage, true);

        if (dmg == null)
            return;

        _force.TryRemoveVolume(user, dmg.GetTotal().Float());

        foreach (var group in args.HealGroups)
        {
            _damageable.TryChangeDamage(user, new DamageSpecifier(_proto.Index<DamageGroupPrototype>(group), dmg.GetTotal() * -2), true);
        }

        if (_mobState.IsDead(target))
            _damageable.TryChangeDamage(target, new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Genetic"), 10), true);
        else args.Repeat = true;

        args.Handled = true;
    }
}
