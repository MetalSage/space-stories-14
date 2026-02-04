using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Economy;

[Serializable] [NetSerializable]
public enum AtmUiKey
{
    Key,
}

[Serializable] [NetSerializable]
public sealed class AtmBoundUserInterfaceState : BoundUserInterfaceState
{
    public string AccountNumber = string.Empty;
    public int Balance;
    public bool IsLoggedIn;
    public string Message = string.Empty;
    public string OwnerName = string.Empty;

    public AtmBoundUserInterfaceState(bool isLoggedIn,
        string accountNumber,
        int balance,
        string message,
        string ownerName)
    {
        IsLoggedIn = isLoggedIn;
        AccountNumber = accountNumber;
        Balance = balance;
        Message = message;
        OwnerName = ownerName;
    }
}

[Serializable] [NetSerializable]
public sealed class AtmLoginMessage : BoundUserInterfaceMessage
{
    public string AccountNumber;
    public string Pin;

    public AtmLoginMessage(string accountNumber, string pin)
    {
        AccountNumber = accountNumber;
        Pin = pin;
    }
}

[Serializable] [NetSerializable]
public sealed class AtmWithdrawMessage : BoundUserInterfaceMessage
{
    public int Amount;
    public AtmWithdrawMessage(int amount) { Amount = amount; }
}

[Serializable] [NetSerializable]
public sealed class AtmLogoutMessage : BoundUserInterfaceMessage
{
}
