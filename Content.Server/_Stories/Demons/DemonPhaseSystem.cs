using Content.Server._Stories.Photosensitivity;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._Stories.Demons;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Content.Shared.Fluids.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Stories.Demons;

public sealed partial class DemonPhaseSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private PhotosensitivitySystem _photosensitivity = default!;
    [Dependency] private PolymorphSystem _polymorph = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SlaughterDemonSystem _slaughterDemon = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;

    private static readonly ProtoId<ReagentPrototype> BloodReagent = "Blood";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DemonPhaseComponent, DemonPhaseActionEvent>(OnPhaseAction);
        SubscribeLocalEvent<DemonPhaseComponent, DemonPhaseDoAfterEvent>(OnPhaseDoAfter);
        SubscribeLocalEvent<DemonPhaseComponent, DemonRiseDoAfterEvent>(OnRiseDoAfter);
        SubscribeLocalEvent<DemonSpawnPhasedComponent, MindAddedMessage>(OnMindAdded);
    }

    private void OnMindAdded(Entity<DemonSpawnPhasedComponent> ent, ref MindAddedMessage args)
    {
        RemComp<DemonSpawnPhasedComponent>(ent);

        if (!TryComp<DemonPhaseComponent>(ent, out var phase) || HasComp<PolymorphedEntityComponent>(ent))
            return;

        _polymorph.PolymorphEntity(ent, phase.PhasePolymorph);
    }

    public bool TryPhase(EntityUid uid, DemonPhaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        if (!IsValidAnchor((uid, comp)))
            return false;

        var coords = Transform(uid).Coordinates;

        if (_polymorph.PolymorphEntity(uid, comp.PhasePolymorph) is not { } phased)
            return false;

        Spawn(comp.PhaseOutEffect, coords);
        _audio.PlayPvs(comp.PhaseOutSound, coords);
        return true;
    }

    private void OnPhaseAction(Entity<DemonPhaseComponent> ent, ref DemonPhaseActionEvent args)
    {
        if (args.Handled)
            return;

        if (!IsValidAnchor(ent))
        {
            _popup.PopupEntity(Loc.GetString(ent.Comp.FailPopup), ent, ent);
            return;
        }

        if (HasComp<PolymorphedEntityComponent>(ent))
        {
            args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                ent,
                ent.Comp.RiseDuration,
                new DemonRiseDoAfterEvent(),
                ent)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
            });
            return;
        }

        EntityUid? pulling = TryComp<PullerComponent>(ent, out var puller) ? puller.Pulling : null;
        if (pulling is { } victim && TryComp<SlaughterDemonComponent>(ent, out var slaughter))
        {
            if (_slaughterDemon.TryStartConsume((ent.Owner, slaughter), victim))
            {
                args.Handled = true;
                return;
            }

            return;
        }

        args.Handled = StartPhase(ent);
    }

    public bool StartPhase(Entity<DemonPhaseComponent> ent)
    {
        if (!IsValidAnchor(ent))
        {
            _popup.PopupEntity(Loc.GetString(ent.Comp.FailPopup), ent, ent);
            return false;
        }

        if (ent.Comp.PhaseDuration > TimeSpan.Zero)
        {
            return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                ent,
                ent.Comp.PhaseDuration,
                new DemonPhaseDoAfterEvent(),
                ent)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
            });
        }

        return TryPhase(ent.Owner, ent.Comp);
    }

    private void OnPhaseDoAfter(Entity<DemonPhaseComponent> ent, ref DemonPhaseDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!IsValidAnchor(ent))
        {
            _popup.PopupEntity(Loc.GetString(ent.Comp.FailPopup), ent, ent);
            return;
        }

        args.Handled = TryPhase(ent.Owner, ent.Comp);
    }

    private void OnRiseDoAfter(Entity<DemonPhaseComponent> ent, ref DemonRiseDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!IsValidAnchor(ent))
        {
            _popup.PopupEntity(Loc.GetString(ent.Comp.FailPopup), ent, ent);
            return;
        }

        args.Handled = true;

        var coords = Transform(ent).Coordinates;

        if (_polymorph.Revert((ent.Owner, null)) is not { } original)
            return;

        Spawn(ent.Comp.PhaseInEffect, coords);
        _audio.PlayPvs(ent.Comp.PhaseInSound, coords);

        if (ent.Comp.HallucinationSounds != null && _random.Prob(ent.Comp.HallucinationChance))
            _audio.PlayPvs(ent.Comp.HallucinationSounds, coords);

        RaiseLocalEvent(original, new DemonPhasedInEvent());
    }

    private bool IsValidAnchor(Entity<DemonPhaseComponent> ent)
    {
        return ent.Comp.Anchor switch
        {
            DemonPhaseAnchor.Blood => HasBloodNearby(ent),
            DemonPhaseAnchor.Darkness => !_photosensitivity.IsInSpace(ent) && _photosensitivity.GetIllumination(ent) < ent.Comp.DarknessThreshold,
            _ => false,
        };
    }

    private bool HasBloodNearby(Entity<DemonPhaseComponent> ent)
    {
        var puddles = _lookup.GetEntitiesInRange<PuddleComponent>(Transform(ent).Coordinates, ent.Comp.SearchRange);
        foreach (var puddle in puddles)
        {
            if (!_solutionContainer.TryGetSolution(puddle.Owner, puddle.Comp.SolutionName, out _, out var solution))
                continue;

            if (solution.GetTotalPrototypeQuantity(BloodReagent) > 0)
                return true;
        }

        return false;
    }
}

public sealed class DemonPhasedInEvent : EntityEventArgs;
