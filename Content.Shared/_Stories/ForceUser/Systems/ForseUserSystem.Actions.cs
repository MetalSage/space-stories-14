using Content.Shared._Stories.Force;
using Content.Shared._Stories.ForceUser.Actions.Events;
using Content.Shared.Actions;
using Content.Shared.Popups;

namespace Content.Shared._Stories.ForceUser;

public abstract partial class SharedForceUserSystem
{
    private void InitializeActions()
    {
        SubscribeLocalEvent<ForceComponent, InstantForceUserActionEvent>(OnForceAction);
        SubscribeLocalEvent<ForceComponent, EntityTargetForceUserActionEvent>(OnForceAction);
        SubscribeLocalEvent<ForceComponent, WorldTargetForceUserActionEvent>(OnForceAction);
    }

    private void OnForceAction(EntityUid uid, ForceComponent component, IForceActionEvent args)
    {
        BaseActionEvent? argsBaseAction = null;
        BaseActionEvent? eventToRaise = null;

        if (args is BaseActionEvent @event)
            argsBaseAction = @event;
        else
            return;

        if (args.BaseEvent is InstantActionEvent instant && args is InstantForceUserActionEvent argsInstant)
        {
            instant.Handled = false;
            instant.Performer = argsInstant.Performer;
            eventToRaise = instant;
        }

        if (args.BaseEvent is EntityTargetActionEvent entityTarget &&
            args is EntityTargetForceUserActionEvent argsEntityTarget)
        {
            entityTarget.Handled = false;
            entityTarget.Performer = argsEntityTarget.Performer;
            entityTarget.Target = argsEntityTarget.Target;
            eventToRaise = entityTarget;
        }

        if (args.BaseEvent is WorldTargetActionEvent worldTarget &&
            args is WorldTargetForceUserActionEvent argsWorldTarget)
        {
            worldTarget.Handled = false;
            worldTarget.Performer = argsWorldTarget.Performer;
            worldTarget.Target = argsWorldTarget.Target;
            eventToRaise = worldTarget;
        }

        if (eventToRaise == null)
            return;

        if (component.CurrentDebuff >= args.MaxDebuff)
        {
            _popup.PopupEntity("Перегружен!", uid, uid, PopupType.SmallCaution);
            return;
        }

        if (!_force.RemoveVolume(eventToRaise.Performer, args.Volume))
        {
            _popup.PopupEntity("Недостаточно сил!", uid, uid, PopupType.SmallCaution);
            return;
        }

        RaiseLocalEvent(eventToRaise.Performer, (object)eventToRaise, true);

        argsBaseAction.Handled = true;
    }
}
