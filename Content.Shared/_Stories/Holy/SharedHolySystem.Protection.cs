using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Prying.Components;
using Content.Shared.Timing;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Holy;

public abstract partial class SharedHolySystem
{
    private void InitializeProtection()
    {
        base.Initialize();

        SubscribeLocalEvent<HolyComponent, GettingPickedUpAttemptEvent>(OnPickedUpAttempt);
        SubscribeLocalEvent<HolyComponent, BeforePryEvent>(OnBeforePry);

        SubscribeLocalEvent<HolyComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HolyComponent, HeldRelayedEvent<StartCollideEvent>>(OnCollide);
        SubscribeLocalEvent<HolyComponent, InventoryRelayedEvent<StartCollideEvent>>(OnCollide);

        SubscribeLocalEvent<HolyComponent, ContactInteractionEvent>(OnContact);
        SubscribeLocalEvent<HolyComponent, HeldRelayedEvent<ContactInteractionEvent>>(OnContact);
        SubscribeLocalEvent<HolyComponent, InventoryRelayedEvent<ContactInteractionEvent>>(OnContact);

        SubscribeLocalEvent<HolyComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<HolyComponent, HeldRelayedEvent<DamageModifyEvent>>(OnDamageModify);
        SubscribeLocalEvent<HolyComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);
    }

    private void UpdateProtection(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HolyComponent, UseDelayComponent>();

        while (query.MoveNext(out var uid, out var holy, out var useDelay))
        {
            // Защита от случаев, когда святой предмет взяли пока он был на кд.
            if (!_useDelay.IsDelayed((uid, useDelay), HolyDelay))
            {
                if (_container.TryGetContainingContainer((uid, null, null), out var container) && IsUnholy(container.Owner))
                {
                    if (_container.TryRemoveFromContainer(uid, false))
                        TryApplyProtection(container.Owner, (uid, holy));
                }
            }
        }
    }

    private void OnBeforePry(Entity<HolyComponent> entity, ref BeforePryEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = TryApplyProtection(args.User, entity);
    }

    private void OnPickedUpAttempt(Entity<HolyComponent> entity, ref GettingPickedUpAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (TryApplyProtection(args.User, entity))
            args.Cancel();
    }

    private void OnContact(Entity<HolyComponent> entity, ref HeldRelayedEvent<ContactInteractionEvent> args)
    {
        OnContact(entity, ref args.Args);
    }

    private void OnContact(Entity<HolyComponent> entity, ref InventoryRelayedEvent<ContactInteractionEvent> args)
    {
        OnContact(entity, ref args.Args);
    }

    private void OnContact(Entity<HolyComponent> entity, ref ContactInteractionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = TryApplyProtection(args.Other, entity);
    }

    private void OnCollide(Entity<HolyComponent> entity, ref StartCollideEvent args)
    {
        TryApplyProtection(args.OtherEntity, entity);
    }

    private void OnCollide(Entity<HolyComponent> entity, ref HeldRelayedEvent<StartCollideEvent> args)
    {
        OnCollide(entity, ref args.Args);
    }

    private void OnCollide(Entity<HolyComponent> entity, ref InventoryRelayedEvent<StartCollideEvent> args)
    {
        OnCollide(entity, ref args.Args);
    }

    private void OnDamageModify(Entity<HolyComponent> entity, ref DamageModifyEvent args)
    {
        if (!(args.Origin is { } origin) || !IsUnholy(origin))
            return;

        if (TryComp<UseDelayComponent>(entity, out var useDelay))
        {
            if (!_useDelay.TryResetDelay((entity, useDelay), true, HolyDelay))
                return;
        }

        if (!TryComp<UnholyComponent>(origin, out var unholy))
            return;

        var coefficient = unholy.ResistanceCoefficient;

        // У меня есть некоторые сомнения насчет кода ниже

        var damageModifierSet = (DamageModifierSet)_prototype.Index(entity.Comp.ProtectionDamageDamageModifierSet);

        Dictionary<string, float> newCoefficients = new();
        foreach (var c in damageModifierSet.Coefficients)
        {
            newCoefficients.Add(c.Key, c.Value * coefficient);
        }

        Dictionary<string, float> newFlatReductions = new();
        foreach (var r in damageModifierSet.FlatReduction)
        {
            newFlatReductions.Add(r.Key, r.Value * coefficient);
        }

        damageModifierSet.Coefficients = newCoefficients;
        damageModifierSet.FlatReduction = newFlatReductions;

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, damageModifierSet);
    }

    private void OnDamageModify(Entity<HolyComponent> entity, ref HeldRelayedEvent<DamageModifyEvent> args)
    {
        OnDamageModify(entity, ref args.Args);
    }

    private void OnDamageModify(Entity<HolyComponent> entity, ref InventoryRelayedEvent<DamageModifyEvent> args)
    {
        OnDamageModify(entity, ref args.Args);
    }

}
