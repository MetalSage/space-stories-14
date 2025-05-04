using Content.Shared.Actions;
using Content.Shared.Inventory.Events;
using Robust.Shared.Timing;
using Content.Shared._Stories.Nightvision;

namespace Content.Server._Stories.Nightvision;

public sealed class NightvisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NightvisionClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<NightvisionClothingComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<NightvisionComponent, ComponentStartup>(OnStartUp);
        SubscribeLocalEvent<NightvisionComponent, ComponentShutdown>(OnShutdown);
    }
    private void OnUnequipped(EntityUid uid, NightvisionClothingComponent component, GotUnequippedEvent args)
    {
        if (args.Slot == "eyes")
            RemCompDeferred<NightvisionComponent>(args.Equipee);
    }
    private void OnEquipped(EntityUid uid, NightvisionClothingComponent component, GotEquippedEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        if (component.Enabled && !HasComp<NightvisionComponent>(args.Equipee) && (args.Slot == "eyes"))
        {
            var comp = _factory.GetComponent<NightvisionComponent>();
            comp.ToggleOnSound = component.ToggleOnSound;
            AddComp(args.Equipee, comp);
        }
    }
    private void OnStartUp(EntityUid uid, NightvisionComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }
    private void OnShutdown(EntityUid uid, NightvisionComponent component, ComponentShutdown args)
    {
        Del(component.ToggleActionEntity);
    }
}
