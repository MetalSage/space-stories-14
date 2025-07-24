using Content.Client.Eui;
using Content.Client.Message;
using Content.Shared.Eui;
using Content.Shared._Stories.Partners.UI;
using Robust.Shared.Utility;
using Content.Shared._Stories.Partners;
using Robust.Client.Player;
using Content.Client.GameTicking.Managers;

namespace Content.Client._Stories.Partners.UI;

public sealed class SpecialRolesEui : BaseEui
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly ILocalizationManager _localization = default!;
    [Dependency] private readonly PartnersManager _partners = default!;

    private readonly ClientGameTicker _gameTicker;

    [ViewVariables] private readonly SpecialRolesMenu _menu;
    [ViewVariables] public string CurrentRole { get; private set; } = default!;

    public SpecialRolesEui()
    {
        IoCManager.InjectDependencies(this);
        _menu = new SpecialRolesMenu(this);
        _gameTicker = _entity.System<ClientGameTicker>();
    }

    public override void Opened()
    {
        base.Opened();
        _menu.OpenCentered();

        if (!_partners.TryGetInfo(out var info))
            return;

        var roles = info.AllowedAntags;
        foreach (var role in roles)
        {
            _localization.TryGetString($"role-{role}", out var locName);
            _menu.RoleSelectButton.AddItem(locName ?? role);
            _menu.RoleSelectButton.SetItemMetadata(_menu.RoleSelectButton.ItemCount - 1, role);
        }
        _menu.RoleSelectButton.SelectId(0);
        SelectRole(roles[0]);
    }

    public void SelectRole(string role)
    {
        CurrentRole = role;
        SendMessage(new SpecialRolesEuiMsg.GetRoleData(role));
    }

    // FIXME: Pls
    private FormattedMessage GetStatusLabel(
        int? earliestStart,
        int? minimumPlayers,
        int? issuedRoles,
        int? maxIssuance,
        int? timeSinceLastEvent,
        int? reoccurrenceDelay,
        StatusLabel? reason)
    {
        List<string> msgs = [];

        if (earliestStart.HasValue)
        {
            var timeSpanEarliestStart = TimeSpan.FromMinutes(earliestStart.Value) - SponsorInfo.TimeAdvantage;
            timeSpanEarliestStart = timeSpanEarliestStart < TimeSpan.Zero ? TimeSpan.Zero : timeSpanEarliestStart;

            var currentTime = _gameTicker.RoundDuration();

            var earliestStartString = currentTime >= timeSpanEarliestStart
                ? $"[color=green]{currentTime.Minutes} / {timeSpanEarliestStart.Minutes}[/color]"
                : $"[color=red]{currentTime.Minutes} / {timeSpanEarliestStart.Minutes}[/color]";

            msgs.Add(Loc.GetString("special-roles-status-start", ("earliest-start", earliestStartString)));
        }

        if (minimumPlayers.HasValue)
        {
            var minimumPlayersString = _playerManager.PlayerCount >= minimumPlayers
                ? $"[color=green]{_playerManager.PlayerCount} / {minimumPlayers}[/color]"
                : $"[color=red]{_playerManager.PlayerCount} / {minimumPlayers}[/color]";

            msgs.Add(Loc.GetString("special-roles-status-players", ("min-players", minimumPlayersString)));
        }

        if (timeSinceLastEvent.HasValue && reoccurrenceDelay.HasValue && timeSinceLastEvent != 0)
        {
            var timeSinceLastEventString = timeSinceLastEvent >= reoccurrenceDelay
                ? $"[color=green]{timeSinceLastEvent} / {reoccurrenceDelay}[/color]"
                : $"[color=red]{timeSinceLastEvent} / {reoccurrenceDelay}[/color]";

            msgs.Add(Loc.GetString("special-roles-status-delay", ("delay", timeSinceLastEventString)));
        }

        if (maxIssuance.HasValue && issuedRoles.HasValue)
        {
            var maxRoleString = issuedRoles < maxIssuance
                ? $"[color=green]{issuedRoles} / {maxIssuance}[/color]"
                : $"[color=red]{issuedRoles} / {maxIssuance}[/color]";

            msgs.Add(Loc.GetString("special-roles-status-max", ("max-role", maxRoleString)));
        }

        if (reason.HasValue)
        {
            var reasonString = Loc.GetString($"special-role-reason-{reason.Value.ToString().ToLower()}");

            msgs.Add(Loc.GetString("special-roles-status-reason", ("reason", reasonString)));
        }
        else
        {
            msgs.Add(Loc.GetString("special-roles-status-success"));
        }

        var msg = new FormattedMessage();
        foreach (var txt in msgs)
        {
            if (!msg.IsEmpty)
                msg.AddText("\n");

            msg.AddText(txt);
        }

        return msg;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not SpecialRolesEuiMsg.SendRoleData msgData)
            return;

        CurrentRole = msgData.Role;
        _menu.Request.Disabled = !msgData.Pickable;
        _menu.StatusLabel.SetMarkup(GetStatusLabel(
            msgData.EarliestStart,
            msgData.MinimumPlayers,
            msgData.Occurrences,
            msgData.MaxOccurrences,
            msgData.TimeSinceLastEvent,
            msgData.ReoccurrenceDelay,
            msgData.Reason)
            .ToMarkup());
        _menu.Title = Loc.GetString("ui-escape-antag-select");
    }
}

