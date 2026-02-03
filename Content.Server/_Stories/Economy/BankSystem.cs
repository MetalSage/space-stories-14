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
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Stories.Economy;

public sealed class BankSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        var entity = args.Mob;
        var station = _station.GetOwningStation(entity);

        if (station == null)
            return;

        var bank = EnsureComp<StationBankComponent>(station.Value);
        var balance = _random.Next(
            args.JobId != null && _prototypeManager.TryIndex<JobPrototype>(args.JobId, out var jobProto)
                ? jobProto.MinBankBalance
                : 100,
            args.JobId != null && _prototypeManager.TryIndex(args.JobId, out jobProto)
                ? jobProto.MaxBankBalance + 1
                : 501);

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
            mindBank.BankStation = station.Value;

            var invSystem = EntityManager.System<ServerInventorySystem>();
            if (invSystem.TryGetSlotEntity(entity, "id", out var idEntity))
                AttachBankToId(mindContainer.Mind.Value, idEntity.Value, mindBank);
        }
    }

    public void AttachBankToId(EntityUid mindId, EntityUid id, MindBankAccountComponent? mindBank = null)
    {
        if (!Resolve(mindId, ref mindBank))
            return;

        if (mindBank.LinkedIdCard.HasValue && Exists(mindBank.LinkedIdCard.Value))
            DetachBankFromId(mindBank.LinkedIdCard.Value);

        if (TryComp<PdaComponent>(id, out var pda))
        {
            if (pda.ContainedId != null)
            {
                var comp = EnsureComp<IdBankAccountComponent>(pda.ContainedId.Value);
                comp.AccountNumber = mindBank.AccountNumber;
                mindBank.LinkedIdCard = pda.ContainedId;
                Dirty(pda.ContainedId.Value, comp);
            }
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

    private string GenerateAccountNumber(StationBankComponent bank)
    {
        string number;
        do
        {
            number = _random.Next(10000000, 99999999).ToString();
        } while (bank.Accounts.ContainsKey(number));

        return number;
    }

    private string GeneratePin()
    {
        return _random.Next(1000, 9999).ToString("D4");
    }

    public bool TryGetAccount(EntityUid stationUid, string accountNumber, [NotNullWhen(true)] out BankAccount? account)
    {
        account = null;
        if (!TryComp<StationBankComponent>(stationUid, out var bank))
            return false;
        return bank.Accounts.TryGetValue(accountNumber, out account);
    }

    public bool TryChangeBalance(EntityUid stationUid, string accountNumber, int amount)
    {
        if (!TryGetAccount(stationUid, accountNumber, out var account))
            return false;

        if (account.Balance + amount < 0)
            return false;

        account.Balance += amount;
        return true;
    }

    public bool TryTransfer(EntityUid stationUid, string fromAcc, string toAcc, int amount)
    {
        if (amount <= 0)
            return false;
        if (!TryGetAccount(stationUid, fromAcc, out var sender))
            return false;
        if (!TryGetAccount(stationUid, toAcc, out var receiver))
            return false;

        if (sender.Balance < amount)
            return false;

        sender.Balance -= amount;
        receiver.Balance += amount;
        return true;
    }
}
