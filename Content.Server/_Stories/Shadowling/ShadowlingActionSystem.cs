using System.Linq;
using Content.Server._Stories.Conversion;
using Content.Server.Chat.Systems;
using Content.Server.Emp;
using Content.Server.Flash;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Light.EntitySystems;
using Content.Server.Polymorph.Systems;
using Content.Server.Stunnable;
using Content.Server.Temperature.Systems;
using Content.Shared._Stories.Conversion;
using Content.Shared._Stories.Shadowling;
using Content.Shared._Stories.SCCVars;
using Content.Shared._Stories.Vision.Events;
using Content.Shared._Stories.Vision.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Eye;
using Content.Shared.Gibbing;
using Content.Shared.Humanoid;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Interaction.Events;
using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Stories.Shadowling;

public sealed class ShadowlingActionSystem : EntitySystem
{
    private static readonly EntProtoId MutedStatusEffect = "Muted";

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ConversionSystem _conversion = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly GibbingSystem _gib = default!;
    [Dependency] private readonly HandheldLightSystem _handheldLight = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UnpoweredFlashlightSystem _unpoweredFlashlight = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly VisibilitySystem _visibility = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedVisionSystem _vision = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAnnihilateEvent>(OnAnnihilate);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingSonicScreechEvent>(OnSonicScreech);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingVeilEvent>(OnVeil);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingBlackRecuperationEvent>(OnBlackRecuperation);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingCollectiveMindEvent>(OnCollectiveMind);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingGlareEvent>(OnGlare);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingBlindnessSmokeEvent>(OnBlindnessSmoke);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHatchEvent>(OnHatch);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHatchDoAfterEvent>(OnHatchDoAfter);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAscendanceEvent>(OnAscendance);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAscendanceDoAfterEvent>(OnAscendanceDoAfter);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingEnthrallEvent>(OnEnthrall);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingEnthrallDoAfterEvent>(OnEnthrallDoAfter);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingShadowWalkEvent>(OnShadowWalk);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingPlaneShiftEvent>(OnPlaneShift);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingFlashFreezeEvent>(OnFlashFreeze);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingDrainLifeEvent>(OnDrainLife);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingGlacialBlastEvent>(OnGlacialBlast);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHypnosisEvent>(OnHypnosis);

        SubscribeLocalEvent<AscendantBroadcastComponent, EntitySpokeEvent>(OnAscendantSpeak);
        SubscribeLocalEvent<ShadowlingComponent, ToggleAscendantBroadcastEvent>(OnToggleBroadcast);

