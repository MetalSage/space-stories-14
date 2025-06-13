using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Shared._Stories.ForceUser;
using Content.Shared._Stories.ProtectiveBubble.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared._Stories.Force.Lightsaber;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;
using Robust.Shared.Serialization.Manager;
using Content.Shared._Stories.Force;
using Content.Shared.Rounding;
using Content.Shared.Damage;
using Content.Shared.Verbs;
using Content.Shared.Lock;
using Content.Shared.Database;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Item;

namespace Content.Server._Stories.ProtectiveBubble;

public sealed partial class ProtectiveBubbleSystem
{
    public const string GeneratorDelay = "Overheat";
    public void InitializeGenerator()
    {
        SubscribeLocalEvent<ProtectiveBubbleGeneratorComponent, DamageChangedEvent>(OnGeneratorDamage);
        SubscribeLocalEvent<ProtectiveBubbleGeneratorComponent, GetVerbsEvent<Verb>>(OnGetVerb);
        SubscribeLocalEvent<ProtectiveBubbleGeneratorComponent, ItemToggledEvent>(OnToggle);
        SubscribeLocalEvent<ProtectiveBubbleGeneratorComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<ProtectiveBubbleGeneratorComponent, GettingPickedUpAttemptEvent>(OnPickUp);
        SubscribeLocalEvent<GeneratedProtectiveBubbleComponent, DamageChangedEvent>(OnGeneratedDamage);
        SubscribeLocalEvent<GeneratedProtectiveBubbleComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnGetVerb(EntityUid uid, ProtectiveBubbleGeneratorComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (TryComp<LockComponent>(uid, out var lockComp) && lockComp.Locked)
            return;

        if (component.SelectableTypes.Count < 2)
            return;

        foreach (var type in component.SelectableTypes)
        {
            var proto = _proto.Index<EntityPrototype>(type);

            var v = new Verb
            {
                Priority = 1,
                Category = VerbCategory.SelectType,
                Text = proto.Name,
                Disabled = type == component.BubbleType,
                Impact = LogImpact.Medium,
                DoContactInteraction = true,
                Act = () =>
                {
                    component.BubbleType = type;
                    _popup.PopupEntity(Loc.GetString("emitter-component-type-set", ("type", proto.Name)), uid);
                }
            };
            args.Verbs.Add(v);
        }
    }

    private void OnActivateAttempt(EntityUid uid, ProtectiveBubbleGeneratorComponent component, ref ItemToggleActivateAttemptEvent args)
    {
        if (_useDelay.IsDelayed(uid, GeneratorDelay))
        {
            if (args.User is { } user)
                _popup.PopupEntity("Генератор слишком сильно нагрет!", uid, user); // FIXME: Hardcode
            else
                _popup.PopupEntity("Генератор слишком сильно нагрет!", uid); // FIXME: Hardcode
            args.Cancelled = true;
        }
    }

    private void OnPickUp(EntityUid uid, ProtectiveBubbleGeneratorComponent component, GettingPickedUpAttemptEvent args)
    {
        if (_itemToggle.IsActivated(uid))
            args.Cancel();
    }

    private void OnToggle(EntityUid uid, ProtectiveBubbleGeneratorComponent component, ref ItemToggledEvent args)
    {
        if (args.Activated)
        {
            component.ProtectiveBubble = StartBubble(Transform(uid).Coordinates, component.BubbleType, uid, out var bubble, out _);
            EnsureComp<GeneratedProtectiveBubbleComponent>(bubble).Generator = uid;
            EnsureComp<UnremoveableComponent>(uid);
            RemComp<ItemComponent>(uid);
        }
        else if (component.ProtectiveBubble is { } bubble)
        {
            StopBubble(bubble);
            RemComp<UnremoveableComponent>(uid);
            EnsureComp<ItemComponent>(uid);
            _item.SetSize(uid, "Ginormous"); // FIXME: Hardcode
        }
    }

    private void OnShutdown(EntityUid uid, GeneratedProtectiveBubbleComponent component, ComponentShutdown args)
    {
        if (component.Generator is { } gen)
        {
            _itemToggle.TryDeactivate(gen);
            _useDelay.SetLength(gen, TimeSpan.FromMinutes(5), GeneratorDelay);
            _useDelay.TryResetDelay(gen, id: GeneratorDelay);
        }
    }

    // FIXME: Это связка дамага, чтобы можно было чинить генератор, но
    // урон остается на генераторе, когда щит ломается, то есть его можно будет
    // чинить, когда щит будет иметь уже полное здоровье. Ой-ой, кому это надо.
    private void OnGeneratorDamage(EntityUid uid, ProtectiveBubbleGeneratorComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;

        if (component.ProtectiveBubble is { } bubble)
        {
            if (args.Origin == bubble)
                return;

            _damageable.TryChangeDamage(bubble, args.DamageDelta, true, origin: uid);
        }
    }

    private void OnGeneratedDamage(EntityUid uid, GeneratedProtectiveBubbleComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;

        if (component.Generator is { } gen)
        {
            if (args.Origin == gen)
                return;

            _damageable.TryChangeDamage(gen, args.DamageDelta, true, origin: uid);
        }
    }
}
