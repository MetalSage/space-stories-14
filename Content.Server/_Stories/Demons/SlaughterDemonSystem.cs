using Content.Server._Stories.Objectives.Systems;
using Content.Server.Popups;
using Content.Shared._Stories.Demons;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server._Stories.Demons;

public sealed partial class SlaughterDemonSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private STCounterObjectiveSystem _counterObjective = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private DemonPhaseSystem _demonPhase = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PopupSystem _popup = default!;

    private static readonly SoundSpecifier ConsumeSound =
        new SoundPathSpecifier("/Audio/_Stories/Demons/Common/demon_consume.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlaughterDemonComponent, SlaughterDemonConsumeDoAfterEvent>(OnConsumeDoAfter);
        SubscribeLocalEvent<SlaughterDemonComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(Entity<SlaughterDemonComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (_container.TryGetContainer(ent, SlaughterDemonComponent.ConsumedContainerId, out var container))
            _container.EmptyContainer(container);
    }

    public bool TryStartConsume(Entity<SlaughterDemonComponent> ent, EntityUid victim)
    {
        if (Deleted(victim))
            return false;

        if (!_mobState.IsDead(victim) && !_mobState.IsCritical(victim))
        {
            _popup.PopupEntity(Loc.GetString("slaughter-demon-victim-alive-fail"), ent, ent, PopupType.SmallCaution);
            return false;
        }

        _audio.PlayPvs(ConsumeSound, ent);
        _popup.PopupEntity(Loc.GetString("slaughter-demon-consume-start"), ent, PopupType.MediumCaution);

        var args = new DoAfterArgs(EntityManager,
            ent,
            ent.Comp.ConsumeDuration,
            new SlaughterDemonConsumeDoAfterEvent(),
            ent,
            target: victim)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
            DistanceThreshold = 2f,
        };

        return _doAfter.TryStartDoAfter(args);
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

        QueueDel(victim);

        if (TryComp<DemonPhaseComponent>(ent, out var phase))
            _demonPhase.StartPhase((ent.Owner, phase));
    }
}
