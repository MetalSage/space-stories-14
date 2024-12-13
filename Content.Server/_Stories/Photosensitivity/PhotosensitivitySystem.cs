using System.Numerics;
using Content.Server.Popups;
using Content.Shared.Damage;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using Content.Shared.Maps;

namespace Content.Server._Stories.Photosensitivity;

public sealed partial class PhotosensitivitySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    private const float UpdateTimer = 2f;
    private float _timer;
    public const float MaxIllumination = 10f;
    public const float MinIllumination = 0f;

    // FIXME: Shitcode + Hardcode
    public override void Update(float frameTime)
    {
        _timer += frameTime;

        if (_timer < UpdateTimer)
            return;

        _timer -= UpdateTimer;

        var query = EntityQueryEnumerator<PhotosensitivityComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Enabled)
                continue;

            var gridUid = Transform(uid).GridUid;

            if (gridUid != null && TryComp<MapGridComponent>(gridUid, out var grid))
            {
                var tile = grid.GetTileRef(Transform(uid).Coordinates);

                if (tile.IsSpace(_tileDefManager))
                {
                    _damageable.TryChangeDamage(uid, comp.DamageInSpace, true, false);
                    _popup.PopupEntity("Свет выжигает вас!", uid, uid);
                    continue;
                }
            }
            else
            {
                _damageable.TryChangeDamage(uid, comp.DamageInSpace, true, false);
                _popup.PopupEntity("Свет выжигает вас!", uid, uid);
                continue;
            }

            var illumination = Math.Min(GetIllumination(uid), 10);

            if (illumination > 1.5)
            {
                _damageable.TryChangeDamage(uid, comp.Damage * illumination, true, false);
                _popup.PopupEntity("Свет выжигает вас!", uid, uid);
            }
            else if (illumination < 1)
            {
                _damageable.TryChangeDamage(uid, comp.DarknessHealing, true, false);
            }
        }
    }

    // В душе не чаю, что тут написано Lokilife'ом и не желаю разбираться, но
    // это единственный щиткод от этого автора, который будет в тенеморфе.
    public float GetIllumination(EntityUid uid)
    {
        var destTrs = Transform(uid);

        var lightPoints = _entityLookup.GetEntitiesInRange<PointLightComponent>(_transform.GetMapCoordinates(destTrs), 20f);
        var destination = _transform.GetWorldPosition(destTrs);

        var illumination = 0f;

        foreach (var lightPoint in lightPoints)
        {
            if (!lightPoint.Comp.Enabled)
                continue;

            var sourceTrs = Transform(lightPoint);
            var source = _transform.GetWorldPosition(sourceTrs);

            var box = Box2.FromTwoPoints(_transform.GetWorldPosition(sourceTrs), _transform.GetWorldPosition(destTrs));
            var grids = new List<Entity<MapGridComponent>>();
            _mapManager.FindGridsIntersecting(sourceTrs.MapID, box, ref grids, true);

            var dir = destination - source;
            var dist = dir.Length();

            if (dist > lightPoint.Comp.Radius)
                continue;

            var lightDirInterrupted = false;

            foreach (var grid in grids)
            {
                var gridTrs = Transform(grid);

                Vector2 srcLocal = sourceTrs.ParentUid == grid.Owner
                    ? sourceTrs.LocalPosition
                    : Vector2.Transform(source, gridTrs.InvLocalMatrix);

                Vector2 dstLocal = destTrs.ParentUid == grid.Owner
                    ? destTrs.LocalPosition
                    : Vector2.Transform(destination, gridTrs.InvLocalMatrix);

                Vector2i sourceGrid = new(
                    (int) Math.Floor(srcLocal.X / grid.Comp.TileSize),
                    (int) Math.Floor(srcLocal.Y / grid.Comp.TileSize));

                Vector2i destGrid = new(
                    (int) Math.Floor(dstLocal.X / grid.Comp.TileSize),
                    (int) Math.Floor(dstLocal.Y / grid.Comp.TileSize));

                var line = new GridLineEnumerator(sourceGrid, destGrid);

                while (line.MoveNext())
                {
                    foreach (var entity in _mapSystem.GetAnchoredEntities(grid, grid.Comp, line.Current))
                    {
                        if (TryComp<OccluderComponent>(entity, out var occluder) && occluder.Enabled)
                        {
                            lightDirInterrupted = true;
                            break;
                        }
                    }
                    if (lightDirInterrupted) break;
                }
            }

            if (lightDirInterrupted) continue;

            if (lightPoint.Comp.MaskPath is { } maskPath && maskPath.EndsWith("cone.png"))
            {
                var lightPointRotation = _transform.GetWorldRotation(lightPoint);
                var entityVector = destination - source;
                var entityAngle = entityVector.ToWorldAngle();

                if (entityAngle > lightPointRotation + 45 || entityAngle < lightPointRotation - 45)
                    continue;
            }

            illumination = Math.Max(illumination, lightPoint.Comp.Radius - lightPoint.Comp.Energy * dist);
        }

        if (illumination > MaxIllumination)
            illumination = MaxIllumination;

        if (illumination < MinIllumination)
            illumination = MinIllumination;

        return illumination;
    }
}
