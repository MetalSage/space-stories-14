using Content.Shared.Damage.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Content.Shared.Weapons.Misc;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Shared.Damage.Systems;

public sealed partial class DamageContactsSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private EntityWhitelistSystem _whitelistSystem = default!;

    [Dependency] private EntityQuery<DamageContactsComponent> _damageQuery = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageContactsComponent, StartCollideEvent>(OnEntityEnter);
        SubscribeLocalEvent<DamageContactsComponent, EndCollideEvent>(OnEntityExit);
        SubscribeLocalEvent<ItemToggleDamageContactsComponent, ItemToggledEvent>(OnItemToggle); // Stories
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DamagedByContactComponent>();

        while (query.MoveNext(out var ent, out var damaged))
        {
            if (_timing.CurTime < damaged.NextSecond)
                continue;
            damaged.NextSecond = _timing.CurTime + TimeSpan.FromSeconds(1);

            if (damaged.Damage != null)
                _damageable.TryChangeDamage(ent, damaged.Damage, interruptsDoAfters: false);
        }
    }

    private void OnEntityExit(EntityUid uid, DamageContactsComponent component, ref EndCollideEvent args)
    {
        var otherUid = args.OtherEntity;

        if (!TryComp<PhysicsComponent>(otherUid, out var body))
            return;

        foreach (var ent in _physics.GetContactingEntities(otherUid, body))
        {
            if (ent == uid)
                continue;

            if (_damageQuery.HasComponent(ent))
                return;
        }

        RemComp<DamagedByContactComponent>(otherUid);
    }

    private void OnEntityEnter(EntityUid uid, DamageContactsComponent component, ref StartCollideEvent args)
    {
        var otherUid = args.OtherEntity;

        if (HasComp<DamagedByContactComponent>(otherUid))
            return;

        if (_whitelistSystem.IsWhitelistPass(component.IgnoreWhitelist, otherUid))
            return;

        if (!HasComp<TetheredComponent>(uid) && component.OnlyTethered) // Stories
            return;

        var damagedByContact = EnsureComp<DamagedByContactComponent>(otherUid);
        damagedByContact.Damage = component.Damage;
        if (component.HitSound != null) _audio.PlayPredicted(component.HitSound, uid, otherUid); // Stories
    }

    private void OnItemToggle(EntityUid uid, ItemToggleDamageContactsComponent itemToggleMelee, ItemToggledEvent args)
    {
        if (!TryComp(uid, out DamageContactsComponent? meleeWeapon))
            return;

        if (args.Activated)
        {
            if (itemToggleMelee.ActivatedDamage != null)
            {
                //Setting deactivated damage to the weapon's regular value before changing it.
                itemToggleMelee.DeactivatedDamage ??= meleeWeapon.Damage;
                meleeWeapon.Damage = itemToggleMelee.ActivatedDamage;
            }

            meleeWeapon.HitSound = itemToggleMelee.ActivatedSoundOnHit;
        }
        else
        {
            if (itemToggleMelee.DeactivatedDamage != null)
                meleeWeapon.Damage = itemToggleMelee.DeactivatedDamage;

            meleeWeapon.HitSound = itemToggleMelee.DeactivatedSoundOnHit;
        }
    }
}
