using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Content.Server.Database;
using Content.Server._Stories.Partners.Systems;
using Content.Shared._Stories.Partners;
using Content.Server._Corvax.Sponsors;

namespace Content.Server._Stories.Partners.Commands;

[AnyCommand]
public sealed class PickAntagCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPartnersManager _db = default!;
    [Dependency] private readonly SponsorsManager _partners = default!;
    public string Command => "pickspecialrole";
    public string Description => "Выдает роль.";
    public string Help => "Usage: pickspecialrole dragon";
    /// <summary>
    /// Тир с которого в БД не будут блокаться взятие антагов.
    /// </summary>
    private const int UnlimitedTier = 5; // FIXME: Pls
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

        //FIXME: Полная дичь, но работает.
        if (data == null || data.LastDayTakingAntag == DateTime.Now.DayOfYear)
        {
            shell.WriteLine("No. >_<");
            return;
        }

        specialRoles.Pick(player, role);

        if (data.Tier < UnlimitedTier)
            _db.SetAntagPicked(player.UserId);
    }
}

