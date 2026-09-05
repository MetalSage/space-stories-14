using Content.Server._Stories.Objectives.Systems;
using Content.Server.Popups;
using Content.Shared._Stories.Demons;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Stories.Demons;

public sealed partial class DemonWrapSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private ShadowGrappleSystem _grapple = default!;
    [Dependency] private ShadowCocoonSystem _cocoon = default!;
    [Dependency] private STCounterObjectiveSystem _counterObjective = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    private ISawmill _sawmill = default!;

    private static readonly SoundSpecifier CocoonSound =
        new SoundPathSpecifier("/Audio/_Stories/Demons/ShadowDemon/shadownode.ogg");

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("demon-wrap");

        SubscribeLocalEvent<DemonWrapComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<DemonWrapComponent, DemonWrapDoAfterEvent>(OnWrapDoAfter);
    }

    private void OnMeleeHit(Entity<DemonWrapComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || ent.Comp.IsWrapping)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (!_mobState.IsDead(hit) || !HasComp<HumanoidProfileComponent>(hit))
                continue;

            ent.Comp.IsWrapping = true;
            _popup.PopupEntity(Loc.GetString("shadow-demon-wrap-start"), ent, ent);

            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                ent,
                ent.Comp.WrapDuration,
                new DemonWrapDoAfterEvent(),
                ent,
                target: hit)
            {
                BreakOnMove = true,
            });
            break;
        }
    }

    private void OnWrapDoAfter(Entity<DemonWrapComponent> ent, ref DemonWrapDoAfterEvent args)
    {
        ent.Comp.IsWrapping = false;

        if (args.Cancelled || args.Handled || args.Target is not { } victim || Deleted(victim))
            return;

        args.Handled = true;

        var coords = Transform(victim).Coordinates;
        var cocoon = Spawn(ent.Comp.CocoonPrototype, coords);
        _audio.PlayPvs(CocoonSound, coords);

        if (TryComp<ShadowCocoonComponent>(cocoon, out var cocoonComp))
        {
            _cocoon.TryInsertBody((cocoon, cocoonComp), victim);
        }
        else
        {
            _sawmill.Error($"DemonWrapComponent.CocoonPrototype '{ent.Comp.CocoonPrototype}' has no ShadowCocoonComponent; " +
                            $"leaving the wrapped victim {ToPrettyString(victim)} where it is instead of deleting it.");
            QueueDel(cocoon);
        }

        if (_mind.TryGetMind(victim, out _, out _))
            _counterObjective.IncrementForMind(ent, victim);

        _grapple.ExtinguishNearby(coords, 3f);
    }
}
