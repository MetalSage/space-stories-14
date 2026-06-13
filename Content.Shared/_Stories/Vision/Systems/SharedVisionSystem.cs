using Content.Shared._Stories.Vision.Components;
using Content.Shared._Stories.Vision.Events;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared._Stories.Vision.Systems;

public abstract partial class SharedVisionSystem : EntitySystem
{
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VisionProviderComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VisionProviderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<VisionProviderComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<VisionProviderComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<VisionProviderComponent, InventoryRelayedEvent<RefreshVisionEvent>>(OnRelayedRefresh);
        SubscribeLocalEvent<VisionProviderComponent, ToggleVisionActionEvent>(OnToggleAction);
        SubscribeLocalEvent<VisionProviderComponent, AfterAutoHandleStateEvent>(OnHandleState);

        SubscribeLocalEvent<VisionComponent, ToggleVisionAlertEvent>(OnPlayerToggleAlert);
    }

    private void OnStartup(EntityUid uid, VisionProviderComponent comp, ComponentStartup args)
    {
        if (!HasComp<ItemComponent>(uid))
        {
            if (comp.ToggleAction != null && comp.ToggleActionEntity == null)
                _actions.AddAction(uid, ref comp.ToggleActionEntity, comp.ToggleAction, uid);
            if (comp.ToggleAlert != null)
                _alerts.ShowAlert(uid, comp.ToggleAlert.Value);

            UpdateVision(uid);
        }
    }

    private void OnShutdown(EntityUid uid, VisionProviderComponent comp, ComponentShutdown args)
    {
        if (!HasComp<ItemComponent>(uid) && comp.ToggleActionEntity != null)
        {
            _actions.RemoveAction(uid, comp.ToggleActionEntity);
            comp.ToggleActionEntity = null;
        }

        if (comp.ToggleAlert != null && !HasComp<ItemComponent>(uid))
        {
            _alerts.ClearAlert(uid, comp.ToggleAlert.Value);
        }

        if (_container.TryGetContainingContainer(uid, out var container))
            UpdateVision(container.Owner);
        else if (!HasComp<ItemComponent>(uid))
            UpdateVision(uid);
    }

    private void OnEquipped(EntityUid uid, VisionProviderComponent comp, GotEquippedEvent args)
    {
        if (comp.ToggleAction != null)
            _actions.AddAction(args.EquipTarget, ref comp.ToggleActionEntity, comp.ToggleAction, uid);
        if (comp.ToggleAlert != null)
            _alerts.ShowAlert(args.EquipTarget, comp.ToggleAlert.Value);

        UpdateVision(args.EquipTarget);
    }

    private void OnUnequipped(EntityUid uid, VisionProviderComponent comp, GotUnequippedEvent args)
    {
        _actions.RemoveProvidedActions(args.EquipTarget, uid);
        comp.ToggleActionEntity = null;

        if (comp.ToggleAlert != null)
            _alerts.ClearAlert(args.EquipTarget, comp.ToggleAlert.Value);

        UpdateVision(args.EquipTarget);
    }

    private void OnRelayedRefresh(EntityUid uid, VisionProviderComponent comp, ref InventoryRelayedEvent<RefreshVisionEvent> args)
    {
        OnRefresh(uid, comp, ref args.Args);
    }

    private void OnRefresh(EntityUid uid, VisionProviderComponent comp, ref RefreshVisionEvent args)
    {
        if (!comp.IsActive)
            return;

        args.IsActive = true;
        args.DrawLighting &= comp.DrawLighting;
        args.DrawShadows &= comp.DrawShadows;
        args.DrawFov &= comp.DrawFov;
        args.ThermalVision |= comp.ThermalVision;

        if (comp.Priority <= args.Priority)
            return;

        args.Priority = comp.Priority;
        args.Shader = comp.Shader;
        args.ThermalShader = comp.ThermalShader;
        args.AmbientColor = comp.AmbientColor;
        args.ThermalAmbientColor = comp.ThermalAmbientColor;
    }

    private void OnToggleAction(EntityUid uid, VisionProviderComponent comp, ToggleVisionActionEvent args)
    {
        if (args.Handled)
            return;

        ToggleProvider(uid, comp);
        args.Handled = true;
    }

    private void OnPlayerToggleAlert(EntityUid uid, VisionComponent vision, ToggleVisionAlertEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<VisionProviderComponent>(uid, out var innateProvider))
        {
            ToggleProvider(uid, innateProvider);
            args.Handled = true;
            return;
        }

        if (TryComp<InventoryComponent>(uid, out var inv))
        {
            var enumerator = _inventory.GetSlotEnumerator((uid, inv));
            while (enumerator.MoveNext(out var slot))
            {
                if (TryComp<VisionProviderComponent>(slot.ContainedEntity, out var prov))
                {
                    ToggleProvider(slot.ContainedEntity.Value, prov);
                    args.Handled = true;
                    return;
                }
            }
        }
    }

    private void OnHandleState(EntityUid uid, VisionProviderComponent comp, ref AfterAutoHandleStateEvent args)
    {
        if (_container.TryGetContainingContainer(uid, out var container))
            UpdateVision(container.Owner);
        else if (!HasComp<ItemComponent>(uid))
            UpdateVision(uid);
    }

    private void ToggleProvider(EntityUid uid, VisionProviderComponent comp)
    {
        comp.IsActive = !comp.IsActive;
        Dirty(uid, comp);

        if (_container.TryGetContainingContainer(uid, out var container))
            UpdateVision(container.Owner);
        else
            UpdateVision(uid);
    }

    public void UpdateVision(EntityUid user)
    {
        var ev = new RefreshVisionEvent();

        if (TryComp<VisionProviderComponent>(user, out var innateProvider))
            OnRefresh(user, innateProvider, ref ev);

        if (TryComp<InventoryComponent>(user, out var inv))
            _inventory.RelayEvent((user, inv), ref ev);

        if (_timing.ApplyingState)
        {
            if (TryComp<VisionComponent>(user, out var existingVision))
            {
                existingVision.IsActive = ev.IsActive;
                existingVision.DrawLighting = ev.DrawLighting;
                existingVision.DrawShadows = ev.DrawShadows;
                existingVision.DrawFov = ev.DrawFov;
                existingVision.ThermalVision = ev.ThermalVision;
                existingVision.Shader = ev.Shader;
                existingVision.ThermalShader = ev.ThermalShader;
                existingVision.AmbientColor = ev.AmbientColor;
                existingVision.ThermalAmbientColor = ev.ThermalAmbientColor;
                Dirty(user, existingVision);
            }
            return;
        }

        if (!ev.IsActive)
        {
            RemCompDeferred<VisionComponent>(user);
            return;
        }

        var vision = EnsureComp<VisionComponent>(user);
        vision.IsActive = ev.IsActive;
        vision.DrawLighting = ev.DrawLighting;
        vision.DrawShadows = ev.DrawShadows;
        vision.DrawFov = ev.DrawFov;
        vision.ThermalVision = ev.ThermalVision;
        vision.Shader = ev.Shader;
        vision.ThermalShader = ev.ThermalShader;
        vision.AmbientColor = ev.AmbientColor;
        vision.ThermalAmbientColor = ev.ThermalAmbientColor;

        Dirty(user, vision);
    }
}
