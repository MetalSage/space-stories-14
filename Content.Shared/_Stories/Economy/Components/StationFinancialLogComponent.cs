using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Economy.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StationFinancialLogComponent : Component
{
    [DataField]
    public List<FinancialLogEntry> Logs = new();
}

[DataDefinition]
public sealed partial class FinancialLogEntry
{
    [DataField]
    public TimeSpan Timestamp;

    [DataField]
    public string Source = string.Empty;

    [DataField]
    public string Destination = string.Empty;

    [DataField]
    public int Amount;

    [DataField]
    public string Reason = string.Empty;
}
