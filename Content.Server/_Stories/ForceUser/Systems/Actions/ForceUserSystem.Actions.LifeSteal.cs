using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;

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

        var doAfterEventArgs = new DoAfterArgs(EntityManager,
            args.Performer,
            args.DoAfterTime,
            new LifeStolenEvent(),
            args.Target,
            args.Target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            Broadcast = true,
            NeedHand = true,
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

        _damageable.TryChangeDamage(target, args.Damage, out var dmg, true);

        if (dmg == null)
            return;

        _force.TryRemoveVolume(user, dmg.GetTotal().Float());

        foreach (var group in args.HealGroups)
        {
            var groupProto = _proto.Index<DamageGroupPrototype>(group);
            var spec = new DamageSpecifier();
            foreach (var type in groupProto.DamageTypes)
            {
                spec.DamageDict.TryAdd(type, dmg.GetTotal() * -2 / groupProto.DamageTypes.Count);
            }

            _damageable.TryChangeDamage(user, spec, true);
        }

        if (_mobState.IsDead(target))
        {
            var geneticGroup = _proto.Index<DamageGroupPrototype>("Genetic");
            var geneticSpec = new DamageSpecifier();
            foreach (var type in geneticGroup.DamageTypes)
            {
                geneticSpec.DamageDict.TryAdd(type, 10 / geneticGroup.DamageTypes.Count);
            }

            _damageable.TryChangeDamage(target, geneticSpec, true);
        }
        else
            args.Repeat = true;

        args.Handled = true;
    }
}
