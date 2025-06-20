using Content.Shared.Speech.Muting;
using Content.Shared._Stories.Force;
using Content.Shared.Stunnable;

namespace Content.Shared._Stories.Pontific;

public sealed partial class PontificSystem
{
    public void InitializePrayer()
    {
        base.Initialize();

        SubscribeLocalEvent<PontificPrayerComponent, ComponentInit>(OnPrayerInit);
        SubscribeLocalEvent<PontificPrayerComponent, ComponentShutdown>(OnPrayerShutdown);
    }

    private void OnPrayerInit(Entity<PontificPrayerComponent> entity, ref ComponentInit args)
    {
        if (HasComp<AppearanceComponent>(entity))
            _appearance.SetData(entity, PontificVisuals.State, PontificState.Prayer);

        EnsureComp<ForceComponent>(entity).PassiveVolume = 10;
        EnsureComp<StunnedComponent>(entity);
        EnsureComp<MutedComponent>(entity);
    }

    private void OnPrayerShutdown(Entity<PontificPrayerComponent> entity, ref ComponentShutdown args)
    {
        if (HasComp<AppearanceComponent>(entity))
            if (_appearance.TryGetData(entity, PontificVisuals.State, out var data) && data is PontificState.Prayer)
                _appearance.SetData(entity, PontificVisuals.State, PontificState.Base);

        EnsureComp<ForceComponent>(entity).PassiveVolume = 0.01f;

        RemComp<StunnedComponent>(entity);
        RemComp<MutedComponent>(entity);
        _movementSpeed.RefreshMovementSpeedModifiers(entity);
    }
}
