using Content.Shared.Explosion;
using Content.Shared.Temperature;
using Content.Shared.Interaction.Events;
using Content.Shared._Stories.ProtectiveBubble.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Damage;

namespace Content.Server._Stories.ProtectiveBubble;

public sealed partial class ProtectiveBubbleSystem
{
    public void InitializeProtection()
    {
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, ModifyChangedTemperatureEvent>(OnTemperatureChangeAttempt);
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, GetExplosionResistanceEvent>(OnGetExplosionResistance);
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, AttackAttemptEvent>(OnAttack);
    }

    public void UpdateProtection(float frameTime)
    {
        var query = EntityQueryEnumerator<ProtectedByProtectiveBubbleComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            _statusEffect.TryAddStatusEffect(uid, "PressureImmunity", TimeSpan.FromSeconds(frameTime), true, "PressureImmunity");
        }
    }

    private void OnAttack(EntityUid uid, ProtectedByProtectiveBubbleComponent component, AttackAttemptEvent args)
    {
        if (component.ProtectiveBubble == args.Target)
            args.Cancel();
    }

    private void OnGetExplosionResistance(EntityUid uid, ProtectedByProtectiveBubbleComponent component, ref GetExplosionResistanceEvent args)
    {
        args.DamageCoefficient = 0; // FIXME: Hardcode
    }

    private void OnTemperatureChangeAttempt(EntityUid uid, ProtectedByProtectiveBubbleComponent component, ModifyChangedTemperatureEvent args)
    {
        args.TemperatureDelta *= 0; // FIXME: Hardcode
    }
}
