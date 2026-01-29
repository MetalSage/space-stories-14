using Robust.Shared.Map;

namespace Content.Shared._Stories.TargetingTeleporter;

public abstract partial class SharedTargetingTeleporterSystem
{
    protected bool SetupEye(Entity<TargetingTeleporterComponent> ent,
        EntityCoordinates? coords = null,
        EntityUid? user = null)
    {
        if (_net.IsClient)
            return false;

        if (ent.Comp.EyeEntity != null)
            return false;

        var proto = ent.Comp.EyeEntityProto;

        if (coords == null)
            coords = Transform(ent.Owner).Coordinates;

        if (proto != null)
        {
            var eye = SpawnAtPosition(proto, coords.Value);
            _xform.SetCoordinates(eye, Transform(eye), coords.Value, Angle.Zero);

            var comp = _componentFactory.GetComponent<TargetingTeleporterEyeComponent>();
            comp.Teleporter = ent;
            comp.User = user;
            AddComp(eye, comp);
            ent.Comp.EyeEntity = eye;
            Dirty(ent);
        }

        return true;
    }

    protected void ClearEye(Entity<TargetingTeleporterComponent> ent)
    {
        if (_net.IsClient)
            return;

        QueueDel(ent.Comp.EyeEntity);
        ent.Comp.EyeEntity = null;
        Dirty(ent);
    }

    protected void AttachEye(Entity<TargetingTeleporterComponent> ent, EntityUid user)
    {
        if (ent.Comp.EyeEntity == null)
            return;

        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(user, false, eyeComp);
            _eye.SetTarget(user, ent.Comp.EyeEntity.Value, eyeComp);
        }

        EnsureComp<TargetingTeleporterUserComponent>(user).Teleporter = ent;
        EnsureComp<TargetingTeleporterUserComponent>(user).Eye = ent.Comp.EyeEntity.Value;

        _mover.SetRelay(user, ent.Comp.EyeEntity.Value);
    }
}
