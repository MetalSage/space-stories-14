using Content.Shared.Actions;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Stories.Pontific;

public sealed partial class PontificSystem : EntitySystem
{
    [ValidatePrototypeId<StatusEffectPrototype>]
    private const string PontificFlameStatusEffect = "PontificFlame";

    [ValidatePrototypeId<StatusEffectPrototype>]
    private const string PontificPrayerStatusEffect = "PontificPrayer";

    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PontificComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PontificComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PontificComponent, PontificPrayerEvent>(OnPrayer);
        SubscribeLocalEvent<PontificComponent, PontificFlameSwordsEvent>(OnFlameSwords);

        SubscribeLocalEvent<CreateEntityEvent>(OnCreateEntity); // TODO: Move to abilities system

        InitializeFlame();
        InitializePrayer();
    }

    private void OnInit(Entity<PontificComponent> entity, ref ComponentInit args)
    {
        foreach (var action in entity.Comp.Actions)
        {
            var actionId = _action.AddAction(entity, action);
            if (actionId.HasValue)
                entity.Comp.GrantedActions.Add(actionId.Value);
        }
    }

    private void OnShutdown(Entity<PontificComponent> entity, ref ComponentShutdown args)
    {
        foreach (var action in entity.Comp.GrantedActions)
        {
            _action.RemoveAction(entity, action);
        }
    }

    private void OnFlameSwords(Entity<PontificComponent> entity, ref PontificFlameSwordsEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<PontificFlameComponent>(entity))
            return;

        if (_statusEffects.TryAddStatusEffect<PontificFlameComponent>(entity, PontificFlameStatusEffect, args.Duration, true))
        {
            EnsureComp<PontificFlameComponent>(entity).DamageMultiplier = args.DamageMultiplier;
            EnsureComp<PontificFlameComponent>(entity).SpeedMultiplier = args.SpeedMultiplier;
            _movementSpeed.RefreshMovementSpeedModifiers(entity);
            args.Handled = true;
        }
    }

    private void OnPrayer(Entity<PontificComponent> entity, ref PontificPrayerEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<PontificFlameComponent>(entity))
            return;

        if (_statusEffects.TryAddStatusEffect<PontificPrayerComponent>(entity, PontificPrayerStatusEffect, args.Duration, true))
        {
            if (args.PrayerSound is { } sound)
                _audio.PlayPvs(sound, entity);

            args.Handled = true;
        }
    }

    private void OnCreateEntity(CreateEntityEvent args) // TODO: Move to abilities system
    {
        Spawn(args.Proto, Transform(args.Performer).Coordinates);
    }
}
