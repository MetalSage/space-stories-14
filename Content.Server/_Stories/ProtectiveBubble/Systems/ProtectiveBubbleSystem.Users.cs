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

namespace Content.Server._Stories.ProtectiveBubble;

public sealed partial class ProtectiveBubbleSystem
{
    public void InitializeUser()
    {
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ProtectiveBubbleUserComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ProtectiveBubbleComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ProtectiveBubbleUserComponent, StopProtectiveBubbleEvent>(OnStopProtectiveBubble);
        SubscribeLocalEvent<CreateProtectiveBubbleEvent>(OnCreateProtectiveBubble);
    }

    private void OnInit(EntityUid uid, ProtectiveBubbleUserComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.StopActionEntity, out var act, component.StopAction);
    }

    private void OnShutdown(EntityUid uid, ProtectiveBubbleComponent component, ComponentShutdown args)
    {
        if (component.User != null)
            RemComp<ProtectiveBubbleUserComponent>(component.User.Value);
    }

    private void OnShutdown(EntityUid uid, ProtectiveBubbleUserComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(component.StopActionEntity);

        foreach (var (action, comp) in _actions.GetActions(uid))
        {
            if (comp.BaseEvent is CreateProtectiveBubbleEvent || comp.BaseEvent is InstantForceUserActionEvent instant && instant.Event is CreateProtectiveBubbleEvent)
                _actions.StartUseDelay(action);
        }
    }

    private void OnCreateProtectiveBubble(CreateProtectiveBubbleEvent args)
    {
        if (args.Handled || HasComp<ProtectiveBubbleUserComponent>(args.Performer))
            return;

        StartBubble(Transform(args.Performer).Coordinates, args.Proto, args.Performer, out var uid, out var component);

        args.Handled = true;
    }

    private void OnStopProtectiveBubble(EntityUid uid, ProtectiveBubbleUserComponent comp, StopProtectiveBubbleEvent args)
    {
        if (args.Handled || comp.ProtectiveBubble == null)
            return;

        StopBubble(comp.ProtectiveBubble.Value);

        args.Handled = true;
    }
}
