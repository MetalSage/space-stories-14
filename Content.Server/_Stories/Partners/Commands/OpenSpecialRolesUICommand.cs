using Content.Server._Stories.Partners;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server.EUI;
using Content.Server._Stories.Partners.UI;
using System.Linq;

namespace Content.Server._Stories.Partners.Commands;

[AnyCommand]
public sealed class OpenSpecialRolesCommand : IConsoleCommand
{
    [Dependency] private readonly PartnersManager _partners = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    public string Command => "openspecialrolesui";
    public string Description => "Открыть меню выдачи спец. ролей.";
    public string Help => "Usage: openspecialrolesui";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player == null || shell.Player.AttachedEntity == null)
            return;

        if (!_partners.TryGetInfo(shell.Player.UserId, out var data) || data.AllowedAntags.Length == 0)
            return;

        var ui = new SpecialRolesEui();
        _euiManager.OpenEui(ui, shell.Player);
    }
}
