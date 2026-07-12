namespace Content.Server._Stories.Economy.Components;

[RegisterComponent]
public sealed partial class BankSecurityConsoleComponent : Component
{
    [DataField]
    public string DestinationAccount = "Security";
}
