using Content.Shared._Stories.Force;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Content.Shared.Speech.Muting;
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
        EnsureComp<PassiveDamageComponent>(entity).Damage = new()
        {
            DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
            {
                { "Holy", -0.7 },
                { "Blunt", -0.8 },
                { "Slash", -0.8 },
                { "Piercing", -0.8 },
                { "Heat", -0.8 },
                { "Shock", -0.8 },
                { "Cold", -0.8 },
                { "Asphyxiation", -1.2 },
                { "Bloodloss", -1.2 },
            },
        };

        EnsureComp<StunnedComponent>(entity);
        EnsureComp<MutedComponent>(entity);
    }

    private void OnPrayerShutdown(Entity<PontificPrayerComponent> entity, ref ComponentShutdown args)
    {
        if (HasComp<AppearanceComponent>(entity))
        {
            if (_appearance.TryGetData(entity, PontificVisuals.State, out var data) && data is PontificState.Prayer)
                _appearance.SetData(entity, PontificVisuals.State, PontificState.Base);
        }

        EnsureComp<ForceComponent>(entity).PassiveVolume = 0.01f;
        EnsureComp<PassiveDamageComponent>(entity).Damage = new()
        {
            DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
            {
                { "Holy", -0.07 },
                { "Blunt", -0.08 },
                { "Slash", -0.08 },
                { "Piercing", -0.08 },
                { "Heat", -0.08 },
                { "Shock", -0.08 },
                { "Cold", -0.08 },
                { "Asphyxiation", -0.12 },
                { "Bloodloss", -0.12 },
            },
        };

        RemComp<StunnedComponent>(entity);
        RemComp<MutedComponent>(entity);
        _movementSpeed.RefreshMovementSpeedModifiers(entity);
    }
}
