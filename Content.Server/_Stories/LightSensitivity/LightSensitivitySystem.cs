using Content.Shared._Stories.LightSensitivity;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Server._Stories.LightSensitivity;

public sealed partial class LightSensitivitySystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private float CalculateLightLevel(EntityUid uid, TransformComponent xform, float lookupRange)
    {
        var worldPos = _transform.GetWorldPosition(xform);
        var totalLight = 0f;

        var lights = _lookup.GetEntitiesInRange<PointLightComponent>(xform.Coordinates, lookupRange);
        foreach (var light in lights)
        {
            var (lightUid, pointLight) = light;

            if (!pointLight.Enabled)
                continue;

            var lightPos = _transform.GetWorldPosition(lightUid);
            var distance = (lightPos - worldPos).Length();

            if (distance > pointLight.Radius)
                continue;

            if (distance > 0.01f)
            {
                var direction = (worldPos - lightPos).Normalized();
                var ray = new CollisionRay(lightPos, direction, (int) CollisionGroup.Opaque);
                var blocked = false;

                foreach (var result in _physics.IntersectRay(xform.MapID, ray, distance, uid))
                {
                    if (result.HitEntity != uid)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                    continue;
            }

            var t = distance / pointLight.Radius;
            totalLight += pointLight.Energy * (1f - t * t);
        }

        return totalLight;
    }

    public bool IsInDarkness(EntityUid uid, LightSensitivityComponent? comp = null, TransformComponent? xform = null)
    {
        if (!Resolve(uid, ref comp, false) || !Resolve(uid, ref xform, false))
            return true;

        return CalculateLightLevel(uid, xform, comp.LookupRange) <= comp.DarknessThreshold;
    }
}
