using Content.Server._Stories.Economy.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;

namespace Content.Server._Stories.Economy.Events;

[RegisterComponent] [Access(typeof(BankingErrorRule))]
public sealed partial class BankingErrorRuleComponent : Component
{
    [DataField] public float LossPercentageMax = 0.20f;
    [DataField] public float LossPercentageMin = 0.05f;
}

[RegisterComponent] [Access(typeof(SalaryModifierRule))]
public sealed partial class SalaryModifierRuleComponent : Component
{
    [DataField] public float ModifierMax = 0.5f;
    [DataField] public float ModifierMin = -0.5f;
}

[RegisterComponent] [Access(typeof(BankHackRule))]
public sealed partial class BankHackRuleComponent : Component
{
}

public sealed class BankingErrorRule : StationEventSystem<BankingErrorRuleComponent>
{
    [Dependency] private readonly EconomySystem _economy = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    protected override void Started(EntityUid uid,
        BankingErrorRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var station) || !TryComp<StationBankComponent>(station, out var bank))
            return;

        foreach (var (accNum, account) in bank.Accounts)
        {
            if (account.Balance <= 0)
                continue;

            var pct = RobustRandom.NextFloat(component.LossPercentageMin, component.LossPercentageMax);
            var loss = (int)(account.Balance * pct);

            if (loss > 0)
            {
                account.Balance -= loss;

                var query = EntityQueryEnumerator<MindComponent, MindBankAccountComponent>();
                while (query.MoveNext(out var mindId, out _, out var bankAccount))
                {
                    if (bankAccount.AccountNumber == accNum)
                    {
                        _economy.TrySendNotification(mindId,
                            Loc.GetString("bank-app-notification-error-title"),
                            Loc.GetString("bank-app-notification-error-body", ("amount", loss)));
                        break;
                    }
                }
            }
        }
    }
}

public sealed class SalaryModifierRule : StationEventSystem<SalaryModifierRuleComponent>
{
    private float _appliedModifier;

    protected override void Started(EntityUid uid,
        SalaryModifierRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var station) || !TryComp<StationBankComponent>(station, out var bank))
            return;

        var mod = RobustRandom.NextFloat(component.ModifierMin, component.ModifierMax);
        _appliedModifier = mod;
        bank.SalaryModifier += mod;

        if (bank.SalaryModifier < 0)
            bank.SalaryModifier = 0;
    }

    protected override void Ended(EntityUid uid,
        SalaryModifierRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);
        if (!TryGetRandomStation(out var station) || !TryComp<StationBankComponent>(station, out var bank))
            return;

        bank.SalaryModifier -= _appliedModifier;
        if (bank.SalaryModifier < 0)
            bank.SalaryModifier = 0;
    }
}

public sealed class BankHackRule : StationEventSystem<BankHackRuleComponent>
{
    protected override void Started(EntityUid uid,
        BankHackRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var station) || !TryComp<StationBankComponent>(station, out var bank))
            return;

        foreach (var account in bank.Accounts.Values)
        {
            account.Balance = 0;
        }
    }
}
