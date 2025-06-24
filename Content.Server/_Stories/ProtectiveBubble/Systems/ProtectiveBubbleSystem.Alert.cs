using Robust.Shared.Physics.Events;
using Content.Shared._Stories.ProtectiveBubble.Components;
using Content.Shared.Projectiles;
using Content.Shared.Damage;
using Content.Shared.Rounding;
using Content.Server.Destructible;
using Content.Shared.Alert;

namespace Content.Server._Stories.ProtectiveBubble;

public sealed partial class ProtectiveBubbleSystem
{
    public const string ProjectiveBubbleAlertId = "ProjectiveBubble"; // TODO: Добавить возможность менять
    public void InitializeAlert()
    {
        SubscribeLocalEvent<ProtectiveBubbleComponent, DamageChangedEvent>(OnDamage);

        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ProtectedByProtectiveBubbleComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnDamage(EntityUid uid, ProtectiveBubbleComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;

        foreach (var ent in component.ProtectedEntities)
        {
            if (!_proto.TryIndex<AlertPrototype>(ProjectiveBubbleAlertId, out var proto))
                continue;

            var severity = ContentHelpers.RoundToLevels(MathF.Max(proto.MinSeverity, args.Damageable.Damage.GetTotal().Float()), (float)_destructible.DestroyedAt(uid), proto.MaxSeverity);
            _alerts.ShowAlert(ent, ProjectiveBubbleAlertId, (short)severity);
        }
    }

    private void OnInit(EntityUid uid, ProtectedByProtectiveBubbleComponent component, ComponentInit args)
    {
        if (!(component.ProtectiveBubble is { } bubble))
            return;

        if (!TryComp<DamageableComponent>(bubble, out var damageable))
            return;

        if (!_proto.TryIndex<AlertPrototype>(ProjectiveBubbleAlertId, out var proto))
            return;

        var severity = ContentHelpers.RoundToLevels(MathF.Max(proto.MinSeverity, (float)damageable.TotalDamage), (float)_destructible.DestroyedAt(bubble), proto.MaxSeverity);
        _alerts.ShowAlert(uid, ProjectiveBubbleAlertId, (short)severity);
    }

    private void OnShutdown(EntityUid uid, ProtectedByProtectiveBubbleComponent component, ComponentShutdown args)
    {
        _alerts.ClearAlert(uid, ProjectiveBubbleAlertId);
    }
}
