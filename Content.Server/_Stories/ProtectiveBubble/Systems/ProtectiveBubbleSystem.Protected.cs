using Content.Server._Stories.ForceUser.ProtectiveBubble.Components;
using Content.Shared.Explosion;
using Content.Shared.Interaction.Events;
using Content.Shared.Temperature;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Systems;

public sealed partial class ProtectiveBubbleSystem
{
    private static readonly EntProtoId PressureImmunity = "StatusEffectPressureImmunity";

    public void InitializeProtected()
    {
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, BeforeHeatExchangeEvent>(
            OnBeforeHeatExchange);
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, GetExplosionResistanceEvent>(
            OnGetExplosionResistance);
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, AttackAttemptEvent>(OnAttack);
    }

    public void UpdateProtected(float frameTime)
    {
        var query = EntityQueryEnumerator<ProtectedByProtectiveBubbleComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            _statusEffect.TrySetStatusEffectDuration(uid, PressureImmunity, TimeSpan.FromSeconds(frameTime));
        }
    }

    private void OnAttack(EntityUid uid, ProtectedByProtectiveBubbleComponent component, AttackAttemptEvent args)
    {
        if (component.ProtectiveBubble == args.Target)
            args.Cancel();
    }

    private void OnGetExplosionResistance(EntityUid uid,
        ProtectedByProtectiveBubbleComponent component,
        ref GetExplosionResistanceEvent args)
    {
        args.DamageCoefficient = 0; // Щит полностью защищает от взрыва впитывая весь урон.
    }

    private void OnBeforeHeatExchange(EntityUid uid,
        ProtectedByProtectiveBubbleComponent component,
        ref BeforeHeatExchangeEvent args)
    {
        args.HeatTransferModifier *= component.TemperatureCoefficient;
    }
}
