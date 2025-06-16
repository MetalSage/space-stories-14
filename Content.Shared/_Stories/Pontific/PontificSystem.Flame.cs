using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Stories.Pontific;

public sealed partial class PontificSystem
{
    public void InitializeFlame()
    {
        base.Initialize();

        SubscribeLocalEvent<PontificFlameComponent, ComponentInit>(OnFlameInit);
        SubscribeLocalEvent<PontificFlameComponent, ComponentShutdown>(OnFlameShutdown);

        SubscribeLocalEvent<PontificFlameComponent, GetMeleeDamageEvent>(OnGetMeleeDamageEvent);
        SubscribeLocalEvent<PontificFlameComponent, RefreshMovementSpeedModifiersEvent>(OnSpeedRefresh);
    }

    private void OnFlameInit(Entity<PontificFlameComponent> entity, ref ComponentInit args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(entity);

        _appearance.SetData(entity, PontificVisuals.State, PontificState.Flame);
    }

    private void OnFlameShutdown(Entity<PontificFlameComponent> entity, ref ComponentShutdown args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(entity);

        if (_appearance.TryGetData(entity, PontificVisuals.State, out var data) && data is PontificState.Flame)
            _appearance.SetData(entity, PontificVisuals.State, PontificState.Base);
    }

    private void OnSpeedRefresh(Entity<PontificFlameComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(entity.Comp.SpeedMultiplier, entity.Comp.SpeedMultiplier);
    }

    private void OnGetMeleeDamageEvent(EntityUid uid, PontificFlameComponent component, ref GetMeleeDamageEvent args)
    {
        args.Damage *= component.DamageMultiplier;
    }
}
