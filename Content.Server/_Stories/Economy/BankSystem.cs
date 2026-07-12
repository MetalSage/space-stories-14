using System.Diagnostics.CodeAnalysis;
using Content.Server._Stories.Economy.Components;
using Content.Server.Inventory;
using Content.Server.Station.Systems;
using Content.Shared._Stories.Economy.Components;
using Content.Shared.Access.Components;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.PDA;
using Content.Shared.Roles;
using Content.Server.Cargo.Systems;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Station.Components;
using Content.Shared._Stories.SCCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Stories.Economy;

public sealed partial class BankSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CargoSystem _cargoSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
        SubscribeLocalEvent<StationBankComponent, MapInitEvent>(OnBankInit);
    }

    private void OnBankInit(EntityUid uid, StationBankComponent component, MapInitEvent args)
    {
        component.SalaryFrequencyMins = _cfg.GetCVar(SCCVars.EconomySalaryFrequency);
        component.NextPayday = _timing.CurTime + TimeSpan.FromMinutes(component.SalaryFrequencyMins);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        var entity = args.Mob;
        EntityUid? station = args.Station;

        if (station == EntityUid.Invalid)
            station = _station.GetOwningStation(entity);

        if (station == null || station == EntityUid.Invalid)
            return;

        var stationUid = station.Value;
        var bank = EnsureComp<StationBankComponent>(stationUid);
        
        var balance = _random.Next(
            args.JobId != null && _prototypeManager.TryIndex<JobPrototype>(args.JobId, out var jobProto) ? jobProto.MinBankBalance : 100,
            args.JobId != null && _prototypeManager.TryIndex(args.JobId, out jobProto) ? jobProto.MaxBankBalance + 1 : 501);

        if (TryComp<MindContainerComponent>(entity, out var mindContainer) &&
            mindContainer.Mind.HasValue &&
            TryComp(mindContainer.Mind.Value, out MindComponent? mind))
        {
            if (HasComp<MindBankAccountComponent>(mindContainer.Mind.Value))
                return;

            var accountNumber = GenerateAccountNumber(bank);
            var pin = GeneratePin();

            var account = new BankAccount
            {
                AccountNumber = accountNumber,
                Pin = pin,
                Balance = balance,
                OwnerName = args.Profile.Name,
            };

            bank.Accounts.Add(accountNumber, account);

            var mindBank = EnsureComp<MindBankAccountComponent>(mindContainer.Mind.Value);
            mindBank.AccountNumber = accountNumber;
            mindBank.Pin = pin;
            mindBank.BankStation = stationUid;

            var invSystem = EntityManager.System<ServerInventorySystem>();
            if (invSystem.TryGetSlotEntity(entity, "id", out var idEntity))
                AttachBankToId(mindContainer.Mind.Value, idEntity.Value, mindBank);
        }
    }

    public void AttachBankToId(EntityUid mindId, EntityUid id, MindBankAccountComponent? mindBank = null)
    {
        if (!Resolve(mindId, ref mindBank)) return;

        if (mindBank.LinkedIdCard.HasValue && Exists(mindBank.LinkedIdCard.Value))
            DetachBankFromId(mindBank.LinkedIdCard.Value);

        if (TryComp<PdaComponent>(id, out var pda) && pda.ContainedId != null)
        {
            var comp = EnsureComp<IdBankAccountComponent>(pda.ContainedId.Value);
            comp.AccountNumber = mindBank.AccountNumber;
            mindBank.LinkedIdCard = pda.ContainedId;
            Dirty(pda.ContainedId.Value, comp);
        }
        else if (HasComp<IdCardComponent>(id))
        {
            var comp = EnsureComp<IdBankAccountComponent>(id);
            comp.AccountNumber = mindBank.AccountNumber;
            mindBank.LinkedIdCard = id;
            Dirty(id, comp);
        }
    }

    public void DetachBankFromId(EntityUid id)
    {
        RemComp<IdBankAccountComponent>(id);
    }

    public string GenerateAccountNumber(StationBankComponent bank)
    {
        string number;
        do { number = _random.Next(10000000, 99999999).ToString(); } while (bank.Accounts.ContainsKey(number));
        return number;
    }

    public string GeneratePin()
    {
        return _random.Next(1000, 9999).ToString("D4");
    }

    public bool TryGetAccount(EntityUid stationUid, string accountNumber, [NotNullWhen(true)] out BankAccount? account)
    {
        account = null;
        if (!TryComp<StationBankComponent>(stationUid, out var bank)) return false;
        return bank.Accounts.TryGetValue(accountNumber, out account);
    }
    
    public bool TryGetMindByAccountNumber(string accountNumber, out EntityUid mindId)
    {
        mindId = EntityUid.Invalid;
        var query = EntityQueryEnumerator<MindBankAccountComponent>();
        while (query.MoveNext(out var uid, out var bankComp))
        {
            if (bankComp.AccountNumber == accountNumber)
            {
                mindId = uid;
                return true;
            }
        }
        return false;
    }

    public bool TryChangeBalance(EntityUid stationUid, string accountNumber, int amount, bool force = false)
    {
        if (!TryGetAccount(stationUid, accountNumber, out var account)) return false;
        if (!force && account.Balance + amount < 0) return false;

        account.Balance = Math.Max(0, account.Balance + amount);
        RaiseLocalEvent(new BankBalanceChangedEventArgs(stationUid, accountNumber));
        return true;
    }

    public bool TryTransfer(EntityUid stationUid, string fromAcc, string toAcc, int amount)
    {
        if (amount <= 0) return false;
        if (!TryGetAccount(stationUid, fromAcc, out var sender)) return false;
        if (!TryGetAccount(stationUid, toAcc, out var receiver)) return false;
        if (sender.Balance < amount) return false;

        sender.Balance -= amount;
        receiver.Balance += amount;

        LogTransaction(stationUid, fromAcc, toAcc, amount, Loc.GetString("stories-bank-log-transfer"));
        RaiseLocalEvent(new BankBalanceChangedEventArgs(stationUid, fromAcc));
        RaiseLocalEvent(new BankBalanceChangedEventArgs(stationUid, toAcc));
        return true;
    }

    public bool TryChangeDepartmentBalance(EntityUid stationUid, string departmentId, int amount, bool force = false)
    {
        var protoId = new ProtoId<CargoAccountPrototype>(departmentId);
        if (!_cargoSystem.TryGetAccount(stationUid, protoId, out var currentBalance)) return false;
        if (!force && currentBalance + amount < 0) return false;

        var addAmount = amount;
        if (force && currentBalance + amount < 0)
            addAmount = -currentBalance;

        var result = _cargoSystem.TryAdjustBankAccount(stationUid, protoId, addAmount);
        if (result)
        {
            RaiseLocalEvent(new BankDepartmentBalanceChangedEventArgs(stationUid, departmentId));
        }
        return result;
    }

    public int GetDepartmentBalance(EntityUid stationUid, string departmentId)
    {
        var protoId = new ProtoId<CargoAccountPrototype>(departmentId);
        if (_cargoSystem.TryGetAccount(stationUid, protoId, out var balance)) return balance;
        return 0;
    }

    public void LogTransaction(EntityUid stationUid, string source, string dest, int amount, string reason)
    {
        var logComp = EnsureComp<StationFinancialLogComponent>(stationUid);
        logComp.Logs.Add(new FinancialLogEntry
        {
            Timestamp = _timing.CurTime,
            Source = source,
            Destination = dest,
            Amount = amount,
            Reason = reason
        });
        Dirty(stationUid, logComp);
    }
}

public sealed class BankBalanceChangedEventArgs : EntityEventArgs
{
    public EntityUid Station { get; }
    public string AccountNumber { get; }

    public BankBalanceChangedEventArgs(EntityUid station, string accountNumber)
    {
        Station = station;
        AccountNumber = accountNumber;
    }
}

public sealed class BankDepartmentBalanceChangedEventArgs : EntityEventArgs
{
    public EntityUid Station { get; }
    public string DepartmentId { get; }

    public BankDepartmentBalanceChangedEventArgs(EntityUid station, string departmentId)
    {
        Station = station;
        DepartmentId = departmentId;
    }
}
