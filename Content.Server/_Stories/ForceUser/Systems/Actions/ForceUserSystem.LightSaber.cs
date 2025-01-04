using Content.Shared.Inventory.Events;
using Content.Shared._Stories.ForceUser;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Item;
using Content.Shared.Weapons.Misc;
using Content.Shared._Stories.Force.Lightsaber;
using Content.Server.Weapons.Melee.EnergySword;
using Content.Server._Stories.TetherGun;
using Content.Shared.Damage;
using Robust.Shared.Random;
using Content.Shared.Throwing;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Physics;
using Content.Shared.Interaction;
using Content.Server._Stories.ForceUser;
using Content.Shared.Verbs;
using Content.Shared._Stories.ForceUser.Actions.Events;

namespace Content.Server._Stories.ForceUser;

public sealed partial class ForceUserSystem
{
    public void InitializeLightsaber()
    {
        SubscribeLocalEvent<LightsaberComponent, DamageChangedEvent>(OnDamage);
        SubscribeLocalEvent<LightsaberComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<LightsaberComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<LightsaberComponent, GettingPickedUpAttemptEvent>(OnTryPickUp);
    }
    private void OnEquipped(EntityUid uid, LightsaberComponent comp, GotEquippedEvent args)
    {
        if (!TryComp<ForceUserComponent>(args.Equipee, out var force) || force.Lightsaber != null)
            return;
        BindLightsaber(args.Equipee, uid, force);
    }
    private void OnDamage(EntityUid uid, LightsaberComponent component, DamageChangedEvent args)
    {
        if (!HasComp<TetheredComponent>(uid) || args.DamageDelta == null) return;
        if (args.DamageDelta.GetTotal() <= 0) return;

        var prob = args.DamageDelta.GetTotal().Float() * 0.01f; // Урон > 100 = 100%
        if (!_random.Prob(prob > 1 ? 1 : prob))
            return;

        if (TryComp<TetheredComponent>(uid, out var comp) && TryComp<TetherGunComponent>(comp.Tetherer, out var tetherGunComponent))
            _tetherGunSystem.StopTether(comp.Tetherer, tetherGunComponent);

        if (_random.Prob(component.DeactivateProb))
            _toggleSystem.TryDeactivate(uid);

        if (args.Origin != uid && args.Origin != null)
            _throwing.TryThrow(uid, _xform.GetWorldPosition(uid, GetEntityQuery<TransformComponent>()) - _xform.GetWorldPosition(Transform(args.Origin.Value), GetEntityQuery<TransformComponent>()), 10, uid, 0);
    }
    private void OnTryPickUp(EntityUid uid, LightsaberComponent component, GettingPickedUpAttemptEvent args)
    {
        if (component.LightsaberOwner != null && args.User != component.LightsaberOwner && HasComp<TetheredComponent>(uid))
        {
            args.Cancel();
            return;
        }

        if (component.LightsaberOwner != args.User && _toggleSystem.IsActivated(uid))
            _toggleSystem.TryDeactivate(uid);
    }
    private void OnActivateAttempt(EntityUid uid, LightsaberComponent comp, ref ItemToggleActivateAttemptEvent args)
    {
        if (comp.LightsaberOwner != args.User) // TODO: Черт, как его включить, если меня клонировали?
            args.Cancelled = true;
    }
}
