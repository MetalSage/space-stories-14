using Content.Shared._Stories.ForceUser;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Timing;
using Robust.Shared.Timing;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Misc;
using Content.Server._Stories.TetherGun;
using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Shared._Stories.PullTo;
using Content.Shared._Stories.Force.Lightsaber;
using Content.Shared.Inventory.Events;

namespace Content.Server._Stories.ForceUser;
public sealed partial class ForceUserSystem
{
    public void InitializeRecallEquipment()
    {
        SubscribeLocalEvent<ForceUserComponent, RecallEquipmentsEvent>(OnRecallEquipment);
        SubscribeLocalEvent<ForceUserComponent, DidEquipEvent>(OnEquipped);
        SubscribeLocalEvent<PulledToTimeOutEvent>(OnTimeOutEquipment);
    }
    private void OnEquipped(EntityUid uid, ForceUserComponent comp, DidEquipEvent args)
    {
        if (!_tagSystem.HasTag(args.Equipment, "ForceRecallEquipment"))
            return;

        comp.Equipments ??= new();

        comp.Equipments.TryAdd(args.Slot, args.Equipment);
    }
    private void OnTimeOutEquipment(PulledToTimeOutEvent args)
    {
        if (args.Handled || !_tagSystem.HasTag(args.EntityUid, "ForceRecallEquipment") || args.Component.PulledTo == null)
            return;

        _inventory.TryEquip(args.Component.PulledTo.Value, args.EntityUid, args.Component.Slot, true, true);
    }
    private void OnRecallEquipment(EntityUid uid, ForceUserComponent comp, RecallEquipmentsEvent args)
    {
        if (args.Handled || comp.Equipments == null)
            return;

        foreach (var (key, ent) in comp.Equipments)
        {
            if (_inventory.TryGetSlotEntity(uid, key, out var entity) && entity == ent)
                continue;
            if (_container.IsEntityInContainer(ent) && !_container.TryRemoveFromContainer(ent))
                continue;
            _tetherGunSystem.StopTether(ent);

            _pullTo.TryPullTo(ent, uid, PulledToOnEnter.Equip, key, 10f);
        }
        args.Handled = true;
    }
}
