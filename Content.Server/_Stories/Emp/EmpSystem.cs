using Content.Server.Emp;
using Content.Shared.Power.EntitySystems;

namespace Content.Server._Stories.Emp;

public sealed class EmpSystem : EntitySystem
{
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DisableOnEmpComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<DisableOnEmpComponent, EmpDisabledRemoved>(OnEmpDisabledRemoved);
    }
    private void OnEmpPulse(Entity<DisableOnEmpComponent> entity, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;
        _powerReceiver.SetPowerDisabled(entity, true);
    }
    private void OnEmpDisabledRemoved(Entity<DisableOnEmpComponent> entity, ref EmpDisabledRemoved args)
    {
        _powerReceiver.SetPowerDisabled(entity, false);
    }
}
