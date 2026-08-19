using Content.Shared._Stories.Demons;
using Content.Shared._Stories.PullTo;
using Content.Shared.Damage.Systems;
using Content.Shared.Item;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Server._Stories.Demons;

public sealed partial class ShadowGrappleSystem : EntitySystem
{
    [Dependency] private PullToSystem _pullTo = default!;
    [Dependency] private SharedPointLightSystem _pointLight = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedPoweredLightSystem _poweredLight = default!;
    [Dependency] private SharedLightBulbSystem _lightBulb = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowGrappleProjectileComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<ShadowGrappleComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<ShadowGrappleComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<ItemComponent>(hit) || !TryComp<PointLightComponent>(hit, out var light))
                continue;

            _pointLight.SetEnabled(hit, false, light);
        }
    }

    private void OnProjectileHit(Entity<ShadowGrappleProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (args.Shooter is not { } shooter || !TryComp<ShadowGrappleComponent>(shooter, out var grapple))
            return;

        var target = args.Target;

        if (HasComp<MobStateComponent>(target))
        {
            _pullTo.TryPullTo(target, shooter, PulledToOnEnter.None, duration: grapple.PullDuration);
            _stun.TryAddParalyzeDuration(target, grapple.ImmobilizeDuration);
            _damageable.TryChangeDamage(target, grapple.HitDamage);
        }
        else
        {
            _pullTo.TryPullTo(shooter, target, PulledToOnEnter.None, duration: grapple.PullDuration);
        }

        BreakLightsNearby(Transform(target).Coordinates, grapple.ExtinguishRange);
    }

    public void ExtinguishNearby(EntityCoordinates coords, float range)
    {
        foreach (var light in _lookup.GetEntitiesInRange<PointLightComponent>(coords, range))
        {
            _pointLight.SetEnabled(light.Owner, false, light.Comp);
        }
    }

    public void BreakLightsNearby(EntityCoordinates coords, float range)
    {
        foreach (var fixture in _lookup.GetEntitiesInRange<PoweredLightComponent>(coords, range))
        {
            if (_poweredLight.GetBulb(fixture.Owner, fixture.Comp) is not { } bulb)
                continue;

            _lightBulb.SetState(bulb, LightBulbState.Broken);
            _lightBulb.PlayBreakSound(bulb);
        }
    }
}
