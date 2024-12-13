using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Administration;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Ban)]
public sealed class PlayTimeReduceOverallCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;

    public string Command => "playtime_reduceoverall";
    public string Description => Loc.GetString("cmd-playtime_reduceoverall-desc");
    public string Help => Loc.GetString("cmd-playtime_reduceoverall-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("cmd-playtime_reduceoverall-error-args"));
            return;
        }

        if (!int.TryParse(args[1], out var minutes))
        {
            shell.WriteError(Loc.GetString("parse-minutes-fail", ("minutes", args[1])));
            return;
        }

        if (!_playerManager.TryGetSessionByUsername(args[0], out var player))
        {
            shell.WriteError(Loc.GetString("parse-session-fail", ("username", args[0])));
            return;
        }

        _playTimeTracking.AddTimeToOverallPlaytime(player, TimeSpan.FromMinutes(minutes));
        var overall = _playTimeTracking.GetOverallPlaytime(player);

        shell.WriteLine(Loc.GetString(
            "cmd-playtime_reduceoverall-succeed",
            ("username", args[0]),
            ("time", overall)));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(),
                Loc.GetString("cmd-playtime_reduceoverall-arg-user"));

        if (args.Length == 2)
            return CompletionResult.FromHint(Loc.GetString("cmd-playtime_reduceoverall-arg-minutes"));

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Ban)]
public sealed class PlayTimeReduceRoleCommand : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;

    public string Command => "playtime_reducerole";
    public string Description => Loc.GetString("cmd-playtime_reducerole-desc");
    public string Help => Loc.GetString("cmd-playtime_reducerole-help", ("command", Command));

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteError(Loc.GetString("cmd-playtime_reducerole-error-args"));
            return;
        }

        var userName = args[0];
        if (!_playerManager.TryGetSessionByUsername(userName, out var player))
        {
            shell.WriteError(Loc.GetString("parse-session-fail", ("username", userName)));
            return;
        }

        var role = args[1];

        var m = args[2];
        if (!int.TryParse(m, out var minutes))
        {
            shell.WriteError(Loc.GetString("parse-minutes-fail", ("minutes", minutes)));
            return;
        }

        _playTimeTracking.AddTimeToTracker(player, role, TimeSpan.FromMinutes(minutes));
        var time = _playTimeTracking.GetOverallPlaytime(player);
        shell.WriteLine(Loc.GetString("cmd-playtime_reducerole-succeed",
            ("username", userName),
            ("role", role),
            ("time", time)));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-playtime_reducerole-arg-user"));
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<PlayTimeTrackerPrototype>(),
                Loc.GetString("cmd-playtime_reducerole-arg-role"));
        }

        if (args.Length == 3)
            return CompletionResult.FromHint(Loc.GetString("cmd-playtime_reducerole-arg-minutes"));

        return CompletionResult.Empty;
    }
}

