using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Economy.Components;

[RegisterComponent] [NetworkedComponent]
public sealed partial class IdBankAccountComponent : Component
{
    [DataField]
    public string AccountNumber = string.Empty;
}
