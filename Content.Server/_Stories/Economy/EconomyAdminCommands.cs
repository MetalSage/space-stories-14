using Content.Server._Stories.Economy.Components;
using Content.Server.Administration;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.Mind;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._Stories.Economy;

[AdminCommand(AdminFlags.Fun)]
public sealed class GetBalanceCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override string Command => "econ_getbalance";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        var located = await _playerLocator.LookupIdByNameOrIdAsync(args[0]);
        if (located == null)
        {
            shell.WriteError(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        var mindSystem = _entManager.System<SharedMindSystem>();
        var bankSystem = _entManager.System<BankSystem>();

        if (!mindSystem.TryGetMind(located.UserId, out var mindId, out _))
        {
            shell.WriteError(Loc.GetString("cmd-econ-error-no-mind"));
            return;
        }

        if (!_entManager.TryGetComponent<MindBankAccountComponent>(mindId.Value, out var bankAcc))
        {
            shell.WriteError(Loc.GetString("cmd-econ-error-no-account"));
            return;
        }

        if (bankAcc.BankStation != null &&
            bankSystem.TryGetAccount(bankAcc.BankStation.Value, bankAcc.AccountNumber, out var account))
        {
            shell.WriteLine(Loc.GetString("cmd-econ-getbalance-success",
                ("player", located.Username),
                ("balance", account.Balance)));
            return;
        }

        shell.WriteError(Loc.GetString("cmd-econ-error-station-account-missing"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-econ-arg-player"));
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class SetBalanceCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override string Command => "econ_setbalance";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!int.TryParse(args[1], out var amount) || amount < 0)
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-number"));
            return;
        }

        var located = await _playerLocator.LookupIdByNameOrIdAsync(args[0]);
        if (located == null)
        {
            shell.WriteError(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        var mindSystem = _entManager.System<SharedMindSystem>();
        var bankSystem = _entManager.System<BankSystem>();
        var economySystem = _entManager.System<EconomySystem>();

        if (!mindSystem.TryGetMind(located.UserId, out var mindId, out _))
        {
            shell.WriteError(Loc.GetString("cmd-econ-error-no-mind"));
            return;
        }

        if (!_entManager.TryGetComponent<MindBankAccountComponent>(mindId.Value, out var bankAcc))
        {
            shell.WriteError(Loc.GetString("cmd-econ-error-no-account"));
            return;
        }

        if (bankAcc.BankStation != null &&
            bankSystem.TryGetAccount(bankAcc.BankStation.Value, bankAcc.AccountNumber, out var account))
        {
            var oldBalance = account.Balance;
            account.Balance = amount;

            shell.WriteLine(Loc.GetString("cmd-econ-setbalance-success",
                ("player", located.Username),
                ("amount", amount)));

            economySystem.TrySendNotification(mindId.Value,
                Loc.GetString("bank-app-notification-admin-change-title"),
                Loc.GetString("bank-app-notification-admin-change-body", ("old", oldBalance), ("new", amount)));
            return;
        }

        shell.WriteError(Loc.GetString("cmd-econ-error-station-account-missing"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-econ-arg-player"));
        if (args.Length == 2)
            return CompletionResult.FromHint(Loc.GetString("cmd-econ-arg-amount"));
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class AddBalanceCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override string Command => "econ_addbalance";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!int.TryParse(args[1], out var amount))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-number"));
            return;
        }

        var located = await _playerLocator.LookupIdByNameOrIdAsync(args[0]);
        if (located == null)
        {
            shell.WriteError(Loc.GetString("shell-target-player-does-not-exist"));
            return;
        }

        var mindSystem = _entManager.System<SharedMindSystem>();
        var bankSystem = _entManager.System<BankSystem>();
        var economySystem = _entManager.System<EconomySystem>();

        if (!mindSystem.TryGetMind(located.UserId, out var mindId, out _))
        {
            shell.WriteError(Loc.GetString("cmd-econ-error-no-mind"));
            return;
        }

        if (!_entManager.TryGetComponent<MindBankAccountComponent>(mindId.Value, out var bankAcc))
        {
            shell.WriteError(Loc.GetString("cmd-econ-error-no-account"));
            return;
        }

        if (bankAcc.BankStation != null &&
            bankSystem.TryChangeBalance(bankAcc.BankStation.Value, bankAcc.AccountNumber, amount))
        {
            shell.WriteLine(Loc.GetString("cmd-econ-addbalance-success",
                ("player", located.Username),
                ("amount", amount)));

            economySystem.TrySendNotification(mindId.Value,
                Loc.GetString("bank-app-notification-admin-change-title"),
                Loc.GetString("bank-app-notification-admin-add-body", ("amount", amount)));
            return;
        }

        shell.WriteError(Loc.GetString("cmd-econ-error-station-account-missing"));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-econ-arg-player"));
        if (args.Length == 2)
            return CompletionResult.FromHint(Loc.GetString("cmd-econ-arg-amount-delta"));
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class PaySalaryCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;

    public override string Command => "econ_paysalary";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var multiplier = 1.0f;
        if (args.Length == 1 && !float.TryParse(args[0], out multiplier))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-number"));
            return;
        }

        var salarySystem = _sysMan.GetEntitySystem<SalarySystem>();
        salarySystem.PaySalaries(multiplier);
        shell.WriteLine(Loc.GetString("cmd-econ-paysalary-success", ("multiplier", multiplier)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHint(Loc.GetString("cmd-econ-arg-multiplier"));
        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class FineAllCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public override string Command => "econ_fineall";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out var amount) || amount <= 0)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        var bankSystem = _entManager.System<BankSystem>();
        var stationSystem = _entManager.System<StationSystem>();
        var economySystem = _entManager.System<EconomySystem>();

        var count = 0;

        var accountToMind = new Dictionary<string, EntityUid>();
        var query = _entManager.EntityQueryEnumerator<MindBankAccountComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!string.IsNullOrEmpty(comp.AccountNumber))
                accountToMind[comp.AccountNumber] = uid;
        }

        foreach (var station in stationSystem.GetStations())
        {
            if (!_entManager.TryGetComponent<StationBankComponent>(station, out var bank))
                continue;

            foreach (var accountNum in bank.Accounts.Keys)
            {
                if (bankSystem.TryChangeBalance(station, accountNum, -amount))
                {
                    count++;

                    if (accountToMind.TryGetValue(accountNum, out var mindId))
                    {
                        economySystem.TrySendNotification(mindId,
                            Loc.GetString("bank-app-notification-fine-title"),
                            Loc.GetString("bank-app-notification-fine-body", ("amount", amount)));
                    }
                }
            }
        }

        shell.WriteLine(Loc.GetString("cmd-econ-fineall-success", ("count", count), ("amount", amount)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHint(Loc.GetString("cmd-econ-arg-amount"));
        return CompletionResult.Empty;
    }
}
