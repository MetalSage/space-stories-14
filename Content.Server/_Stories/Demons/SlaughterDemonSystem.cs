using Content.Server._Stories.Objectives.Systems;
using Content.Server.Polymorph.Components;
using Content.Server.Popups;
using Content.Shared._Stories.Demons;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._Stories.Demons;

public sealed partial class SlaughterDemonSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private STCounterObjectiveSystem _counterObjective = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedContainerSystem _container = default!;

    private static readonly SoundSpecifier ConsumeSound =
        new SoundPathSpecifier("/Audio/_Stories/Demons/Common/demon_consume.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlaughterDemonComponent, DemonPhasedOutWithPullEvent>(OnPhasedOutWithPull);
        SubscribeLocalEvent<SlaughterDemonComponent, SlaughterDemonConsumeDoAfterEvent>(OnConsumeDoAfter);
        SubscribeLocalEvent<SlaughterDemonComponent, DemonPhasedInEvent>(OnPhasedIn);
        SubscribeLocalEvent<SlaughterDemonComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<SlaughterDemonComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private EntityUid GetPersistentOwner(EntityUid demon)
    {
        if (TryComp<PolymorphedEntityComponent>(demon, out var poly)
            && poly.Parent is { } parent
            && !TerminatingOrDeleted(parent))
        {
            return parent;
        }

        return demon;
    }

    private void OnMobStateChanged(Entity<SlaughterDemonComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (_container.TryGetContainer(ent, SlaughterDemonComponent.ConsumedContainerId, out var container))
            _container.EmptyContainer(container);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlaughterDemonComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.BoostActive || comp.BoostEndTime > _timing.CurTime)
                continue;

            comp.BoostActive = false;
            _movement.RefreshMovementSpeedModifiers(uid);
        }
    }

    private void OnPhasedIn(Entity<SlaughterDemonComponent> ent, ref DemonPhasedInEvent args)
    {
        ent.Comp.BoostEndTime = _timing.CurTime + ent.Comp.BoostDuration;
        ent.Comp.BoostActive = true;
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefreshSpeed(Entity<SlaughterDemonComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.BoostActive && ent.Comp.BoostEndTime > _timing.CurTime)
            args.ModifySpeed(ent.Comp.BoostMultiplier, ent.Comp.BoostMultiplier);
    }

    private void OnPhasedOutWithPull(Entity<SlaughterDemonComponent> ent, ref DemonPhasedOutWithPullEvent args)
    {
        if (Deleted(args.Pulled))
            return;

        if (!_mobState.IsDead(args.Pulled) && !_mobState.IsCritical(args.Pulled))
            return;

        _transform.SetCoordinates(args.Pulled, Transform(ent).Coordinates);
        _audio.PlayPvs(ConsumeSound, ent);

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            ent,
            ent.Comp.ConsumeDuration,
            new SlaughterDemonConsumeDoAfterEvent(),
            ent,
            target: args.Pulled)
        {
            BreakOnMove = false,
            NeedHand = false,
        });
    }

    private void OnConsumeDoAfter(Entity<SlaughterDemonComponent> ent, ref SlaughterDemonConsumeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } victim || Deleted(victim))
            return;

        args.Handled = true;

        var isFullMeal = HasComp<HumanoidProfileComponent>(victim) || HasComp<BorgChassisComponent>(victim);
        var heal = isFullMeal
            ? ent.Comp.HealOnConsume
            : ent.Comp.HealOnMeagreConsume ?? ent.Comp.HealOnConsume;

        _damageable.TryChangeDamage(ent.Owner, heal, true);

        if (isFullMeal)
            _counterObjective.IncrementForMind(ent);

        _popup.PopupEntity(Loc.GetString("slaughter-demon-consume-complete"), ent, ent);

        _damageable.TryChangeDamage(victim, ent.Comp.ConsumeKillDamage, true);

        var holder = GetPersistentOwner(ent);
        var container = _container.EnsureContainer<Container>(holder, SlaughterDemonComponent.ConsumedContainerId);

        if (!_container.Insert(victim, container))
            QueueDel(victim);
    }
}
