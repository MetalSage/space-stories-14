using System.Numerics;
using Content.Shared._Stories.Shadowling;
using Content.Shared.Alert;
using Content.Shared.Damage.Systems;
using Content.Shared.Flash;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server._Stories.Photosensitivity;

public sealed partial class PhotosensitivitySystem : EntitySystem
{
    private const float UpdateTimer = 2f;
    public const float MaxIllumination = 10f;
    public const float MinIllumination = 0f;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MapSystem _map = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private TurfSystem _turf = default!;

    private readonly HashSet<Entity<PointLightComponent>> _lightPoints = new();
    private List<Entity<MapGridComponent>> _intersectingGrids = new();
    private float _timer;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PhotosensitivityComponent, AfterFlashedEvent>(OnFlashed);
        SubscribeLocalEvent<PhotosensitivityComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    private void OnRefreshSpeed(Entity<PhotosensitivityComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var multiplier = ent.Comp.WasInDarkness == true
            ? ent.Comp.DarkSpeedMultiplier
            : ent.Comp.LightSpeedMultiplier;

        args.ModifySpeed(multiplier, multiplier);
    }

    private void UpdateLightState(EntityUid uid, PhotosensitivityComponent comp, bool inDarkness)
    {
        if (comp.WasInDarkness == inDarkness)
            return;

        comp.WasInDarkness = inDarkness;
        _movement.RefreshMovementSpeedModifiers(uid);

        if (comp.LightAlert is not { } alert)
            return;

        if (inDarkness)
            _alerts.ClearAlert(uid, alert);
        else
            _alerts.ShowAlert(uid, alert);
    }

    private float GetDamageMultiplier(EntityUid uid, PhotosensitivityComponent comp)
    {
        if (TryComp<MobStateComponent>(uid, out var mobState))
        {
            if (mobState.CurrentState == MobState.Critical || mobState.CurrentState == MobState.Dead)
                return comp.CritDamageMultiplier;
        }

        return 1f;
    }

    private void OnFlashed(Entity<PhotosensitivityComponent> ent, ref AfterFlashedEvent args)
    {
        if (!ent.Comp.Enabled || HasComp<ShadowWalkingComponent>(ent))
            return;

        if (args.Target != ent.Owner)
            return;

        var damageMult = GetDamageMultiplier(ent, ent.Comp);
        var damage = (args.Melee ? ent.Comp.MeleeFlashDamage : ent.Comp.FlashDamage) * damageMult;

        _damageable.TryChangeDamage(ent.Owner, damage, true, false);
        _audio.PlayPvs(ent.Comp.BurnSound, ent);
    }

    public override void Update(float frameTime)
    {
        _timer += frameTime;

        if (_timer < UpdateTimer)
            return;

        _timer -= UpdateTimer;

        var query = EntityQueryEnumerator<PhotosensitivityComponent, MetaDataComponent>();

        while (query.MoveNext(out var uid, out var comp, out var meta))
        {
            if (meta.EntityPaused)
                continue;

            if (!comp.Enabled || HasComp<ShadowWalkingComponent>(uid))
                continue;

            var damageMult = GetDamageMultiplier(uid, comp);
            var gridUid = Transform(uid).GridUid;
            var inSpace = false;

            if (gridUid != null && TryComp<MapGridComponent>(gridUid, out var grid))
            {
                if (_turf.IsSpace(_map.GetTileRef(gridUid.Value, grid, Transform(uid).Coordinates)))
                    inSpace = true;
            }
            else
                inSpace = true;

            if (inSpace)
            {
                UpdateLightState(uid, comp, false);
                _damageable.TryChangeDamage(uid, comp.DamageInSpace * damageMult, true, false);
                _audio.PlayPvs(comp.BurnSound, uid);
                continue;
            }

            var illumination = Math.Min(GetIllumination(uid), 10);

            UpdateLightState(uid, comp, illumination < 1f);

            if (illumination > 1.5f)
            {
                var scale = illumination - 1.5f;
                _damageable.TryChangeDamage(uid, comp.Damage * scale * damageMult, true, false);
                _audio.PlayPvs(comp.BurnSound, uid);
            }
            else if (illumination < 1f)
                _damageable.TryChangeDamage(uid, comp.DarknessHealing, true, false);
        }
    }

    public float GetIllumination(EntityUid uid)
    {
        var destTrs = Transform(uid);

        _lightPoints.Clear();
        _lookup.GetEntitiesInRange(
            _transform.GetMapCoordinates(destTrs),
            20f,
            _lightPoints,
            LookupFlags.Dynamic | LookupFlags.Static | LookupFlags.Contained);

        var destination = _transform.GetWorldPosition(destTrs);

        var illumination = 0f;

        foreach (var lightPoint in _lightPoints)
        {
            if (!lightPoint.Comp.Enabled)
                continue;

            var sourceTrs = Transform(lightPoint);
            var source = _transform.GetWorldPosition(sourceTrs);

            var box = Box2.FromTwoPoints(_transform.GetWorldPosition(sourceTrs), _transform.GetWorldPosition(destTrs));
            _intersectingGrids.Clear();
            _map.FindGridsIntersecting(sourceTrs.MapID, box, ref _intersectingGrids, true);

            var dir = destination - source;
            var dist = dir.Length();

            if (dist > lightPoint.Comp.Radius)
                continue;

            var lightDirInterrupted = false;

            foreach (var grid in _intersectingGrids)
            {
                var gridTrs = Transform(grid);

                var srcLocal = sourceTrs.ParentUid == grid.Owner
                    ? sourceTrs.LocalPosition
                    : Vector2.Transform(source, gridTrs.InvLocalMatrix);

                var dstLocal = destTrs.ParentUid == grid.Owner
                    ? destTrs.LocalPosition
                    : Vector2.Transform(destination, gridTrs.InvLocalMatrix);

                Vector2i sourceGrid = new(
                    (int)Math.Floor(srcLocal.X / grid.Comp.TileSize),
                    (int)Math.Floor(srcLocal.Y / grid.Comp.TileSize));

                Vector2i destGrid = new(
                    (int)Math.Floor(dstLocal.X / grid.Comp.TileSize),
                    (int)Math.Floor(dstLocal.Y / grid.Comp.TileSize));

                var line = new GridLineEnumerator(sourceGrid, destGrid);

                while (line.MoveNext())
                {
                    foreach (var entity in _map.GetAnchoredEntities(grid, grid.Comp, line.Current))
                    {
                        if (TryComp<OccluderComponent>(entity, out var occluder) && occluder.Enabled)
                        {
                            lightDirInterrupted = true;
                            break;
                        }
                    }

                    if (lightDirInterrupted)
                        break;
                }
            }

            if (lightDirInterrupted)
                continue;

            if (lightPoint.Comp.LightMask is { } lightMask)
            {
                var localPos = Vector2.Transform(destination, _transform.GetInvWorldMatrix(sourceTrs));
                var x = localPos.X;
                var y = localPos.Y;

                if (lightMask.Id == "ConeSingle" || lightMask.Id.EndsWith("cone.png"))
                {
                    if (-y + 0.5f < x * x * 0.25f)
                        continue;
                }
                else if (lightMask.Id == "ConeDouble" || lightMask.Id.EndsWith("double_cone.png"))
                {
                    var cond1 = y + 0.5f >= x * x * 0.25f;
                    var cond2 = -y + 0.5f >= x * x * 0.25f;

                    if (!cond1 && !cond2)
                        continue;
                }
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
