using Content.Server._Stories.Economy.Components;
using Content.Server.Station.Systems;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared._Stories.SCCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Stories.Economy;

public sealed partial class SalarySystem : EntitySystem
{
    [Dependency] private BankSystem _bank = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private EconomySystem _economy = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedRoleSystem _roleSystem = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StationBankComponent>();
        while (query.MoveNext(out var uid, out var bank))
        {
            if (bank.SalaryFrequencyMins <= 0)
                continue;

            if (bank.NextPayday == TimeSpan.Zero)
            {
                bank.NextPayday = _timing.CurTime + TimeSpan.FromMinutes(bank.SalaryFrequencyMins);
                continue;
            }

            if (_timing.CurTime >= bank.NextPayday)
            {
                PaySalaries(uid, bank);
                bank.NextPayday = _timing.CurTime + TimeSpan.FromMinutes(bank.SalaryFrequencyMins);
            }
        }
    }

    public void PaySalaries(EntityUid stationUid, StationBankComponent stationBank, float multiplier = 1.0f)
    {
        var percentage = _cfg.GetCVar(SCCVars.EconomySalaryPercentage);

        var query = EntityQueryEnumerator<MindBankAccountComponent, MindComponent>();
        while (query.MoveNext(out var uid, out var bankComp, out var mind))
        {
            if (mind.OwnedEntity == null || bankComp.BankStation != stationUid)
                continue;

            var roles = _roleSystem.MindGetAllRoleInfo((uid, mind));
            string? jobPrototypeId = null;

            foreach (var role in roles)
            {
                if (!string.IsNullOrEmpty(role.Prototype) && _prototypeManager.HasIndex<JobPrototype>(role.Prototype))
                {
                    jobPrototypeId = role.Prototype;
                    break;
                }
            }

            if (jobPrototypeId == null)
                continue;

            if (!_prototypeManager.TryIndex<JobPrototype>(jobPrototypeId, out var jobProto))
                continue;

            if (!stationBank.Accounts.ContainsKey(bankComp.AccountNumber))
                continue;

            var baseSalary = _random.Next(jobProto.MinBankBalance, jobProto.MaxBankBalance + 1);
            var actualSalary = (int)(baseSalary * percentage * stationBank.SalaryModifier * multiplier);

            if (actualSalary > 0)
            {
                _bank.TryChangeBalance(stationUid, bankComp.AccountNumber, actualSalary);
                _economy.TrySendNotification(uid,
                    Loc.GetString("stories-bank-app-notification-salary-title"),
                    Loc.GetString("stories-bank-app-notification-salary-body", ("amount", actualSalary)));
            }
        }
    }

    public void PaySalaries(float multiplier = 1.0f)
    {
        var query = EntityQueryEnumerator<StationBankComponent>();
        while (query.MoveNext(out var uid, out var bank))
        {
            PaySalaries(uid, bank, multiplier);
        }
    }
}
