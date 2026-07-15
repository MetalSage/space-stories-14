namespace Content.Server._Stories.Economy.Components;

[RegisterComponent]
public sealed partial class BankCentcomConsoleComponent : Component
{
    [ViewVariables]
    public EntityUid? SelectedStation;
}
