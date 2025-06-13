using Robust.Shared.Physics.Events;
using Content.Shared._Stories.ProtectiveBubble.Components;
using Content.Shared.Projectiles;

namespace Content.Server._Stories.ProtectiveBubble;

public sealed partial class ProtectiveBubbleSystem
{
    public void InitializeCollide()
    {
        SubscribeLocalEvent<ProtectiveBubbleComponent, EndCollideEvent>(OnEntityExit);
        SubscribeLocalEvent<ProtectiveBubbleComponent, StartCollideEvent>(OnEntityEnter);
        SubscribeLocalEvent<ProtectiveBubbleComponent, PreventCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, ProtectiveBubbleComponent component, ref PreventCollideEvent args)
    {
        if (TryComp<ProjectileComponent>(args.OtherEntity, out var bullet))
        {
            if (bullet.Shooter is { } shooter && IsProtected(shooter, uid))
                args.Cancelled = true;
        }
        else if (IsProtected(args.OtherEntity, uid))
            args.Cancelled = true;
    }

    private void OnEntityExit(EntityUid uid, ProtectiveBubbleComponent component, ref EndCollideEvent args)
    {
        if (!HasComp<ProjectileComponent>(args.OtherEntity))
            StopProtect(args.OtherEntity, uid, component);
    }

    private void OnEntityEnter(EntityUid uid, ProtectiveBubbleComponent component, ref StartCollideEvent args)
    {
        if (!HasComp<ProjectileComponent>(args.OtherEntity))
            StartProtect(args.OtherEntity, uid, component);
    }
}
