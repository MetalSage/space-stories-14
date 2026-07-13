using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Economy;

[Serializable, NetSerializable]
public enum BankSecurityConsoleUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class BankSecurityConsoleState : BoundUserInterfaceState
{
    public readonly List<FinancialLogDto> Logs;
    public readonly List<AccountDto> KnownAccounts;

    public BankSecurityConsoleState(List<FinancialLogDto> logs, List<AccountDto> knownAccounts)
    {
        Logs = logs;
        KnownAccounts = knownAccounts;
    }
}

[Serializable, NetSerializable]
public struct FinancialLogDto
{
    public TimeSpan Timestamp;
    public string Source;
    public string Destination;
    public int Amount;
    public string Reason;
}

[Serializable, NetSerializable]
public struct AccountDto
{
    public string Id;
    public string DisplayName;
    public bool IsDepartment;
    public int Balance;

    public AccountDto(string id, string displayName, bool isDepartment, int balance)
    {
        Id = id;
        DisplayName = displayName;
        IsDepartment = isDepartment;
        Balance = balance;
    }
}

[Serializable, NetSerializable]
public sealed class BankSecurityIssueFineMessage : BoundUserInterfaceMessage
{
    public readonly string TargetAccount;
    public readonly int Amount;
    public readonly string Reason;

    public BankSecurityIssueFineMessage(string targetAccount, int amount, string reason)
    {
        TargetAccount = targetAccount;
        Amount = amount;
        Reason = reason;
    }
}

[Serializable, NetSerializable]
public sealed class BankSecurityRefreshMessage : BoundUserInterfaceMessage
{
}
