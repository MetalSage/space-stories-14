using Content.Shared.Explosion;
using Content.Shared.Temperature;
using Content.Shared.Interaction.Events;
using Content.Server._Stories.ForceUser.ProtectiveBubble.Components;

namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Systems;

public sealed partial class ProtectiveBubbleSystem
{
    public void InitializeProtected()
    {
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, ModifyChangedTemperatureEvent>(OnTemperatureChangeAttempt);
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, GetExplosionResistanceEvent>(OnGetExplosionResistance);
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, AttackAttemptEvent>(OnAttack);
    }
    public void UpdateProtected(float frameTime)
    {
        var query = EntityQueryEnumerator<ProtectedByProtectiveBubbleComponent>();
        while (query.MoveNext(out var uid, out var component))
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
        args.DamageCoefficient = 0; // Щит полностью защищает от взрыва впитывая весь урон.
    }
    private void OnTemperatureChangeAttempt(EntityUid uid, ProtectedByProtectiveBubbleComponent component, ModifyChangedTemperatureEvent args)
    {
        args.TemperatureDelta *= component.TemperatureCoefficient;
    }
}
