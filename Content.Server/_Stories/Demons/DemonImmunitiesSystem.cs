using Content.Shared._Stories.Demons;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Mobs;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Server._Stories.Demons;

public sealed partial class DemonImmunitiesSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DemonImmunitiesComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<DemonImmunitiesComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<DemonImmunitiesComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnStartCollide(Entity<DemonImmunitiesComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.SmashWalls)
            return;

        if (ent.Comp.NextSmash > _timing.CurTime)
            return;

        var other = args.OtherEntity;

        if (!TryComp<DamageableComponent>(other, out var damageable))
            return;

        if (!Transform(other).Anchored)
            return;

        ent.Comp.NextSmash = _timing.CurTime + ent.Comp.SmashInterval;
        _damageable.TryChangeDamage((other, damageable), ent.Comp.SmashDamage, origin: ent);
    }

    private void OnPullAttempt(Entity<DemonImmunitiesComponent> ent, ref PullAttemptEvent args)
    {
        if (args.PulledUid == ent.Owner)
            args.Cancelled = true;
    }

    private void OnMobStateChanged(Entity<DemonImmunitiesComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (ent.Comp.VanishDelay is not { } delay)
            return;

        var uid = ent.Owner;
        Timer.Spawn(delay, () =>
        {
            if (!Deleted(uid))
                QueueDel(uid);
        });
    }
}
