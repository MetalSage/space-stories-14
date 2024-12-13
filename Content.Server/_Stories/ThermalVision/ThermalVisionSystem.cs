using Content.Shared.Actions;
using Content.Shared.GameTicking;
using Content.Shared.Inventory.Events;
using Content.Shared._Stories.ThermalVision;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Stories.ThermalVision;

public sealed class ThermalVisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ThermalVisionClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ThermalVisionClothingComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ThermalVisionComponent, ComponentStartup>(OnStartUp);
        SubscribeLocalEvent<ThermalVisionComponent, ComponentShutdown>(OnShutdown);
    }
    private void OnUnequipped(EntityUid uid, ThermalVisionClothingComponent component, GotUnequippedEvent args)
    {
    if (args.Slot == "eyes" && !(TryComp<ThermalVisionComponent>(args.Equipee, out var comp) && comp.Innate))
        RemCompDeferred<ThermalVisionComponent>(args.Equipee);
    }
    private void OnEquipped(EntityUid uid, ThermalVisionClothingComponent component, GotEquippedEvent args)
    {
        if (_gameTiming.ApplyingState)
            return;

        if (args.Slot == "eyes" && component.Enabled && !HasComp<ThermalVisionComponent>(args.Equipee))
            AddComp<ThermalVisionComponent>(args.Equipee);
    }
    private void OnStartUp(EntityUid uid, ThermalVisionComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }
    private void OnShutdown(EntityUid uid, ThermalVisionComponent component, ComponentShutdown args)
    {
        Del(component.ToggleActionEntity);
    }
}
