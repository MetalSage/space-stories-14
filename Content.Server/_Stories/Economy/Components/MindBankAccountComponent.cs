namespace Content.Server._Stories.Economy.Components;

[RegisterComponent]
public sealed partial class MindBankAccountComponent : Component
{
    [DataField]
    public string AccountNumber = string.Empty;

    [DataField]
    public EntityUid? BankStation;

    [DataField]
    public EntityUid? LinkedIdCard;

    [DataField]
    public bool NotificationsEnabled = true;

    [DataField]
    public string Pin = string.Empty;
}
