using Content.Client.Eui;
using Content.Client.Message;
using Content.Shared.Eui;
using Content.Shared._Stories.Partners.UI;
using Robust.Shared.Utility;
using Content.Shared._Stories.Partners;
using Robust.Client.Player;
using Content.Client.GameTicking.Managers;
using Content.Client._Stories.Partners;

namespace Content.Client._Stories.Partners.UI;

public sealed class SpecialRolesEui : BaseEui
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly PartnersManager _partners = default!;
    private readonly ClientGameTicker _gameTicker = default!;
    [ViewVariables] private readonly SpecialRolesMenu _menu;
    [ViewVariables] public string CurrentRole { get; set; } = default!;
    [ViewVariables] public HashSet<string> Roles { get; set; } = [];

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
            var name = role;
            if (Loc.TryGetString($"role-{role}", out var locName))
            {
                name = locName;
            }
            _menu.RoleSelectButton.AddItem(name);
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
    public FormattedMessage GetStatusLabel(int? earliestStart, int? minimumPlayers, int? issuedRoles, int? maxIssuance, int? timeSinceLastEvent, int? reoccurrenceDelay, StatusLabel? reason)
    {
        List<string> msgs = [];

        if (earliestStart.HasValue)
        {
            var currentTime = _gameTicker.RoundDuration();

            var earliestStartString = Math.Round(currentTime.TotalMinutes) >= earliestStart ? $"[color=#008000]{Math.Round(currentTime.TotalMinutes)} / {earliestStart}[/color]" : $"[color=#ff0000]{Math.Round(currentTime.TotalMinutes)} / {earliestStart}[/color]";

            msgs.Add(Loc.GetString("special-roles-status-start", ("earliest-start", earliestStartString)));
        }

        if (minimumPlayers.HasValue)
        {
            var minimumPlayersString = _playerManager.PlayerCount >= minimumPlayers ? $"[color=#008000]{_playerManager.PlayerCount} / {minimumPlayers}[/color]" : $"[color=#ff0000]{_playerManager.PlayerCount} / {minimumPlayers}[/color]";

            msgs.Add(Loc.GetString("special-roles-status-players", ("min-players", minimumPlayersString)));
        }

        if (timeSinceLastEvent.HasValue && reoccurrenceDelay.HasValue && timeSinceLastEvent != 0)
        {
            var timeSinceLastEventString = timeSinceLastEvent >= reoccurrenceDelay ? $"[color=#008000]{timeSinceLastEvent} / {reoccurrenceDelay}[/color]" : $"[color=#ff0000]{timeSinceLastEvent} / {reoccurrenceDelay}[/color]";

            msgs.Add(Loc.GetString("special-roles-status-delay", ("delay", timeSinceLastEventString)));
        }

        if (maxIssuance.HasValue && issuedRoles.HasValue)
        {
            var maxRoleString = issuedRoles < maxIssuance ? $"[color=#008000]{issuedRoles} / {maxIssuance}[/color]" : $"[color=#ff0000]{issuedRoles} / {maxIssuance}[/color]";

            msgs.Add(Loc.GetString("special-roles-status-max", ("max-role", maxRoleString)));
        }

        if (reason.HasValue)
        {
            var reasonString = Loc.GetString($"special-role-reason-{reason.Value.ToString().ToLower()}");

            msgs.Add(Loc.GetString("special-roles-status-reason", ("reason", reasonString)));
        }
        else msgs.Add(Loc.GetString("special-roles-status-success"));

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

        if (_menu == null)
            return;

        if (msg is SpecialRolesEuiMsg.SendRoleData msgData)
        {
            CurrentRole = msgData.Role;
            _menu.Request.Disabled = !msgData.Pickable;
            _menu.StatusLabel.SetMarkup(GetStatusLabel(msgData.EarliestStart, msgData.MinimumPlayers, msgData.Occurrences, msgData.MaxOccurrences, msgData.TimeSinceLastEvent, msgData.ReoccurrenceDelay, msgData.Reason).ToMarkup());
            _menu.Title = Loc.GetString("ui-escape-antagselect");
        }
    }
}

