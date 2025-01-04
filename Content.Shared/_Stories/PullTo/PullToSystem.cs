using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands.EntitySystems;
using Content.Shared._Stories.Force.Lightsaber;
using Robust.Shared.Physics.Events;
using Content.Shared.Weapons.Misc;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Shared._Stories.Force;
using Content.Shared.Throwing;
using Content.Shared.Inventory;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Components;
using System.Numerics;

namespace Content.Shared._Stories.PullTo;
public sealed partial class PullToSystem : EntitySystem
{
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<PulledToComponent, StartCollideEvent>(OnEntityEnter);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PulledToComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.PulledTo == null || Deleted(comp.PulledTo))
                continue;

            comp.ActiveInterval -= frameTime;
            if (comp.ActiveInterval <= 0)
            {
                comp.ActiveInterval = comp.Interval;
                _throwing.TryThrow(uid, Transform(comp.PulledTo.Value).Coordinates, comp.Strength);
            }

            if (comp.Duration == null)
                continue;

            comp.Duration -= frameTime;

            if (comp.Duration <= 0)
            {
                RaiseLocalEvent(uid, new PulledToTimeOutEvent(uid, comp), true);
                RemCompDeferred<PulledToComponent>(uid);
            }
        }
    }
    public void TryPullTo(EntityUid item, EntityUid pulledTo, PulledToOnEnter pulledToOnEnter = PulledToOnEnter.PickUp, string slot = "Pocket", float? duration = null)
    {
        var component = _factory.GetComponent<PulledToComponent>();
        component.PulledTo = pulledTo;
        component.OnEnter = pulledToOnEnter;
        component.Duration = duration;
        component.Slot = slot;
        AddComp(item, component, true);
    }
    private void OnEntityEnter(EntityUid uid, PulledToComponent component, StartCollideEvent args)
    {
        if (args.OtherEntity != component.PulledTo)
            return;

        switch (component.OnEnter)
        {
            case PulledToOnEnter.None:
                RecursivelyUpdatePhysics(uid);
                break;
            case PulledToOnEnter.PickUp:
                _hands.TryPickupAnyHand(args.OtherEntity, uid);
                break;
            case PulledToOnEnter.Equip:
                _inventory.TryEquip(args.OtherEntity, uid, component.Slot, true, true);
                break;
        }

        RemCompDeferred<PulledToComponent>(uid);
    }
    private void RecursivelyUpdatePhysics(EntityUid uid, TransformComponent? xform = null, PhysicsComponent? physics = null)
    {
        if (!Resolve(uid, ref xform, ref physics))
            return;

        _physics.SetLinearVelocity(uid, Vector2.Zero, false, body: physics);
        _physics.SetAngularVelocity(uid, 0, false, body: physics);

        var children = xform.ChildEnumerator;

        while (children.MoveNext(out var child))
        {
            if (TryComp<TransformComponent>(child, out var childXform) && TryComp<PhysicsComponent>(child, out var childPhysics))
                RecursivelyUpdatePhysics(child, childXform, childPhysics);
        }
    }
}
