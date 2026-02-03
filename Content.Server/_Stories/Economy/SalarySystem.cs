using Content.Server._Stories.Economy.Components;
using Content.Server.Station.Systems;
using Content.Shared._Stories.SCCVars;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Stories.Economy;

public sealed class SalarySystem : EntitySystem
{
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly EconomySystem _economy = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private TimeSpan _nextPayday;

    public override void Initialize()
    {
        base.Initialize();
        var freq = _cfg.GetCVar(SCCVars.EconomySalaryFrequency);
        _nextPayday = _timing.CurTime + TimeSpan.FromMinutes(freq);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var freq = _cfg.GetCVar(SCCVars.EconomySalaryFrequency);
        if (freq <= 0)
            return;

        if (_timing.CurTime < _nextPayday)
            return;

        PaySalaries();
        _nextPayday = _timing.CurTime + TimeSpan.FromMinutes(freq);
    }

    public void PaySalaries(float multiplier = 1.0f)
    {
        var percentage = _cfg.GetCVar(SCCVars.EconomySalaryPercentage);

        var query = EntityQueryEnumerator<MindBankAccountComponent, MindComponent>();
        while (query.MoveNext(out var uid, out var bankComp, out var mind))
        {
            if (mind.OwnedEntity == null)
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

            var stationUid = _station.GetOwningStation(mind.OwnedEntity.Value);
            if (!stationUid.HasValue || !TryComp<StationBankComponent>(stationUid, out var stationBank))
                continue;

            if (!stationBank.Accounts.ContainsKey(bankComp.AccountNumber))
                continue;

            var baseSalary = _random.Next(jobProto.MinBankBalance, jobProto.MaxBankBalance + 1);
            var actualSalary = (int)(baseSalary * percentage * stationBank.SalaryModifier * multiplier);

            if (actualSalary > 0)
            {
                _bank.TryChangeBalance(stationUid.Value, bankComp.AccountNumber, actualSalary);
                _economy.TrySendNotification(uid,
                    Loc.GetString("bank-app-notification-salary-title"),
                    Loc.GetString("bank-app-notification-salary-body", ("amount", actualSalary)));
            }
        }
    }
}
