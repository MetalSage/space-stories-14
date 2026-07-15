using System.Linq;
using Content.Server._Stories.Economy.Components;
using Content.Server.Station.Systems;
using Content.Shared._Stories.Economy;
using Content.Shared._Stories.Economy.Components;
using Content.Shared.Access.Systems;
using Content.Server.Cargo.Systems;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Popups;
using Content.Server.Chat.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Stories.Economy;

public sealed partial class BankCentcomConsoleSystem : EntitySystem
{
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private BankSystem _bank = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private CargoSystem _cargoSystem = default!;
    [Dependency] private EconomySystem _economy = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        
        Subs.BuiEvents<BankCentcomConsoleComponent>(BankCentcomConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
            subs.Event<CentcomRefreshMessage>(OnRefresh);
            subs.Event<CentcomChangeStationMessage>(OnChangeStation);
            subs.Event<CentcomIssueFineMessage>(OnIssueFine);
            subs.Event<CentcomSetSalaryMessage>(OnSetSalary);
            subs.Event<CentcomEditAccountMessage>(OnEditAccount);
            subs.Event<CentcomCreateAccountMessage>(OnCreateAccount);
            subs.Event<CentcomResetPinMessage>(OnResetPin);
        });

        SubscribeLocalEvent<BankBalanceChangedEventArgs>(OnBalanceChanged);
        SubscribeLocalEvent<BankDepartmentBalanceChangedEventArgs>(OnDeptBalanceChanged);
    }

    private void OnOpened(EntityUid uid, BankCentcomConsoleComponent component, BoundUIOpenedEvent args)
    {
        UpdateUi(uid);
    }

    private void OnRefresh(EntityUid uid, BankCentcomConsoleComponent component, CentcomRefreshMessage args)
    {
        UpdateUi(uid);
    }

    private void OnChangeStation(EntityUid uid, BankCentcomConsoleComponent component, CentcomChangeStationMessage args)
    {
        component.SelectedStation = GetEntity(args.Station);
        UpdateUi(uid);
    }

    private void UpdateUi(EntityUid uid)
    {
        var stations = new List<StationDto>();
        foreach (var st in _station.GetStations())
        {
            stations.Add(new StationDto(GetNetEntity(st), Name(st)));
        }

        if (!TryComp<BankCentcomConsoleComponent>(uid, out var comp)) return;

        var selectedStation = comp.SelectedStation;
        if (selectedStation == null || !Exists(selectedStation.Value) || !HasComp<StationBankComponent>(selectedStation.Value))
        {
            if (stations.Count > 0)
                selectedStation = GetEntity(stations[0].NetId);
        }

        var logs = new List<FinancialLogDto>();
        var accounts = new List<AccountDto>();
        float salMod = 1.0f;
        float salFreq = 30f;

        if (selectedStation != null && Exists(selectedStation.Value))
        {
            if (TryComp<StationFinancialLogComponent>(selectedStation.Value, out var logComp))
            {
                foreach (var log in logComp.Logs)
                    logs.Add(new FinancialLogDto { Timestamp = log.Timestamp, Source = log.Source, Destination = log.Destination, Amount = log.Amount, Reason = log.Reason });
            }

            if (TryComp<StationBankComponent>(selectedStation.Value, out var bankComp))
            {
                foreach (var acc in bankComp.Accounts.Values)
                    accounts.Add(new AccountDto(acc.AccountNumber, acc.OwnerName, false, acc.Balance));
                salMod = bankComp.SalaryModifier;
                salFreq = bankComp.SalaryFrequencyMins;
            }

            var deptAccounts = _cargoSystem.GetAccounts(selectedStation.Value);
            foreach (var acc in deptAccounts)
            {
                string name = acc.Key;
                if (_prototypeManager.TryIndex<CargoAccountPrototype>(acc.Key, out var cargoAcc))
                    name = Loc.GetString(cargoAcc.Name);
                accounts.Add(new AccountDto(acc.Key, name, true, acc.Value));
            }
        }

        _ui.SetUiState(uid, BankCentcomConsoleUiKey.Key, new BankCentcomConsoleState(stations, selectedStation != null ? GetNetEntity(selectedStation.Value) : null, logs, accounts, salMod, salFreq));
    }

    private void OnCreateAccount(EntityUid uid, BankCentcomConsoleComponent component, CentcomCreateAccountMessage args)
    {
        if (args.Actor is not { Valid: true } actor) return;
        var station = GetEntity(args.Station);
        
        if (TryComp<StationBankComponent>(station, out var bank))
        {
            var accNum = _bank.GenerateAccountNumber(bank);
            var pin = _bank.GeneratePin();
            var account = new BankAccount
            {
                AccountNumber = accNum,
                Pin = pin,
                Balance = args.StartingBalance,
                OwnerName = args.OwnerName
            };
            bank.Accounts.Add(accNum, account);
            Dirty(station, bank);

            _popup.PopupEntity(Loc.GetString("stories-bank-centcom-account-created-popup", ("name", args.OwnerName), ("number", accNum), ("pin", pin)), uid, actor);
            UpdateUi(uid);
        }
    }

    private void OnResetPin(EntityUid uid, BankCentcomConsoleComponent component, CentcomResetPinMessage args)
    {
        if (args.Actor is not { Valid: true } actor) return;
        var station = GetEntity(args.Station);

        if (TryComp<StationBankComponent>(station, out var bank) && bank.Accounts.TryGetValue(args.TargetId, out var acc))
        {
            acc.Pin = _bank.GeneratePin();
            Dirty(station, bank);

            _popup.PopupEntity(Loc.GetString("stories-bank-centcom-pin-reset-popup", ("number", acc.AccountNumber), ("pin", acc.Pin)), uid, actor);
        }
    }

    private void OnIssueFine(EntityUid uid, BankCentcomConsoleComponent component, CentcomIssueFineMessage args)
    {
        if (args.Actor is not { Valid: true } actor) return;
        var station = GetEntity(args.Station);
        if (!Exists(station)) return;

        var centComName = Loc.GetString("stories-bank-centcom-name");
        int totalFined = 0;
        string targetAnnounceName = Loc.GetString("stories-bank-centcom-target-all-name");

        void ApplyFine(string accId, bool isDepartment)
        {
            int fineAmount = args.Amount;

            if (args.IsPercentage)
            {
                if (isDepartment)
                {
                    var proto = new ProtoId<CargoAccountPrototype>(accId);
                    if (_cargoSystem.TryGetAccount(station, proto, out var currentBal))
                        fineAmount = (int)(currentBal * (args.Amount / 100f));
                }
                else
                {
                    if (_bank.TryGetAccount(station, accId, out var acc))
                        fineAmount = (int)(acc.Balance * (args.Amount / 100f));
                }
            }

            if (fineAmount <= 0) return;

            var logReason = Loc.GetString("stories-bank-log-fine-reason", ("reason", args.Reason));

            if (isDepartment)
            {
                if (_bank.TryChangeDepartmentBalance(station, accId, -fineAmount, true))
                {
                    _bank.LogTransaction(station, accId, centComName, fineAmount, logReason);
                    totalFined++;
                }
            }
            else
            {
                if (_bank.TryChangeBalance(station, accId, -fineAmount, true))
                {
                    _bank.LogTransaction(station, accId, centComName, fineAmount, logReason);
                    totalFined++;

                    if (args.SendNotification && _bank.TryGetMindByAccountNumber(accId, out var mindId))
                    {
                        if (args.IsPercentage)
                            _economy.TrySendNotification(mindId, Loc.GetString("stories-bank-app-notification-fine-title"), Loc.GetString("stories-bank-app-notification-fine-percent-body", ("percent", args.Amount), ("amount", fineAmount), ("reason", args.Reason)));
                        else
                            _economy.TrySendNotification(mindId, Loc.GetString("stories-bank-app-notification-fine-title"), Loc.GetString("stories-bank-app-notification-fine-body", ("amount", fineAmount), ("reason", args.Reason)));
                    }
                }
            }
        }

        if (args.TargetType == CentcomFineTarget.Crew)
        {
            ApplyFine(args.TargetId, false);
            targetAnnounceName = _bank.TryGetAccount(station, args.TargetId, out var acc) ? acc.OwnerName : args.TargetId;
        }
        else if (args.TargetType == CentcomFineTarget.Department)
        {
            ApplyFine(args.TargetId, true);
            targetAnnounceName = _prototypeManager.TryIndex<CargoAccountPrototype>(args.TargetId, out var cargoAcc) ? Loc.GetString(cargoAcc.Name) : args.TargetId;
        }
        else if (args.TargetType == CentcomFineTarget.AllCrew || args.TargetType == CentcomFineTarget.All)
        {
            if (TryComp<StationBankComponent>(station, out var bankComp))
            {
                var keys = bankComp.Accounts.Keys.ToList();
                foreach (var acc in keys) ApplyFine(acc, false);
            }
            if (args.TargetType == CentcomFineTarget.AllCrew)
                targetAnnounceName = Loc.GetString("stories-bank-centcom-target-allcrew-name");
        }
        
        if (args.TargetType == CentcomFineTarget.AllDepartments || args.TargetType == CentcomFineTarget.All)
        {
            var deptAccounts = _cargoSystem.GetAccounts(station);
            var keys = deptAccounts.Keys.ToList();
            foreach (var acc in keys) ApplyFine(acc, true);
            if (args.TargetType == CentcomFineTarget.AllDepartments)
                targetAnnounceName = Loc.GetString("stories-bank-centcom-target-alldepts-name");
        }

        if (args.AnnounceToStation && totalFined > 0)
        {
            string announcementBody = args.CustomAnnouncement ?? Loc.GetString("stories-bank-centcom-announcement-fine-complex");
            
            string fineValueStr = args.IsPercentage ? $"{args.Amount}%" : $"{args.Amount} кр.";
            
            announcementBody = announcementBody.Replace("{target}", targetAnnounceName);
            announcementBody = announcementBody.Replace("{amount}", fineValueStr);
            announcementBody = announcementBody.Replace("{reason}", args.Reason);

            _chat.DispatchStationAnnouncement(station, announcementBody, centComName, true, null, Color.Gold);
        }

        _popup.PopupEntity(Loc.GetString("stories-bank-centcom-fine-success"), uid, actor);
        UpdateUi(uid);
    }

    private void OnSetSalary(EntityUid uid, BankCentcomConsoleComponent component, CentcomSetSalaryMessage args)
    {
        if (args.Actor is not { Valid: true } actor) return;
        var station = GetEntity(args.Station);
        if (TryComp<StationBankComponent>(station, out var bank))
        {
            bank.SalaryModifier = args.Modifier;
            bank.SalaryFrequencyMins = args.FrequencyMins;
            bank.NextPayday = _timing.CurTime + TimeSpan.FromMinutes(args.FrequencyMins);
            Dirty(station, bank);
            _popup.PopupEntity(Loc.GetString("stories-bank-centcom-salary-success"), uid, actor);
            UpdateUi(uid);
        }
    }

    private void OnEditAccount(EntityUid uid, BankCentcomConsoleComponent component, CentcomEditAccountMessage args)
    {
        if (args.Actor is not { Valid: true } actor) return;
        var station = GetEntity(args.Station);

        if (args.Delete)
        {
            if (!args.IsDepartment && TryComp<StationBankComponent>(station, out var bank) && bank.Accounts.Remove(args.TargetId))
            {
                Dirty(station, bank);
                if (_bank.TryGetMindByAccountNumber(args.TargetId, out var mindId))
                    _economy.TrySendNotification(mindId, Loc.GetString("stories-bank-app-notification-admin-change-title"), Loc.GetString("stories-bank-app-notification-account-deleted"));
                
                _popup.PopupEntity(Loc.GetString("stories-bank-centcom-edit-success"), uid, actor);
                UpdateUi(uid);
                return;
            }
        }
        else
        {
            if (args.IsDepartment)
            {
                var proto = new ProtoId<CargoAccountPrototype>(args.TargetId);
                if (_cargoSystem.TrySetBankAccount(station, proto, args.NewBalance, false, true))
                {
                    _popup.PopupEntity(Loc.GetString("stories-bank-centcom-edit-success"), uid, actor);
                    UpdateUi(uid);
                    return;
                }
            }
            else
            {
                if (TryComp<StationBankComponent>(station, out var bank) && bank.Accounts.ContainsKey(args.TargetId))
                {
                    int oldBal = bank.Accounts[args.TargetId].Balance;
                    bank.Accounts[args.TargetId].Balance = args.NewBalance;
                    Dirty(station, bank);
                    
                    if (_bank.TryGetMindByAccountNumber(args.TargetId, out var mindId))
                        _economy.TrySendNotification(mindId, Loc.GetString("stories-bank-app-notification-admin-change-title"), Loc.GetString("stories-bank-app-notification-admin-change-body", ("old", oldBal), ("new", args.NewBalance)));

                    _popup.PopupEntity(Loc.GetString("stories-bank-centcom-edit-success"), uid, actor);
                    UpdateUi(uid);
                    return;
                }
            }
        }
        _popup.PopupEntity(Loc.GetString("stories-bank-security-error-funds"), uid, actor);
    }

    private void OnBalanceChanged(BankBalanceChangedEventArgs ev)
    {
        var query = EntityQueryEnumerator<BankCentcomConsoleComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            UpdateUi(uid);
        }
    }

    private void OnDeptBalanceChanged(BankDepartmentBalanceChangedEventArgs ev)
    {
        var query = EntityQueryEnumerator<BankCentcomConsoleComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            UpdateUi(uid);
        }
    }
}
