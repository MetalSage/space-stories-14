using System.Numerics;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Stories.Reflectors;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Direction = Robust.Shared.Maths.Direction;

namespace Content.Server._Stories.Reflectors;

public sealed class ReflectorSystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReflectorComponent, ProjectileReflectAttemptEvent>(OnReflectCollide);
    }

    private void OnReflectCollide(EntityUid uid, ReflectorComponent component, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var collisionDirection = CalculateCollisionDirection(uid, args.ProjUid);
        if (component.BlockedDirections.Contains(collisionDirection.ToString()))
            return;

        if (TryReflectProjectile(uid, component, args.ProjUid, collisionDirection))
            args.Cancelled = true;
    }

    private Direction CalculateCollisionDirection(EntityUid uid, EntityUid projectile)
    {
        var projWorldPos = _transform.GetWorldPosition(projectile);
        var uidWorldMatrix = _transform.GetInvWorldMatrix(Transform(uid));
        var localCollisionPoint = Vector2.Transform(projWorldPos, uidWorldMatrix);

        return localCollisionPoint.ToAngle().GetCardinalDir();
    }

    private bool TryReflectProjectile(EntityUid user,
        ReflectorComponent component,
        EntityUid projectile,
        Direction collisionDirection)
    {
        if (!TryComp<ReflectiveComponent>(projectile, out var reflective) ||
            reflective.Reflective == 0x0 ||
            !TryComp<GunComponent>(user, out var gunComponent) ||
            _whitelistSystem.IsWhitelistPass(component.Blacklist, projectile) ||
            _whitelistSystem.IsWhitelistFail(component.Whitelist, projectile))
            return false;

        if (!TryComp<AmmoComponent>(projectile, out var ammoComp))
            return false;

        var targetOffset = ReflectBasedOnType(component, collisionDirection);
        if (!targetOffset.HasValue)
            return false;

        var xform = Transform(user);
        var targetPos = new EntityCoordinates(user, targetOffset.Value);

        _transform.SetLocalPosition(projectile,
            xform.LocalPosition + xform.LocalRotation.RotateVec(targetOffset.Value));

        var gunEnt = new Entity<GunComponent>(user, gunComponent);
        var ammoList = new List<(EntityUid? Entity, IShootable Shootable)>
        {
            (projectile, ammoComp),
        };

        _gun.Shoot(gunEnt, ammoList, xform.Coordinates, targetPos, out _);

        if (_netManager.IsServer)
            _popup.PopupEntity(Loc.GetString("reflect-shot"), user);

        return true;
    }

    private Vector2 ReflectAngular(Direction collisionDirection)
    {
        return collisionDirection switch
        {
            Direction.North => Vector2.UnitY,
            Direction.South => -Vector2.UnitY,
            Direction.East => -Vector2.UnitX,
            Direction.West => Vector2.UnitX,
            _ => throw new ArgumentOutOfRangeException(nameof(collisionDirection)),
        };
    }

    private Vector2? ReflectBasedOnType(ReflectorComponent component, Direction collisionDirection)
    {
        return component.State switch
        {
            ReflectorType.Simple => component.ReflectionDirection?.ToVec(),
            ReflectorType.Angular => ReflectAngular(collisionDirection),
            _ => throw new ArgumentOutOfRangeException(nameof(component.State),
                component.State,
                "Invalid ReflectorType encountered."),
        };
    }
}
