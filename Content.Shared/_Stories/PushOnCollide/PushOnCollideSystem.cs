using Content.Shared._Stories.Damage.Components;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;

namespace Content.Shared._Stories.Damage.Systems;
public sealed partial class PushOnCollideSystem : EntitySystem
{
    // TODO: Добавить откидывание при контакте с кем-то и значения в компонент
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PushOnCollideComponent, ProjectileHitEvent>(OnProjectileHit);
    }
    private void OnProjectileHit(EntityUid uid, PushOnCollideComponent component, ref ProjectileHitEvent args)
    {
        var shooter = args.Shooter.HasValue ? args.Shooter.Value : uid;
        var xform = Transform(shooter);
        var strength = 10;
        var xformQuery = GetEntityQuery<TransformComponent>();
        var worldPos = _xform.GetWorldPosition(xform, xformQuery);
        var foo = _xform.GetWorldPosition(args.Target, xformQuery) - worldPos;
        _throwing.TryThrow(args.Target, foo * 10, strength, uid, 0);
    }
}
