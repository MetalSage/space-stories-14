using Content.Server.EUI;
using Content.Server.StationEvents;
using Content.Server.StationEvents.Components;
using Content.Server._Stories.Partners.Systems;
using Content.Shared.Eui;
using Content.Shared._Stories.Partners;
using Content.Shared._Stories.Partners.UI;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Partners.UI;

public sealed class SpecialRolesEui : BaseEui
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    private readonly SpecialRolesSystem _specialRoles;
    private readonly EventManagerSystem _event;

    public SpecialRolesEui()
    {
        IoCManager.InjectDependencies(this);
        _specialRoles = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SpecialRolesSystem>();
        _event = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<EventManagerSystem>();
    }

    public override async void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is SpecialRolesEuiMsg.GetRoleData msgData)
            SendRoleData(msgData.Role);
    }

    public void SendRoleData(string role)
    {
        int? minPlayers = null;
        int? earliStart = null;
        int? maxOccurrences = null;
        int? occurrences = null;

        int? timeSinceLastEvent = null;
        int? reoccurrenceDelay = null;

        if (
        _prototype.TryIndex<SpecialRolePrototype>(role, out var prototype) &&
        _prototype.TryIndex(prototype.GameRule, out var gameRulePrototype) &&
        gameRulePrototype.TryGetComponent<StationEventComponent>(out var comp, _factory)
        )
        {
            minPlayers = comp.MinimumPlayers;
            earliStart = comp.EarliestStart;
            maxOccurrences = comp.MaxOccurrences;
            occurrences = _event.GetOccurrences(gameRulePrototype);

            timeSinceLastEvent = (int)_event.TimeSinceLastEvent(gameRulePrototype).TotalMinutes;
            reoccurrenceDelay = comp.ReoccurrenceDelay;
        }

        SendMessage(new SpecialRolesEuiMsg.SendRoleData(
            role,
            _specialRoles.CanPick(Player, role, out var reason),
            occurrences,
            minPlayers,
            earliStart,
            maxOccurrences,
            timeSinceLastEvent,
            reoccurrenceDelay,
            reason
            ));
    }
}
