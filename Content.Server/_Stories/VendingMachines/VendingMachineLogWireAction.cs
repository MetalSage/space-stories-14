using Content.Server.Wires;
using Content.Shared.VendingMachines;
using Content.Shared.Wires;

namespace Content.Server.VendingMachines;

[DataDefinition]
public sealed partial class VendingMachineLogWireAction : BaseToggleWireAction
{
    public override Color Color { get; set; } = Color.Blue;
    public override string Name { get; set; } = "stories-wire-name-vending-log";
    public override object? StatusKey { get; } = LogWireKey.StatusKey;
    public override object? TimeoutKey { get; } = LogWireKey.TimeoutKey;

    public override StatusLightState? GetLightState(Wire wire)
    {
        if (EntityManager.TryGetComponent(wire.Owner, out VendingMachineComponent? vending))
            return vending.DisableFinancialLogging ? StatusLightState.Off : StatusLightState.On;

        return StatusLightState.Off;
    }

    public override void ToggleValue(EntityUid owner, bool setting)
    {
        if (EntityManager.TryGetComponent(owner, out VendingMachineComponent? vending))
        {
            vending.DisableFinancialLogging = !setting;
            EntityManager.Dirty(owner, vending);
        }
    }

    public override bool GetValue(EntityUid owner)
    {
        return EntityManager.TryGetComponent(owner, out VendingMachineComponent? vending) && !vending.DisableFinancialLogging;
    }
}
