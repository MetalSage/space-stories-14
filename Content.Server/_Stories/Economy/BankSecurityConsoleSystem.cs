using System.Linq;
using Content.Server._Stories.Economy.Components;
using Content.Server.Station.Systems;
using Content.Shared._Stories.Economy;
using Content.Shared._Stories.Economy.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Popups;
using Robust.Server.GameObjects;

namespace Content.Server._Stories.Economy;

public sealed class BankSecurityConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly EconomySystem _economy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        
        Subs.BuiEvents<BankSecurityConsoleComponent>(BankSecurityConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
            subs.Event<BankSecurityRefreshMessage>(OnRefresh);
            subs.Event<BankSecurityIssueFineMessage>(OnIssueFine);
        });

        SubscribeLocalEvent<BankBalanceChangedEventArgs>(OnBalanceChanged);
        SubscribeLocalEvent<BankDepartmentBalanceChangedEventArgs>(OnDeptBalanceChanged);
    }

    private void OnOpened(EntityUid uid, BankSecurityConsoleComponent component, BoundUIOpenedEvent args)
    {
        UpdateUi(uid);
    }

    private void OnRefresh(EntityUid uid, BankSecurityConsoleComponent component, BankSecurityRefreshMessage args)
    {
        UpdateUi(uid);
    }

    private void UpdateUi(EntityUid uid)
    {
        var station = _station.GetOwningStation(uid);
        if (station == null) return;

        var logs = new List<FinancialLogDto>();
        if (TryComp<StationFinancialLogComponent>(station.Value, out var logComp))
        {
            foreach (var log in logComp.Logs)
            {
                logs.Add(new FinancialLogDto
                {
                    Timestamp = log.Timestamp,
                    Source = log.Source,
                    Destination = log.Destination,
                    Amount = log.Amount,
                    Reason = log.Reason
                });
            }
        }

        var accounts = new List<AccountDto>();
        if (TryComp<StationBankComponent>(station.Value, out var bankComp))
        {
            foreach (var acc in bankComp.Accounts.Values)
                accounts.Add(new AccountDto(acc.AccountNumber, acc.OwnerName, false, acc.Balance));
        }

        _ui.SetUiState(uid, BankSecurityConsoleUiKey.Key, new BankSecurityConsoleState(logs, accounts));
    }

    private void OnIssueFine(EntityUid uid, BankSecurityConsoleComponent component, BankSecurityIssueFineMessage args)
    {
        if (args.Actor is not { Valid: true } actor) return;

        var station = _station.GetOwningStation(uid);
        if (station == null) return;

        var targetAcc = args.TargetAccount;

        if (_bank.TryChangeBalance(station.Value, targetAcc, -args.Amount))
        {
            _bank.TryChangeDepartmentBalance(station.Value, component.DestinationAccount, args.Amount);
            _bank.LogTransaction(station.Value, targetAcc, component.DestinationAccount, args.Amount, Loc.GetString("stories-bank-log-fine-reason", ("reason", args.Reason)));
            
            if (_bank.TryGetMindByAccountNumber(targetAcc, out var mindId))
            {
                _economy.TrySendNotification(mindId,
                    Loc.GetString("stories-bank-app-notification-fine-title"),
                    Loc.GetString("stories-bank-app-notification-fine-body", ("amount", args.Amount), ("reason", args.Reason)));
            }

            _popup.PopupEntity(Loc.GetString("stories-bank-security-fine-success"), uid, actor);
            UpdateUi(uid);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("stories-bank-security-error-funds"), uid, actor);
        }
    }

    private void OnBalanceChanged(BankBalanceChangedEventArgs ev)
    {
        var query = EntityQueryEnumerator<BankSecurityConsoleComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            var station = _station.GetOwningStation(uid);
            if (station == ev.Station)
            {
                UpdateUi(uid);
            }
        }
    }

    private void OnDeptBalanceChanged(BankDepartmentBalanceChangedEventArgs ev)
    {
        var query = EntityQueryEnumerator<BankSecurityConsoleComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            var station = _station.GetOwningStation(uid);
            if (station == ev.Station)
            {
                UpdateUi(uid);
            }
        }
    }
}
