using Content.Shared.Cuffs.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Server.Store.Components;
using Content.Shared._Stories.ForceUser;
using Robust.Shared.Physics.Components;
using Content.Shared.Physics;
using Content.Shared.Mobs;
using Content.Shared._Stories.Empire.Components;
using Content.Server._Stories.ForceUser.ProtectiveBubble.Components;
using Content.Shared.Store.Components;

namespace Content.Server._Stories.ForceUser;
public sealed partial class ForceUserSystem
{
    public void InitializeSimpleActions()
    {
        SubscribeLocalEvent<FlashAreaEvent>(OnFlashAreaEvent);
        SubscribeLocalEvent<EmpActionEvent>(OnEmp);
        SubscribeLocalEvent<RejuvenateActionEvent>(OnRejuvenate);
        SubscribeLocalEvent<FreedomActionEvent>(OnFreedom);
        // SubscribeLocalEvent<LightningStrikeEvent>(OnStrike);
        SubscribeLocalEvent<IgniteTargetActionEvent>(OnIgnite);
        SubscribeLocalEvent<RecliningPulseEvent>(OnPulseEvent);
        SubscribeLocalEvent<ForceDashActionEvent>(OnDash);

        SubscribeLocalEvent<HypnosisTargetActionEvent>(OnHypnosis); // FIXME: Тут не должно быть этого - start
        SubscribeLocalEvent<ForceUserComponent, FrozeBulletsActionEvent>(OnFrozeBullets);
        SubscribeLocalEvent<ForceUserComponent, ForceShopActionEvent>(OnShop);
        SubscribeLocalEvent<ForceUserComponent, ForceLookUpActionEvent>(OnLookUp); // FIXME: Тут не должно быть этого - end
    }
    private void OnLookUp(EntityUid uid, ForceUserComponent component, ForceLookUpActionEvent args)
    {
        if (args.Handled) return;
        var ents = _lookup.GetEntitiesInRange<ForceUserComponent>(_xform.GetMapCoordinates(uid), args.Range);
        foreach (var ent in ents)
        {
            if (ents.Count == 1 && ent.Owner == uid)
            {
                _popup.PopupEntity(Loc.GetString("force-lookup-lonely"), uid, uid);
            }
            else if (ent.Owner == uid) continue;
            else if (ents.Count == 2) _popup.PopupEntity(Loc.GetString("force-lookup-one", ("name", ent.Comp.Name())), uid, uid);
            else if (ents.Count > 2) _popup.PopupEntity(Loc.GetString("force-lookup-many"), uid, uid);
            break;
        }
        args.Handled = true;
    }
    private void OnFrozeBullets(EntityUid uid, ForceUserComponent component, FrozeBulletsActionEvent args)
    {
        if (args.Handled) return;
        _statusEffect.TryAddStatusEffect(uid, "FrozeBullets", TimeSpan.FromSeconds(args.Seconds), true, "FrozeBullets");
        args.Handled = true;
    }
    private void OnShop(EntityUid uid, ForceUserComponent component, ForceShopActionEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;
        _store.ToggleUi(uid, uid, store);
    }
    private void OnRejuvenate(RejuvenateActionEvent args)
    {
        if (args.Handled) return;
        _rejuvenate.PerformRejuvenate(args.Performer);
        args.Handled = true;
    }
    private void OnPulseEvent(RecliningPulseEvent args)
    {
        if (args.Handled) return;

        var xform = Transform(args.Performer);
        var range = args.Range;
        var strength = args.Strength;
        var lookup = _lookup.GetEntitiesInRange(args.Performer, range, LookupFlags.Dynamic | LookupFlags.Sundries);
        var xformQuery = GetEntityQuery<TransformComponent>();
        var worldPos = _xform.GetWorldPosition(xform, xformQuery);
        var physQuery = GetEntityQuery<PhysicsComponent>();

        foreach (var ent in lookup)
        {
            if (physQuery.TryGetComponent(ent, out var phys)
                && (phys.CollisionMask & (int) CollisionGroup.GhostImpassable) != 0)
                continue;

            var foo = _xform.GetWorldPosition(ent, xformQuery) - worldPos;
            _throwing.TryThrow(ent, foo * 10, strength, args.Performer, 0);

            if (_force.TryRemoveVolume(ent, _random.Next(10, 30)))
                _popup.PopupEntity("Устоял!", ent);
            else
                _stun.TryParalyze(ent, TimeSpan.FromSeconds(args.StunTime), true);
        }

        args.Handled = true;
    }
    private void OnFlashAreaEvent(FlashAreaEvent args)
    {
        if (args.Handled) return;
        _flashSystem.FlashArea(args.Performer, args.Performer, args.Range, args.FlashDuration, slowTo: args.SlowTo, sound: args.Sound);
        args.Handled = true;
    }
    private void OnDash(ForceDashActionEvent args)
    {
        if (args.Handled) return;
        _throwing.TryThrow(args.Performer, args.Target, args.Strength);
        args.Handled = true;
    }
    private void OnHypnosis(HypnosisTargetActionEvent args)
    {
        if (args.Handled || _mobState.IsIncapacitated(args.Target) || HasComp<MindShieldComponent>(args.Target)) return;
        _conversion.TryConvert(args.Target, "HypnotizedEmpire", args.Performer); // FIXME: Hardcode. Исправим в обновлении инквизитора.
        args.Handled = true;
    }
    private void OnIgnite(IgniteTargetActionEvent args)
    {
        if (args.Handled || _mobState.IsIncapacitated(args.Target) || HasComp<ProtectedByProtectiveBubbleComponent>(args.Target)) return; // FIXME: Hardcode

        _flammable.AdjustFireStacks(args.Target, args.StackAmount);
        _flammable.Ignite(args.Target, args.Performer);

        args.Handled = true;
    }
    private void OnStrike(LightningStrikeEvent args)
    {
        if (args.Handled) return;
        _lightning.ShootLightning(args.Performer, args.Target);
        args.Handled = true;
    }
    private void OnFreedom(FreedomActionEvent args)
    {
        if (!TryComp<CuffableComponent>(args.Performer, out var cuffs) || cuffs.Container.ContainedEntities.Count < 1) return;
        _cuffable.Uncuff(args.Performer, cuffs.LastAddedCuffs, cuffs.LastAddedCuffs);
        args.Handled = true;
    }
    private void OnEmp(EmpActionEvent args)
    {
        if (args.Handled) return;
        _emp.EmpPulse(_xform.GetMapCoordinates(args.Performer), args.Range, args.EnergyConsumption, args.DisableDuration);
        args.Handled = true;
    }
}
