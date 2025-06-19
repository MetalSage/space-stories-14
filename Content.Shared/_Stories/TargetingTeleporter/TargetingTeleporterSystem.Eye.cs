using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.DoAfter;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Station;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._Stories.TargetingTeleporter;

public abstract partial class SharedTargetingTeleporterSystem
{
    protected bool SetupEye(Entity<TargetingTeleporterComponent> ent, EntityCoordinates? coords = null, EntityUid? user = null)
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
