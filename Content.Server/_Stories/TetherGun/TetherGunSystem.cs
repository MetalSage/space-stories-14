using Content.Shared.Pulling.Events;
using Content.Shared.Weapons.Misc;

namespace Content.Server._Stories.TetherGun;

public sealed class TetherGunSystem : SharedTetherGunSystem
{
    // TODO: Выбрать лучшее название этому недоразумению. Добавить больше функционала. Перенести в Content.Shared.
    public override void Initialize()
    {
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
        base.StopTether(gunUid, component, land, transfer);
    }
    public void StopTether(EntityUid entityUid, bool land = true, bool transfer = false)
    {
        if (TryComp<TetheredComponent>(entityUid, out var tetheredComponent))
            base.StopTether(tetheredComponent.Tetherer, EnsureComp<TetherGunComponent>(tetheredComponent.Tetherer), land, transfer);
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
