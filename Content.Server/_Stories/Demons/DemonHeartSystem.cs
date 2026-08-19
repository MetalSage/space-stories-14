using Content.Server.Popups;
using Content.Shared._Stories.Demons;
using Content.Shared.Actions;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Server._Stories.Demons;

public sealed partial class DemonHeartSystem : EntitySystem
{
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DemonHeartComponent, UseInHandEvent>(OnUse);
    }

    private void OnUse(Entity<DemonHeartComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var user = args.User;

        foreach (var entry in ent.Comp.Components.Values)
        {
            if (HasComp(user, entry.Component.GetType()))
            {
                _popup.PopupEntity(Loc.GetString(ent.Comp.AlreadyHasAbilityMessage), user, user);
                return;
            }
        }

        args.Handled = true;

        EntityManager.AddComponents(user, ent.Comp.Components);
        _actions.AddAction(user, ent.Comp.GrantedAction);

        _popup.PopupEntity(Loc.GetString(ent.Comp.ConsumedMessage), user, user, PopupType.LargeCaution);

        QueueDel(ent);
    }
}
