using Content.Server._Stories.LightSensitivity;
using Content.Shared._Stories.LightSensitivity;
using Content.Shared.Alert;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Stories.LightSensitivity;

public sealed partial class LightSensitivityDamageSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private LightSensitivitySystem _lightSensitivity = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightSensitivityDamageComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LightSensitivityDamageComponent, LightSensitivityComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out var damageComp, out var lightComp, out var meta))
        {
            if (meta.EntityPaused)
                continue;

            if (damageComp.NextUpdate > _timing.CurTime)
                continue;

            damageComp.NextUpdate = _timing.CurTime + damageComp.UpdateInterval;

            var inDarkness = _lightSensitivity.IsInDarkness(uid, lightComp);

            if (inDarkness != damageComp.WasInDarkness)
            {
                damageComp.WasInDarkness = inDarkness;
                _movement.RefreshMovementSpeedModifiers(uid);

                if (damageComp.LightAlert is { } alert)
                {
                    if (inDarkness)
                        _alerts.ClearAlert(uid, alert);
                    else
                        _alerts.ShowAlert(uid, alert);
                }
            }

            if (_mobState.IsDead(uid))
                continue;

            if (inDarkness)
            {
                if (damageComp.HealInDarkness is { } heal)
                    _damageable.TryChangeDamage(uid, heal, true);
            }
            else
            {
                _damageable.TryChangeDamage(uid, damageComp.DamageOnLight);
            }
        }
    }

    private void OnRefreshSpeed(Entity<LightSensitivityDamageComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var multiplier = ent.Comp.WasInDarkness == true ? ent.Comp.DarkSpeedMultiplier : ent.Comp.LightSpeedMultiplier;
        args.ModifySpeed(multiplier, multiplier);
    }
}
