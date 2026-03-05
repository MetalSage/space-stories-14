using System.Reflection;
using Content.Shared.Pulling.Events;
using Content.Shared.Weapons.Misc;

namespace Content.Server._Stories.TetherGun;

public sealed class StoriesTetherGunSystem : EntitySystem
{
    [Dependency] private readonly SharedTetherGunSystem _sharedTetherGun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TetheredComponent, BeingPulledAttemptEvent>(Cancel);
        SubscribeLocalEvent<StartPullAttemptEvent>(CancelTetherOnPulled);
    }

    public void StopTetherGun(EntityUid gunUid)
    {
        if (TryComp<TetherGunComponent>(gunUid, out var comp))
            StopTether(gunUid, comp);
    }

    public void StopTether(EntityUid gunUid, BaseForceGunComponent component, bool land = true, bool transfer = false)
    {
        var method = typeof(SharedTetherGunSystem).GetMethod(
            "StopTether", 
            BindingFlags.Instance | BindingFlags.NonPublic, 
            null, 
            new[] { typeof(EntityUid), typeof(BaseForceGunComponent), typeof(bool), typeof(bool) }, 
            null);

        method?.Invoke(_sharedTetherGun, new object[] { gunUid, component, land, transfer });
    }

    public void StopTether(EntityUid entityUid, bool land = true, bool transfer = false)
    {
        if (TryComp<TetheredComponent>(entityUid, out var tetheredComponent) &&
            TryComp<TetherGunComponent>(tetheredComponent.Tetherer, out var gunComp))
        {
            StopTether(tetheredComponent.Tetherer, gunComp, land, transfer);
        }
    }

    private void Cancel(EntityUid uid, TetheredComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void CancelTetherOnPulled(StartPullAttemptEvent args)
    {
        if (HasComp<TetheredComponent>(args.Pulled))
            args.Cancel();
    }
}
