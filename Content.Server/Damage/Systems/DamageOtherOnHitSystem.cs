using Content.Server.Administration.Logs;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.Mobs.Components;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Server.Damage.Systems;

public sealed class DamageOtherOnHitSystem : SharedDamageOtherOnHitSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly GunSystem _guns = default!;
    [Dependency] private readonly Shared.Damage.Systems.DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageOtherOnHitComponent, ThrowDoHitEvent>(OnDoHit);
        SubscribeLocalEvent<ItemToggleDamageOtherOnHitComponent, ItemToggledEvent>(OnItemToggle);
    }

    private void OnDoHit(EntityUid uid, DamageOtherOnHitComponent component, ThrowDoHitEvent args)
    {
        if (TerminatingOrDeleted(args.Target))
            return;

        var dmg = _damageable.ChangeDamage(args.Target, component.Damage * _damageable.UniversalThrownDamageModifier, component.IgnoreResistances, origin: args.Component.Thrower);

        // Log damage only for mobs. Useful for when people throw spears at each other, but also avoids log-spam when explosions send glass shards flying.
        if (HasComp<MobStateComponent>(args.Target))
            _adminLogger.Add(LogType.ThrowHit, $"{ToPrettyString(args.Target):target} received {dmg.GetTotal():damage} damage from collision");

        if (!dmg.Empty)
        {
            _color.RaiseEffect(Color.Red, [args.Target], Filter.Pvs(args.Target, entityManager: EntityManager));
        }

        _guns.PlayImpactSound(args.Target, dmg, null, false);
        if (TryComp<PhysicsComponent>(uid, out var body) && body.LinearVelocity.LengthSquared() > 0f)
        {
            var direction = body.LinearVelocity.Normalized();
            _sharedCameraRecoil.KickCamera(args.Target, direction);
        }
    }

    private void OnItemToggle(EntityUid uid, ItemToggleDamageOtherOnHitComponent itemToggleMelee, ItemToggledEvent args)
    {
        if (!TryComp(uid, out DamageOtherOnHitComponent? meleeWeapon))
            return;

        TryComp(uid, out DamageContactsComponent? contactsComp);

        if (args.Activated)
        {
            if (itemToggleMelee.ActivatedDamage != null)
            {
                //Setting deactivated damage to the weapon's regular value before changing it.
                itemToggleMelee.DeactivatedDamage ??= meleeWeapon.Damage;
                meleeWeapon.Damage = itemToggleMelee.ActivatedDamage;
            }

            if (contactsComp != null)
                contactsComp.HitSound = itemToggleMelee.ActivatedSoundOnHit;
        }
        else
        {
            if (itemToggleMelee.DeactivatedDamage != null)
                meleeWeapon.Damage = itemToggleMelee.DeactivatedDamage;

            if (contactsComp != null)
                contactsComp.HitSound = itemToggleMelee.DeactivatedSoundOnHit;
        }
    }
}
