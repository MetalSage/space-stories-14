using Content.Server.Administration.Logs.Converters;
using Content.Server.Light.Components;
using Content.Server.Lightning.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage.Components;
using Content.Shared.DoAfter;
using Content.Shared.Light.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Polymorph;
using Content.Shared.Rejuvenate;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared._Stories.Conversion;
using Content.Shared._Stories.Shadowling;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Stories.Shadowling;
public sealed partial class ShadowlingSystem
{
    [ValidatePrototypeId<PolymorphPrototype>]
    public const string ShadowlingPolymorph = "Shadowling";

    [ValidatePrototypeId<PolymorphPrototype>]
    public const string ShadowlingAscendedPolymorph = "Ascended";

    [ValidatePrototypeId<EntityPrototype>]
    public const string SmokePrototype = "Smoke";

    [ValidatePrototypeId<ReagentPrototype>]
    public const string ShadowlingSmokeReagent = "ShadowlingSmokeReagent";

    [ValidatePrototypeId<ConversionPrototype>]
    public const string ShadowlingThrallConversion = "ShadowlingThrall";
    public void InitializeActions()
    {
        SubscribeLocalEvent<ShadowlingComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAnnihilateEvent>(OnAnnihilateEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingSonicScreechEvent>(OnSonicScreechEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingLightningStormEvent>(OnLightningStormEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingVeilEvent>(OnVeilEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingRapidReHatchEvent>(OnRapidReHatchEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingBlackRecuperationEvent>(OnBlackRecuperationEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingCollectiveMindEvent>(OnCollectiveEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingGlareEvent>(OnGlareEvent);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingBlindnessSmokeEvent>(OnBlindnessSmokeEvent);

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHatchEvent>(OnHatch);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingHatchDoAfterEvent>(OnHatchDoAfter);

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAscendanceEvent>(OnAscendance);
        SubscribeLocalEvent<ShadowlingComponent, ShadowlingAscendanceDoAfterEvent>(OnAscendanceDoAfter);

        SubscribeLocalEvent<ShadowlingComponent, ShadowlingEnthrallEvent>(OnEnthrallEvent);
    }
    private void OnInit(EntityUid uid, ShadowlingComponent component, ComponentInit args)
    {
        RefreshActions(uid, component);
    }
    public float RefreshActions(EntityUid uid, ShadowlingComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return 0;

        var thralls = _conversion.GetEntitiesConvertedBy(uid, ShadowlingThrallConversion);
        var aliveThrallsAmount = 0;

        foreach (var thrall in thralls)
            if (_mobState.IsAlive(thrall))
                aliveThrallsAmount++;

        foreach (var (action, thrallsAmount) in component.Actions)
        {
            if (aliveThrallsAmount >= thrallsAmount && !component.GrantedActions.ContainsKey(action))
            {
                var actionId = _actions.AddAction(uid, action);
                if (actionId != null)
                    component.GrantedActions.Add(action, actionId.Value);
            }
            else if (aliveThrallsAmount < thrallsAmount && component.GrantedActions.TryGetValue(action, out var actionId))
            {
                _actions.RemoveAction(uid, actionId);
                component.GrantedActions.Remove(action);
            }
        }

        return aliveThrallsAmount;
    }
    public void ShadowSmoke(EntityCoordinates coords, float amount = 100f, float duration = 15f, int spreadAmount = 7)
    {
        var solution = new Solution(ShadowlingSmokeReagent, amount);

        var smokeEnt = Spawn(SmokePrototype, coords);
        _smoke.StartSmoke(smokeEnt, solution, duration, spreadAmount);
    }
    // Actions
    private void OnAnnihilateEvent(EntityUid uid, ShadowlingComponent component, ShadowlingAnnihilateEvent args)
    {
        if (args.Handled)
            return;

        _body.GibBody(args.Target, false, splatModifier: 10); // FIXME: Hardcode

        args.Handled = true;
    }
    private void OnSonicScreechEvent(EntityUid uid, ShadowlingComponent component, ShadowlingSonicScreechEvent args)
    {
        if (args.Handled)
            return;

        var targets = _entityLookup.GetEntitiesInRange<MobStateComponent>(Transform(uid).Coordinates, 15f); // FIXME: Hardcode

        foreach (var (target, mobState) in targets)
        {
            if (HasComp<ShadowlingComponent>(target) || HasComp<ShadowlingThrallComponent>(target))
                continue;

            if (HasComp<BorgChassisComponent>(target))
            {
                _emp.DoEmpEffects(target, 50_000, 15); // FIXME: Hardcode
                _popup.PopupEntity("Волна визга выводит вашу электронику из строя", target, target); // FIXME: Hardcode
            }
            else
            {
                _stamina.TakeStaminaDamage(target, 100); // FIXME: Hardcode
                _popup.PopupEntity("Волна визга оглушает вас!", target, target); // FIXME: Hardcode
            }
        }

        args.Handled = true;
    }
    private void OnLightningStormEvent(EntityUid uid, ShadowlingComponent component, ShadowlingLightningStormEvent args)
    {
        if (args.Handled)
            return;

        HashSet<EntityUid> targets = new();

        foreach (var (ent, comp) in _entityLookup.GetEntitiesInRange<LightningTargetComponent>(Transform(uid).Coordinates, 10f))
        {
            if (HasComp<ShadowlingComponent>(ent) || HasComp<ShadowlingThrallComponent>(ent))
                continue;

            if (comp.LightningExplode) // FIXME: Hardcode
                continue;

            targets.Add(ent);
        }

        if (targets.Count == 0)
            return;

        foreach (var target in targets)
        {
            // Молнии от случайной цели к случайной цели. Хаос.
            _lightning.ShootLightning(_random.Pick(targets), target);
        }

        _emp.EmpPulse(_xform.GetMapCoordinates(uid), 12, 10000, 30);  // FIXME: Hardcode

        args.Handled = true;
    }
    private void OnVeilEvent(EntityUid uid, ShadowlingComponent component, ShadowlingVeilEvent args)
    {
        if (args.Handled)
            return;

        foreach (var (ent, _) in _entityLookup.GetEntitiesInRange<PointLightComponent>(_xform.GetMapCoordinates(uid), 10f)) // FIXME: Hardcode
        {
            if (HasComp<PoweredLightComponent>(ent))
                _poweredLight.TryDestroyBulb(ent);
            else if (TryComp<HandheldLightComponent>(ent, out var HandheldLight))
                _handheldLight.TurnOff((ent, HandheldLight));
            else if (TryComp<UnpoweredFlashlightComponent>(ent, out var UnpoweredFlashlight))
                _unpoweredFlashlight.SetLight(ent, false, ent, true);
        }

        args.Handled = true;
    }
    private void OnRapidReHatchEvent(EntityUid uid, ShadowlingComponent component, ShadowlingRapidReHatchEvent args)
    {
        if (args.Handled)
            return;

        // FIXME: Убирает КД на actions. Это нам не подходит.
        RaiseLocalEvent(uid, new RejuvenateEvent());

        args.Handled = true;
    }
    private void OnBlackRecuperationEvent(EntityUid uid, ShadowlingComponent component, ShadowlingBlackRecuperationEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<ShadowlingThrallComponent>(args.Target))
        {
            _popup.PopupCursor("Он не является траллом!", uid); // FIXME: Hardcode
            return;
        }
        else if (HasComp<ShadowlingComponent>(args.Target))
        {
            _popup.PopupCursor("Вы не можете вылечить его!", uid); // FIXME: Hardcode
            return;
        }
        else if (_mobState.IsAlive(args.Target))
        {
            _popup.PopupCursor("Выбранный раб уже живой.", uid); // FIXME: Hardcode
            return;
        }

        _popup.PopupEntity("Ваши раны покрываются тенью и затягиваются...", args.Target, args.Target); // FIXME: Hardcode
        RaiseLocalEvent(args.Target, new RejuvenateEvent());

        args.Handled = true;
    }
    private void OnCollectiveEvent(EntityUid uid, ShadowlingComponent component, ShadowlingCollectiveMindEvent args)
    {
        if (args.Handled)
            return;

        _popup.PopupEntity($"У вас {RefreshActions(uid, component)} живых порабощённых", uid, uid); // FIXME: Hardcode

        args.Handled = true;
    }
    private void OnGlareEvent(EntityUid uid, ShadowlingComponent component, ShadowlingGlareEvent args)
    {
        if (args.Handled)
            return;

        _flash.Flash(args.Target, uid, null, 15000, 0.8f, false); // FIXME: Hardcode
        _stun.TryStun(args.Target, TimeSpan.FromSeconds(5), false); // FIXME: Hardcode

        args.Handled = true;
    }
    private void OnBlindnessSmokeEvent(EntityUid uid, ShadowlingComponent component, ShadowlingBlindnessSmokeEvent args)
    {
        if (args.Handled)
            return;
        ShadowSmoke(Transform(uid).Coordinates, 100, 30, 7); // FIXME: Hardcode
        args.Handled = true;
    }

    // Hatch
    private void OnHatch(EntityUid uid, ShadowlingComponent component, ShadowlingHatchEvent args)
    {
        if (args.Handled)
            return;

        var shadowlingUid = _polymorph.PolymorphEntity(uid, ShadowlingPolymorph);

        if (shadowlingUid == null)
            return;

        ShadowSmoke(Transform(shadowlingUid.Value).Coordinates, 100, 15, 7); // FIXME: Hardcode

        _stun.TryParalyze(shadowlingUid.Value, TimeSpan.FromSeconds(15), true); // FIXME: Hardcode

        var doAfterArgs = new DoAfterArgs(EntityManager, shadowlingUid.Value, TimeSpan.FromSeconds(15f), new ShadowlingHatchDoAfterEvent(), shadowlingUid) // FIXME: Hardcode
        {
            RequireCanInteract = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }
    private void OnHatchDoAfter(EntityUid uid, ShadowlingComponent component, ShadowlingHatchDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;
        // А зачем падал?
        _standing.Stand(uid);
        args.Handled = true;
    }
    // Ascendance
    private void OnAscendance(EntityUid uid, ShadowlingComponent component, ShadowlingAscendanceEvent args)
    {
        if (args.Handled)
            return;

        ShadowSmoke(Transform(uid).Coordinates, 100f, 5f, 7); // FIXME: Hardcode
        _stun.TryParalyze(uid, TimeSpan.FromSeconds(5f), true); // FIXME: Hardcode

        var doAfter = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(5f), new ShadowlingAscendanceDoAfterEvent(), uid) // FIXME: Hardcode
        {
            RequireCanInteract = false,
        };

        _doAfter.TryStartDoAfter(doAfter);

        args.Handled = true;
    }
    private void OnAscendanceDoAfter(EntityUid uid, ShadowlingComponent component, ShadowlingAscendanceDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;
        // А зачем падал?
        _standing.Stand(uid);
        var ascendance = _polymorph.PolymorphEntity(uid, ShadowlingAscendedPolymorph);

        if (ascendance == null)
            return;

        RaiseLocalEvent(new ShadowlingWorldAscendanceEvent()
        {
            EntityUid = ascendance.Value,
        });

        args.Handled = true;
    }
    // Enthrall
    private void OnEnthrallEvent(EntityUid uid, ShadowlingComponent component, ShadowlingEnthrallEvent args)
    {
        if (args.Handled)
            return;

        if (_conversion.TryConvert(args.Target, ShadowlingThrallConversion, args.Performer))
        {
            _popup.PopupEntity("Вы поглотили чей-то разум...", args.Performer, args.Performer); // FIXME: Hardcode
            _popup.PopupEntity("Ваш разум поглощен тенями...", args.Target, args.Target); // FIXME: Hardcode
        }
        else
        {
            _popup.PopupCursor("Не удалось обратить это!", args.Performer); // FIXME: Hardcode
            _stamina.TryTakeStamina(args.Performer, 50f); // FIXME: Hardcode
        }

        args.Handled = true;
    }
}
