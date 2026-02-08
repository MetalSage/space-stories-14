namespace Content.Server._Stories.Economy.Components;

[RegisterComponent]
public sealed partial class StationBankComponent : Component
{
    [DataField]
    public Dictionary<string, BankAccount> Accounts = new();

    [DataField] [ViewVariables(VVAccess.ReadWrite)]
    public float SalaryModifier = 1.0f;
}

[DataDefinition]
public sealed partial class BankAccount
{
    [DataField]
    public string AccountNumber = string.Empty;

    [DataField]
    public int Balance;

    [DataField]
    public string OwnerName = string.Empty;

    [DataField]
    public string Pin = string.Empty;
}
