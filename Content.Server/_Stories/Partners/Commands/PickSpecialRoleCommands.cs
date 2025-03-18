using Content.Server._Stories.Partners.Systems;
using Content.Shared._Stories.Partners;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Partners.Commands;

[AnyCommand]
public sealed class PickAntagCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PartnersManager _partners = default!;
    public string Command => "pickspecialrole";
    public string Description => "Выдает роль.";
    public string Help => "Usage: pickspecialrole dragon";
    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1)
            return;

        var role = args[0];

        if (!_proto.TryIndex<SpecialRolePrototype>(role, out var proto))
            return;

        if (!(shell.Player is { } player))
            return;

        if (!(player.AttachedEntity is { } uid))
            return;

        var specialRoles = _entityManager.System<SpecialRolesSystem>();

        if (!specialRoles.CanPick(player, role, out var reason))
        {
            shell.WriteError(reason?.ToString() ?? "Success?");
            return;
        }
        var data = await _partners.LoadSponsorInfo(player.UserId);

        if (data == null || data.Tokens <= 0)
        {
            shell.WriteLine("Нет токенов для выбора роли.");
            return;
        }

        specialRoles.Pick(player, role);

        shell.WriteLine($"Роль {proto.ID} выдана.");
    }
}
