using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Economy;

[Serializable] [NetSerializable]
public enum BankUiKey
{
    Key,
}

[Serializable] [NetSerializable]
public sealed class BankCartridgeUiState : BoundUserInterfaceState
{
    public string AccountNumber;
    public int Balance;
    public bool IsIdLinked;
    public bool NotificationsEnabled;
    public string OwnerName;

    public BankCartridgeUiState(string accountNumber,
        string ownerName,
        int balance,
        bool isIdLinked,
        bool notificationsEnabled)
    {
        AccountNumber = accountNumber;
        OwnerName = ownerName;
        Balance = balance;
        IsIdLinked = isIdLinked;
        NotificationsEnabled = notificationsEnabled;
    }
}

[Serializable] [NetSerializable]
public sealed class BankTransferMessage : CartridgeMessageEvent
{
    public int Amount;
    public string TargetAccount;

    public BankTransferMessage(string targetAccount, int amount)
    {
        TargetAccount = targetAccount;
        Amount = amount;
    }
}

[Serializable] [NetSerializable]
public sealed class BankLinkIdMessage : CartridgeMessageEvent
{
}

[Serializable] [NetSerializable]
public sealed class BankUnlinkIdMessage : CartridgeMessageEvent
{
}

[Serializable] [NetSerializable]
public sealed class BankToggleNotificationsMessage : CartridgeMessageEvent
{
}