        SubscribeLocalEvent<ShadowWalkingComponent, DamageModifyEvent>(OnShadowWalkDamage);
        SubscribeLocalEvent<ShadowWalkingComponent, InteractionAttemptEvent>(OnShadowWalkInteract);
        SubscribeLocalEvent<ShadowWalkingComponent, AttackAttemptEvent>(OnShadowWalkAttack);
        SubscribeLocalEvent<ShadowWalkingComponent, RefreshVisionEvent>(OnShadowWalkVision);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShadowWalkingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.EndTime != null && _timing.CurTime >= comp.EndTime)
            {
                DisableShadowWalk(uid, comp);
            }
        }
    }

    private void SpawnShadowSmoke(EntityUid uid, ShadowlingComponent component, EntityCoordinates coords, float amount = 100f, float duration = 15f, float radius = 5f)
    {
        var solution = new Content.Shared.Chemistry.Components.Solution(component.ShadowlingSmokeReagent, amount);
        var smokeEnt = Spawn(component.SmokePrototype, coords);
        _smoke.StartSmoke(smokeEnt, solution, duration, (int)radius);
    }

    private void OnShadowWalkDamage(EntityUid uid, ShadowWalkingComponent component, DamageModifyEvent args)
    {
        args.Damage *= 0;
    }

    private void OnShadowWalkInteract(EntityUid uid, ShadowWalkingComponent component, ref InteractionAttemptEvent args)
    {
        if (args.Target != null)
            args.Cancelled = true;
    }

    private void OnShadowWalkAttack(EntityUid uid, ShadowWalkingComponent component, AttackAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnShadowWalkVision(EntityUid uid, ShadowWalkingComponent comp, ref RefreshVisionEvent args)
    {
        args.DrawFov = false;
        args.DrawLighting = false;
        args.DrawShadows = false;
        args.IsActive = true;
    }

    private void EnableShadowWalk(EntityUid uid, TimeSpan? duration = null)
    {
        if (HasComp<ShadowWalkingComponent>(uid))
            return;

        var comp = AddComp<ShadowWalkingComponent>(uid);
        if (duration != null)
            comp.EndTime = _timing.CurTime + duration.Value;

        EnsureComp<MovementIgnoreGravityComponent>(uid);

        if (TryComp<FixturesComponent>(uid, out var fixtures))
        {
            var firstFixture = fixtures.Fixtures.Values.FirstOrDefault();
            if (firstFixture != null)
            {
                comp.OriginalCollisionLayer = firstFixture.CollisionLayer;
                comp.OriginalCollisionMask = firstFixture.CollisionMask;

                foreach (var (id, fixture) in fixtures.Fixtures)
                {
                    _physics.SetCollisionLayer(uid, id, fixture, 0, fixtures);
                    _physics.SetCollisionMask(uid, id, fixture, 0, fixtures);
                }
            }
        }

        if (TryComp<MovementSpeedModifierComponent>(uid, out var move))
        {
            comp.OriginalWalkSpeed = move.BaseWalkSpeed;
            comp.OriginalSprintSpeed = move.BaseSprintSpeed;
            _movement.ChangeBaseSpeed(uid, 15f, 15f, move.Acceleration);
        }

        var vis = EnsureComp<VisibilityComponent>(uid);
        comp.OriginalVisibility = vis.Layer;
        _visibility.AddLayer((uid, vis), (ushort)2, false);
        _visibility.RemoveLayer((uid, vis), (ushort)1, false);
        _visibility.RefreshVisibility(uid, vis);

        if (TryComp<EyeComponent>(uid, out var eye))
        {
            comp.OriginalEyeVisibilityMask = eye.VisibilityMask;
            comp.OriginalDrawFov = eye.DrawFov;
            _eye.SetVisibilityMask(uid, eye.VisibilityMask | 2, eye);
            _eye.SetDrawFov(uid, false, eye);
        }

        _vision.UpdateVision(uid);

        _popup.PopupEntity(Loc.GetString("stories-shadowling-shadow-walk-enter"), uid, uid);
    }

    private void DisableShadowWalk(EntityUid uid, ShadowWalkingComponent comp)
    {
        RemComp<MovementIgnoreGravityComponent>(uid);

        if (TryComp<FixturesComponent>(uid, out var fixtures))
        {
            foreach (var (id, fixture) in fixtures.Fixtures)
            {
                _physics.SetCollisionLayer(uid, id, fixture, comp.OriginalCollisionLayer, fixtures);
                _physics.SetCollisionMask(uid, id, fixture, comp.OriginalCollisionMask, fixtures);
            }
        }

        if (TryComp<MovementSpeedModifierComponent>(uid, out var move))
        {
            _movement.ChangeBaseSpeed(uid, comp.OriginalWalkSpeed, comp.OriginalSprintSpeed, move.Acceleration);
        }

        if (TryComp<VisibilityComponent>(uid, out var vis))
        {
            _visibility.RemoveLayer((uid, vis), (ushort)2, false);
            _visibility.AddLayer((uid, vis), (ushort)1, false);
            _visibility.RefreshVisibility(uid, vis);
        }

        if (TryComp<EyeComponent>(uid, out var eye))
        {
            _eye.SetVisibilityMask(uid, comp.OriginalEyeVisibilityMask, eye);
            _eye.SetDrawFov(uid, comp.OriginalDrawFov, eye);
        }

        RemComp<ShadowWalkingComponent>(uid);

        _vision.UpdateVision(uid);

        _popup.PopupEntity(Loc.GetString("stories-shadowling-shadow-walk-exit"), uid, uid);
    }

    private void CheckHalfwayAscendance(EntityUid shadowlingUid, ShadowlingComponent component)
    {
        var count = _shadowling.RefreshActions(shadowlingUid, component);
        if (count >= component.AscendanceThrallRequirement / 2)
        {
            RaiseLocalEvent(new ShadowlingHalfwayEvent());
        }
    }

    private void OnShadowWalk(EntityUid uid, ShadowlingComponent component, ShadowlingShadowWalkEvent args)
    {
        if (args.Handled) return;
        EnableShadowWalk(uid, component.ShadowWalkDuration);
        args.Handled = true;
    }

    private void OnPlaneShift(EntityUid uid, ShadowlingComponent component, ShadowlingPlaneShiftEvent args)
    {
        if (args.Handled) return;

        if (TryComp<ShadowWalkingComponent>(uid, out var walk))
            DisableShadowWalk(uid, walk);
        else
            EnableShadowWalk(uid);

        args.Handled = true;
    }

    private void OnToggleBroadcast(EntityUid uid, ShadowlingComponent component, ToggleAscendantBroadcastEvent args)
    {
        if (args.Handled) return;

        if (HasComp<AscendantBroadcastComponent>(uid))
        {
            RemComp<AscendantBroadcastComponent>(uid);
            _popup.PopupEntity(Loc.GetString("stories-shadowling-broadcast-off"), uid, uid);
        }
        else
        {
            AddComp<AscendantBroadcastComponent>(uid);
            _popup.PopupEntity(Loc.GetString("stories-shadowling-broadcast-on"), uid, uid);
        }
        args.Handled = true;
    }

    private void OnAscendantSpeak(EntityUid uid, AscendantBroadcastComponent component, EntitySpokeEvent args)
    {
        if (args.Channel != null)
            return;

        if (_timing.CurTime < component.NextBroadcast)
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-broadcast-cooldown"), uid, uid);
            return;
        }

        component.NextBroadcast = _timing.CurTime + TimeSpan.FromSeconds(10);
        _chat.DispatchGlobalAnnouncement(args.Message, MetaData(uid).EntityName, false, null, Color.Red);

        var broadcastAction = new EntProtoId("STActionShadowlingAscendantBroadcast");
        if (TryComp<ShadowlingComponent>(uid, out var shadowling) && shadowling.GrantedActions.TryGetValue(broadcastAction, out var actionId))
        {
            _actions.SetCooldown(actionId, _timing.CurTime, _timing.CurTime + TimeSpan.FromSeconds(10));
        }
    }

    private void OnAnnihilate(EntityUid uid, ShadowlingComponent component, ShadowlingAnnihilateEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        var isAscendant = MetaData(uid).EntityPrototype?.ID == "STMobAscendance";

        if (args.Target == uid || (!isAscendant && (HasComp<ShadowlingComponent>(args.Target) || HasComp<ShadowlingThrallComponent>(args.Target))))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-ally"), uid, uid);
            return;
        }

        if (!HasComp<BodyComponent>(args.Target))
            return;

        _stun.TryAddStunDuration(args.Target, TimeSpan.FromSeconds(2));
        _popup.PopupEntity(Loc.GetString("stories-shadowling-annihilate-target"), args.Target, args.Target, PopupType.LargeCaution);

        _audio.PlayPvs(component.AnnihilateSound, args.Target);

        Timer.Spawn(2000, () =>
        {
            if (!Deleted(args.Target))
                _gib.Gib(args.Target);
        });

        args.Handled = true;
    }

    private void OnSonicScreech(EntityUid uid, ShadowlingComponent component, ShadowlingSonicScreechEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        _audio.PlayPvs(component.ScreechSound, uid);

        var targets = _entityLookup.GetEntitiesInRange<TransformComponent>(Transform(uid).Coordinates, component.SonicScreechRange);
        foreach (var (target, _) in targets)
        {
            if (target == uid || HasComp<ShadowlingComponent>(target) || HasComp<ShadowlingThrallComponent>(target))
                continue;

            if (HasComp<BorgChassisComponent>(target))
            {
                _emp.DoEmpEffects(target, 50000, TimeSpan.FromSeconds(6));
            }
            else if (_tag.HasTag(target, "Window"))
            {
                _damageable.TryChangeDamage(target, component.SonicScreechWindowDamage, true);
            }
            else if (HasComp<MobStateComponent>(target))
            {
                _stun.TryKnockdown(target, TimeSpan.FromSeconds(2), true);
            }
        }
        args.Handled = true;
    }

    private void OnVeil(EntityUid uid, ShadowlingComponent component, ShadowlingVeilEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        foreach (var (target, _) in _entityLookup.GetEntitiesInRange<PointLightComponent>(_transform.GetMapCoordinates(uid), component.VeilRange))
        {
            if (TryComp<PoweredLightComponent>(target, out var poweredLight))
                _poweredLight.TryDestroyBulb(target, poweredLight);
            else if (TryComp<HandheldLightComponent>(target, out var handheldLight))
                _handheldLight.TurnOff(new Entity<HandheldLightComponent>(target, handheldLight));
            else if (TryComp<UnpoweredFlashlightComponent>(target, out var unpoweredFlashlight))
                _unpoweredFlashlight.SetLight(new Entity<UnpoweredFlashlightComponent?>(target, unpoweredFlashlight), false, uid, true);
        }
        args.Handled = true;
    }

    private void OnFlashFreeze(EntityUid uid, ShadowlingComponent component, ShadowlingFlashFreezeEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        _audio.PlayPvs(component.FreezeSound, uid);

        foreach (var (target, _) in _entityLookup.GetEntitiesInRange<MobStateComponent>(Transform(uid).Coordinates, component.FlashFreezeRange))
        {
            if (target == uid || HasComp<ShadowlingComponent>(target) || HasComp<ShadowlingThrallComponent>(target))
                continue;

            _damageable.TryChangeDamage(target, component.FlashFreezeDamage, true);
            _stun.TryAddStunDuration(target, component.FlashFreezeStunDuration);

            if (TryComp<TemperatureComponent>(target, out var temp))
                _temperature.ForceChangeTemperature(target, 200f, temp);
        }
        args.Handled = true;
    }

    private void OnGlacialBlast(EntityUid uid, ShadowlingComponent component, ShadowlingGlacialBlastEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        _audio.PlayPvs(component.FreezeSound, uid);

        foreach (var (target, _) in _entityLookup.GetEntitiesInRange<MobStateComponent>(Transform(uid).Coordinates, component.GlacialBlastRange))
        {
            if (target == uid || HasComp<ShadowlingComponent>(target) || HasComp<ShadowlingThrallComponent>(target))
                continue;

            _damageable.TryChangeDamage(target, component.GlacialBlastDamage, true);
            _stun.TryAddStunDuration(target, component.GlacialBlastStunDuration);
            _stun.TryKnockdown(target, component.GlacialBlastStunDuration, true);

            if (TryComp<TemperatureComponent>(target, out var temp))
                _temperature.ForceChangeTemperature(target, 73.15f, temp);
        }
        args.Handled = true;
    }

    private void OnDrainLife(EntityUid uid, ShadowlingComponent component, ShadowlingDrainLifeEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        var healed = false;
        foreach (var (target, _) in _entityLookup.GetEntitiesInRange<MobStateComponent>(Transform(uid).Coordinates, component.DrainLifeRange))
        {
            if (target == uid || !_mobState.IsAlive(target) || HasComp<ShadowlingComponent>(target) || HasComp<ShadowlingThrallComponent>(target))
                continue;

            _damageable.TryChangeDamage(target, component.DrainLifeDamage, true);
            healed = true;
        }

        if (healed)
        {
            _damageable.TryChangeDamage(uid, component.DrainLifeHeal, true);
            RemComp<KnockedDownComponent>(uid);
            RemComp<StunnedComponent>(uid);
            args.Handled = true;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-drain-life-fail"), uid, uid);
        }
    }

    private void OnBlackRecuperation(EntityUid uid, ShadowlingComponent component, ShadowlingBlackRecuperationEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        if (args.Target == uid || !HasComp<ShadowlingThrallComponent>(args.Target) || HasComp<ShadowlingComponent>(args.Target))
            return;

        if (_mobState.IsAlive(args.Target))
            return;

        _mobState.ChangeMobState(args.Target, MobState.Alive);
        _damageable.SetAllDamage(args.Target, 0);
        args.Handled = true;
    }

    private void OnCollectiveMind(EntityUid uid, ShadowlingComponent component, ShadowlingCollectiveMindEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        var count = _shadowling.RefreshActions(uid, component);
        _popup.PopupEntity(Loc.GetString("stories-shadowling-collective-mind-count", ("count", count)), uid, uid);

        CheckHalfwayAscendance(uid, component);

        args.Handled = true;
    }

    private void OnGlare(EntityUid uid, ShadowlingComponent component, ShadowlingGlareEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        if (args.Target == uid || HasComp<ShadowlingComponent>(args.Target) || HasComp<ShadowlingThrallComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-ally"), uid, uid);
            return;
        }

        _audio.PlayPvs(component.GlareSound, uid);

        _flash.Flash(args.Target, uid, null, component.GlareFlashDuration, 0.8f, false);
        _stun.TryAddStunDuration(args.Target, component.GlareStunDuration);
        _status.TryAddStatusEffectDuration(args.Target, MutedStatusEffect, component.GlareStunDuration);

        args.Handled = true;
    }

    private void OnBlindnessSmoke(EntityUid uid, ShadowlingComponent component, ShadowlingBlindnessSmokeEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        _audio.PlayPvs(component.SmokeSound, uid);
        SpawnShadowSmoke(uid, component, Transform(uid).Coordinates, 100f, 15f, component.SmokeRadius);
        args.Handled = true;
    }

    private void OnHatch(EntityUid uid, ShadowlingComponent component, ShadowlingHatchEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        _audio.PlayPvs(component.HatchSound, uid);
        SpawnShadowSmoke(uid, component, Transform(uid).Coordinates, 100f, 15f, component.SmokeRadius);
        _stun.TryAddParalyzeDuration(uid, component.HatchDuration);

        var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.HatchDuration, new ShadowlingHatchDoAfterEvent(), uid)
        {
            RequireCanInteract = false
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs))
            args.Handled = true;
    }

    private void OnHatchDoAfter(EntityUid uid, ShadowlingComponent component, ShadowlingHatchDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled) return;

        _standing.Stand(uid);

        var thralls = _conversion.GetEntitiesConvertedBy(uid, component.ShadowlingThrallConversion).ToList();
        var shadowlingUid = _polymorph.PolymorphEntity(uid, component.HatchPolymorph);
        if (shadowlingUid == null)
            return;

        var newComp = Comp<ShadowlingComponent>(shadowlingUid.Value);

        foreach (var thrall in thralls)
        {
            if (_conversion.TryGetConversion(thrall, component.ShadowlingThrallConversion, out var conversion))
            {
                conversion.Owner = GetNetEntity(shadowlingUid.Value);
                Dirty(thrall, Comp<ConversionableComponent>(thrall));
            }
        }

        _shadowling.RefreshActions(shadowlingUid.Value, newComp);
        args.Handled = true;
    }

    private void OnAscendance(EntityUid uid, ShadowlingComponent component, ShadowlingAscendanceEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        _audio.PlayGlobal(component.AscendanceSound, Filter.Broadcast(), true);
        SpawnShadowSmoke(uid, component, Transform(uid).Coordinates, 100f, 5f, component.SmokeRadius);
        _stun.TryAddParalyzeDuration(uid, component.AscendanceDuration);

        var doAfter = new DoAfterArgs(EntityManager, uid, component.AscendanceDuration, new ShadowlingAscendanceDoAfterEvent(), uid)
        {
            RequireCanInteract = false
        };

        if (_doAfter.TryStartDoAfter(doAfter))
            args.Handled = true;
    }

    private void OnAscendanceDoAfter(EntityUid uid, ShadowlingComponent component, ShadowlingAscendanceDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled) return;

        _standing.Stand(uid);

        var thralls = _conversion.GetEntitiesConvertedBy(uid, component.ShadowlingThrallConversion).ToList();

        var ascendance = _polymorph.PolymorphEntity(uid, component.AscendancePolymorph);
        if (ascendance == null) return;

        foreach (var thrall in thralls)
        {
            if (_conversion.TryGetConversion(thrall, component.ShadowlingThrallConversion, out var conversion))
            {
                conversion.Owner = GetNetEntity(ascendance.Value);
                Dirty(thrall, Comp<ConversionableComponent>(thrall));
            }
            _damageable.TryChangeDamage(thrall, component.AscendanceKillDamage, true);
        }

        var newComp = Comp<ShadowlingComponent>(ascendance.Value);
        _shadowling.RefreshActions(ascendance.Value, newComp);

        RaiseLocalEvent(new ShadowlingWorldAscendanceEvent { Entity = GetNetEntity(ascendance.Value) });
        args.Handled = true;
    }

    private void OnEnthrall(EntityUid uid, ShadowlingComponent component, ShadowlingEnthrallEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        if (args.Target == uid || HasComp<ShadowlingComponent>(args.Target) || HasComp<ShadowlingThrallComponent>(args.Target) || HasComp<BorgChassisComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-ally"), uid, uid);
            return;
        }

        if (component.RequireHumanoid && !HasComp<HumanoidProfileComponent>(args.Target))
        {
            return;
        }

        if (component.RequireConnectedMind && !_cfg.GetCVar(SCCVars.EnthrallWithoutMind))
        {
            if (!_mind.TryGetMind(args.Target, out var targetMindId, out var targetMind) || targetMind.UserId == null)
            {
                _popup.PopupEntity(Loc.GetString("stories-shadowling-enthrall-fail-no-mind"), uid, uid);
                return;
            }

            if (!_player.TryGetSessionById(targetMind.UserId.Value, out var session))
            {
                _popup.PopupEntity(Loc.GetString("stories-shadowling-enthrall-fail-no-mind"), uid, uid);
                return;
            }
        }

        var isHatched = HasComp<MobStateComponent>(uid) && (MetaData(uid).EntityPrototype?.ID == "STMobShadowling" || MetaData(uid).EntityPrototype?.ID == "STMobAscendance");
        if (!isHatched && component.MaxThrallsBeforeHatch != null)
        {
            var thrallsCount = _conversion.GetEntitiesConvertedBy(uid, component.ShadowlingThrallConversion).Count;
            if (thrallsCount >= component.MaxThrallsBeforeHatch.Value)
            {
                _popup.PopupEntity(Loc.GetString("stories-shadowling-max-thralls"), uid, uid);
                return;
            }
        }

        if (!_conversion.CanConvert(args.Target, component.ShadowlingThrallConversion, uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-enthrall-fail", ("target", args.Target)), uid, uid);
            return;
        }

        _popup.PopupEntity(Loc.GetString("stories-shadowling-enthrall-start", ("target", args.Target)), uid, uid);

        var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.EnthrallDuration, new ShadowlingEnthrallDoAfterEvent(), uid, args.Target)
        {
            RequireCanInteract = true,
            BreakOnMove = true,
            BreakOnDamage = true
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs))
            args.Handled = true;
    }

    private void OnEnthrallDoAfter(EntityUid uid, ShadowlingComponent component, ShadowlingEnthrallDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null) return;

        if (HasComp<ShadowlingComponent>(args.Args.Target.Value) || HasComp<ShadowlingThrallComponent>(args.Args.Target.Value))
            return;

        var isHatched = HasComp<MobStateComponent>(uid) && (MetaData(uid).EntityPrototype?.ID == "STMobShadowling" || MetaData(uid).EntityPrototype?.ID == "STMobAscendance");
        if (!isHatched && component.MaxThrallsBeforeHatch != null)
        {
            var thrallsCount = _conversion.GetEntitiesConvertedBy(uid, component.ShadowlingThrallConversion).Count;
            if (thrallsCount >= component.MaxThrallsBeforeHatch.Value)
            {
                _popup.PopupEntity(Loc.GetString("stories-shadowling-max-thralls"), uid, uid);
                return;
            }
        }

        if (_conversion.TryConvert(args.Args.Target.Value, component.ShadowlingThrallConversion, uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-enthrall-success", ("target", args.Args.Target.Value)), uid, uid);
            CheckHalfwayAscendance(uid, component);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-enthrall-fail", ("target", args.Args.Target.Value)), uid, uid);
        }

        args.Handled = true;
    }

    private void OnHypnosis(EntityUid uid, ShadowlingComponent component, ShadowlingHypnosisEvent args)
    {
        if (args.Handled) return;

        if (HasComp<ShadowWalkingComponent>(uid))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-phased"), uid, uid);
            return;
        }

        if (args.Target == uid || HasComp<ShadowlingComponent>(args.Target) || HasComp<ShadowlingThrallComponent>(args.Target) || HasComp<BorgChassisComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("stories-shadowling-action-fail-ally"), uid, uid);
            return;
        }

        if (component.RequireHumanoid && !HasComp<HumanoidProfileComponent>(args.Target))
        {
            return;
        }

        if (component.RequireConnectedMind && !_cfg.GetCVar(SCCVars.EnthrallWithoutMind))
        {
            if (!_mind.TryGetMind(args.Target, out var targetMindId, out var targetMind) || targetMind.UserId == null)
            {
                _popup.PopupEntity(Loc.GetString("stories-shadowling-enthrall-fail-no-mind"), uid, uid);
                return;
            }

            if (!_player.TryGetSessionById(targetMind.UserId.Value, out var session))
            {
                _popup.PopupEntity(Loc.GetString("stories-shadowling-enthrall-fail-no-mind"), uid, uid);
                return;
            }
        }

        var isHatched = HasComp<MobStateComponent>(uid) && (MetaData(uid).EntityPrototype?.ID == "STMobShadowling" || MetaData(uid).EntityPrototype?.ID == "STMobAscendance");
        if (!isHatched && component.MaxThrallsBeforeHatch != null)
        {
            var thrallsCount = _conversion.GetEntitiesConvertedBy(uid, component.ShadowlingThrallConversion).Count;
            if (thrallsCount >= component.MaxThrallsBeforeHatch.Value)
            {
                _popup.PopupEntity(Loc.GetString("stories-shadowling-max-thralls"), uid, uid);
                return;
            }
        }

        if (!_conversion.CanConvert(args.Target, component.ShadowlingThrallConversion, args.Performer))
        {
            return;
        }

        if (_conversion.TryConvert(args.Target, component.ShadowlingThrallConversion, args.Performer))
        {
            CheckHalfwayAscendance(uid, component);
            args.Handled = true;
        }
    }
}
