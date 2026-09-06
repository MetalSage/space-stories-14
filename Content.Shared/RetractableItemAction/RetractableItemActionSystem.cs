using Content.Shared.Actions;
using Content.Shared.Cuffs;
using Content.Shared.Hands;
using Content.Shared.Hands.Components; // Stories-RetractableItems
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Shared.RetractableItemAction;

/// <summary>
/// System for handling retractable items, such as armblades.
/// </summary>
public sealed partial class RetractableItemActionSystem : EntitySystem
{
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedPopupSystem _popups = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RetractableItemActionComponent, MapInitEvent>(OnActionInit);
        SubscribeLocalEvent<RetractableItemActionComponent, OnRetractableItemActionEvent>(OnRetractableItemAction);

        SubscribeLocalEvent<ActionRetractableItemComponent, ComponentShutdown>(OnActionSummonedShutdown);
        Subs.SubscribeWithRelay<ActionRetractableItemComponent, HeldRelayedEvent<TargetHandcuffedEvent>>(OnItemHandcuffed, inventory: false);
    }

    private void OnActionInit(Entity<RetractableItemActionComponent> ent, ref MapInitEvent args)
    {
        _containers.EnsureContainer<Container>(ent, RetractableItemActionComponent.ContainerId);

        PopulateActionItem(ent.Owner);
    }

    private void OnRetractableItemAction(Entity<RetractableItemActionComponent> ent, ref OnRetractableItemActionEvent args)
    {
        if (_actions.GetAction(ent.Owner) is not { } action)
            return;

        if (action.Comp.AttachedEntity == null)
            return;

        if (ent.Comp.ActionItemUid == null)
            return;

        // Stories-RetractableItems-Start
        if (_hands.IsHolding(args.Performer, ent.Comp.ActionItemUid))
        {
            RetractRetractableItem(args.Performer, ent.Comp.ActionItemUid.Value, ent.Owner);
            args.Handled = true;
            return;
        }

        string? targetHandId = null;
        if (ent.Comp.TargetHandLocation != null && TryComp<HandsComponent>(args.Performer, out var handsComp))
        {
            foreach (var handId in handsComp.SortedHands)
            {
                if (_hands.TryGetHand((args.Performer, handsComp), handId, out var hand) && hand.Value.Location == ent.Comp.TargetHandLocation)
                {
                    targetHandId = handId;
                    break;
                }
            }
        }

        targetHandId ??= _hands.GetActiveHand(args.Performer);

        if (targetHandId == null)
            return;

        // Don't allow to summon an item if holding an unremoveable item in the target hand.
        var held = _hands.GetHeldItem(args.Performer, targetHandId);
        if (held != null && held != ent.Comp.ActionItemUid && !_hands.CanDropHeld(args.Performer, targetHandId, false))
        {
            _popups.PopupEntity(Loc.GetString("retractable-item-hand-cannot-drop"), args.Performer, args.Performer);
            return;
        }

        SummonRetractableItem(args.Performer, ent.Comp.ActionItemUid.Value, targetHandId, ent.Owner);
        args.Handled = true;
        // Stories-RetractableItems-End
    }

    private void OnActionSummonedShutdown(Entity<ActionRetractableItemComponent> ent, ref ComponentShutdown args)
    {
        if (_actions.GetAction(ent.Comp.SummoningAction) is not { } action)
            return;

        if (!TryComp<RetractableItemActionComponent>(action, out var retract) || retract.ActionItemUid != ent.Owner)
            return;

        // If the item is somehow destroyed, re-add it to the action.
        PopulateActionItem(action.Owner);
    }

    private void OnItemHandcuffed(Entity<ActionRetractableItemComponent> ent, ref HeldRelayedEvent<TargetHandcuffedEvent> args)
    {
        if (_actions.GetAction(ent.Comp.SummoningAction) is not { } action)
            return;

        if (action.Comp.AttachedEntity == null)
            return;

        if (_hands.GetActiveHand(action.Comp.AttachedEntity.Value) is not { })
            return;

        RetractRetractableItem(action.Comp.AttachedEntity.Value, ent, action.Owner);
    }

    private void PopulateActionItem(Entity<RetractableItemActionComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false) || TerminatingOrDeleted(ent))
            return;

        if (!PredictedTrySpawnInContainer(ent.Comp.SpawnedPrototype, ent.Owner, RetractableItemActionComponent.ContainerId, out var summoned))
            return;

        ent.Comp.ActionItemUid = summoned.Value;

        // Mark the unremovable item so it can be added back into the action.
        var summonedComp = AddComp<ActionRetractableItemComponent>(summoned.Value);
        summonedComp.SummoningAction = ent.Owner;
        Dirty(summoned.Value, summonedComp);

        Dirty(ent);
    }

    private void RetractRetractableItem(EntityUid holder, EntityUid item, Entity<RetractableItemActionComponent?> action)
    {
        if (!Resolve(action, ref action.Comp, false))
            return;

        RemComp<UnremoveableComponent>(item);
        var container = _containers.GetContainer(action, RetractableItemActionComponent.ContainerId);
        _containers.Insert(item, container);
        _audio.PlayPredicted(action.Comp.RetractSounds, holder, holder);
    }

    private void SummonRetractableItem(EntityUid holder, EntityUid item, string hand, Entity<RetractableItemActionComponent?> action)
    {
        if (!Resolve(action, ref action.Comp, false))
            return;

        _hands.TryForcePickup(holder, item, hand, checkActionBlocker: false);
        _audio.PlayPredicted(action.Comp.SummonSounds, holder, holder);
        EnsureComp<UnremoveableComponent>(item);
    }
}
