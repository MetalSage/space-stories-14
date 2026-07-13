using Content.Server.Wires;
using Content.Shared.VendingMachines;
using Content.Shared.Wires;

namespace Content.Server.VendingMachines;

[DataDefinition]
public sealed partial class VendingMachineFreeWireAction : BaseToggleWireAction
{
    public override Color Color { get; set; } = Color.Gold;
    public override string Name { get; set; } = "stories-wire-name-vending-free";
    public override object? StatusKey { get; } = FreeWireKey.StatusKey;
    public override object? TimeoutKey { get; } = FreeWireKey.TimeoutKey;

    public override StatusLightState? GetLightState(Wire wire)
    {
        if (EntityManager.TryGetComponent(wire.Owner, out VendingMachineComponent? vending))
            return vending.FreeVend ? StatusLightState.BlinkingSlow : StatusLightState.On;

        return StatusLightState.Off;
    }

    public override void ToggleValue(EntityUid owner, bool setting)
    {
        if (EntityManager.TryGetComponent(owner, out VendingMachineComponent? vending))
        {
            vending.FreeVend = !setting;
            EntityManager.Dirty(owner, vending);
        }
    }

    public override bool GetValue(EntityUid owner)
    {
        return EntityManager.TryGetComponent(owner, out VendingMachineComponent? vending) && !vending.FreeVend;
    }
}
