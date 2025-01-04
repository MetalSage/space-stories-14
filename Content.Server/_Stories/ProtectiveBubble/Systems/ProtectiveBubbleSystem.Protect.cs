using Robust.Shared.Physics.Events;
using Content.Server._Stories.ForceUser.ProtectiveBubble.Components;
using Content.Shared.Projectiles;
using Content.Shared.Damage;
using Content.Shared.Rounding;
using SixLabors.ImageSharp.Processing;
using Content.Shared.Alert;

namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Systems;

public sealed partial class ProtectiveBubbleSystem
{
    public const float MaxBubbleDamage = 100f; // TODO: Добавить возможность менять
    public void InitializeBubble()
    {
        SubscribeLocalEvent<ProtectiveBubbleComponent, EndCollideEvent>(OnEntityExit);
        SubscribeLocalEvent<ProtectiveBubbleComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ProtectiveBubbleComponent, StartCollideEvent>(OnEntityEnter);
        SubscribeLocalEvent<ProtectiveBubbleComponent, PreventCollideEvent>(OnCollide);
        SubscribeLocalEvent<ProtectiveBubbleComponent, DamageChangedEvent>(OnDamage);
    }
    private void OnDamage(EntityUid uid, ProtectiveBubbleComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null || component.User == null)
            return;
        var severity = ContentHelpers.RoundToLevels(MathF.Max(0f, args.Damageable.Damage.GetTotal().Float()), MaxBubbleDamage, 20);
        _alerts.ShowAlert(component.User.Value, "ProjectiveBubble", (short) severity);
    }
    public void StartBubbleWithUser(string proto, EntityUid user)
    {
        var bubble = Spawn(proto, Transform(user).Coordinates);
        _xform.SetParent(bubble, user);

        if (HasComp<ProtectiveBubbleComponent>(bubble))
            RemComp<ProtectiveBubbleComponent>(bubble);

        var comp = _factory.GetComponent<ProtectiveBubbleComponent>();
        comp.User = user;
        AddComp(bubble, comp);

        var comp1 = _factory.GetComponent<ProtectiveBubbleUserComponent>();
        comp1.ProtectiveBubble = bubble;
        AddComp(user, comp1);
    }
    private void OnCollide(EntityUid uid, ProtectiveBubbleComponent component, ref PreventCollideEvent args)
    {
        if (TryComp<ProjectileComponent>(args.OtherEntity, out var bullet) && bullet.Shooter == component.User && component.User != null)
            args.Cancelled = true;
    }
    private void OnEntityExit(EntityUid uid, ProtectiveBubbleComponent component, ref EndCollideEvent args)
    {
        if (!HasComp<ProjectileComponent>(args.OtherEntity))
            StopProtect(args.OtherEntity, component);
    }
    private void OnEntityEnter(EntityUid uid, ProtectiveBubbleComponent component, ref StartCollideEvent args)
    {
        if (!HasComp<ProjectileComponent>(args.OtherEntity))
            StartProtect(args.OtherEntity, uid, component);
    }
    private void OnShutdown(EntityUid uid, ProtectiveBubbleComponent component, ComponentShutdown args)
    {
        foreach (var ent in component.ProtectedEntities)
        {
            StopProtect(ent, component);
        }

        if (component.User != null)
            RemComp<ProtectiveBubbleUserComponent>(component.User.Value);
    }
    private void StartProtect(EntityUid uid, EntityUid bubble, ProtectiveBubbleComponent component)
    {
        if (IsProtected(uid))
            return;
        var comp = _factory.GetComponent<ProtectedByProtectiveBubbleComponent>();
        comp.ProtectiveBubble = bubble;
        comp.TemperatureCoefficient = component.TemperatureCoefficient;
        component.ProtectedEntities.Add(uid);
        AddComp(uid, comp);
    }
    private void StopProtect(EntityUid uid, ProtectiveBubbleComponent component)
    {
        if (!IsProtected(uid))
            return;
        component.ProtectedEntities.Remove(uid);
        RemComp<ProtectedByProtectiveBubbleComponent>(uid);
    }
    public bool IsProtected(EntityUid uid)
    {
        return HasComp<ProtectedByProtectiveBubbleComponent>(uid);
    }
}
