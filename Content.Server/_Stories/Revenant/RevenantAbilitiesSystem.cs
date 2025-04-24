using System.Numerics;
using System.Security.Cryptography;
using Content.Server.Actions;
using Content.Server.GameTicking;
using Content.Server.Revenant.Components;
using Content.Server.Revenant.EntitySystems;
using Content.Server.Store.Systems;
using Content.Shared._Stories.Revenant;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Jittering;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server._Stories.Revenant;

public sealed class RevenantAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly RevenantSystem _revenant = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevenantComponent, RevenantReapActionEvent>(OnReap);
        SubscribeLocalEvent<RevenantComponent, RevenantGhostlyTouchActionEvent>(OnGhostlyTouch);
    }

    #region Abilities
    private void OnGhostlyTouch(EntityUid uid, RevenantComponent component, RevenantGhostlyTouchActionEvent args)
    {
        DamageSpecifier dspec = new();
        dspec.DamageDict.Add("Cold", 5f);
        _damage.TryChangeDamage(args.Target, dspec, true, origin: uid);
        _jittering.DoJitter(args.Target, component.JitterDuration, true, 1f, 1f);
    }
    private void OnReap(EntityUid uid, RevenantComponent component, RevenantReapActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAbility(uid, component, 0, component.ReapDebuffs))
            return;

        args.Handled = true;

        var lookup = _lookup.GetEntitiesInRange(uid, component.ReapRadius, LookupFlags.Dynamic);
        var mobState = GetEntityQuery<MobStateComponent>();
        foreach (var ent in lookup)
        {
            if (!mobState.HasComponent(ent) || !_mobState.IsAlive(ent) || TryComp<RevenantComponent>(ent, out _))
                continue;
            if (!TryComp<EssenceComponent>(ent, out var essence))
                continue;

            var essenceAmount = essence.EssenceAmount * essence.EssenceLossMultiplier;
            essence.EssenceAmount -= essenceAmount;
            _revenant.ChangeEssenceAmount(uid, essenceAmount, component);
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2>
                { {component.StolenEssenceCurrencyPrototype, essenceAmount} }, uid);
            DamageSpecifier dspec = new();
            dspec.DamageDict.Add("Cold", 5f);
            _damage.TryChangeDamage(ent, dspec, true, origin: uid);
        }
        JitterAround(component.JitterDuration, lookup);
    }

    #endregion Abilities

    public void JitterAround(TimeSpan time, HashSet<EntityUid> lookup)
    {
        var mobState = GetEntityQuery<MobStateComponent>();
        foreach (var ent in lookup)
        {
            if (!TryComp<EssenceComponent>(ent, out var _))
                continue;
            if (!mobState.HasComponent(ent) || !_mobState.IsAlive(ent) || TryComp<RevenantComponent>(ent, out _))
                continue;
            _popup.PopupEntity("Goida", ent, ent);
            _jittering.DoJitter(ent, time, true, 1f, 1f);
        }
    }

    private bool TryUseAbility(EntityUid uid, RevenantComponent component, FixedPoint2 abilityCost, Vector2 debuffs)
    {
        if (component.Essence <= abilityCost)
        {
            _popup.PopupEntity(Loc.GetString("revenant-not-enough-essence"), uid, uid);
            return false;
        }

        var tileref = Transform(uid).Coordinates.GetTileRef();
        if (tileref != null)
        {
            if(_physics.GetEntitiesIntersectingBody(uid, (int) CollisionGroup.Impassable).Count > 0)
            {
                _popup.PopupEntity(Loc.GetString("revenant-in-solid"), uid, uid);
                return false;
            }
        }

        _revenant.ChangeEssenceAmount(uid, -abilityCost, component, false);

        if (debuffs.Y > 0)
            _statusEffects.TryAddStatusEffect<CorporealComponent>(uid, "Corporeal", TimeSpan.FromSeconds(debuffs.Y), false);
        _stun.TryStun(uid, TimeSpan.FromSeconds(debuffs.X), false);

        return true;
    }
}
