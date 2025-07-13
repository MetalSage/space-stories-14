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
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Stories.TargetingTeleporter;

public abstract partial class SharedTargetingTeleporterSystem
{
    private void InitializeUser()
    {
        SubscribeLocalEvent<TargetingTeleporterUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<TargetingTeleporterUserComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TargetingTeleporterUserComponent, TargettingTeleporterExitEvent>(OnExit);
        SubscribeLocalEvent<TargetingTeleporterUserComponent, TargettingTeleporterSetExitPortalEvent>(OnSetExit);
    }

    private void OnInit(Entity<TargetingTeleporterUserComponent> entity, ref ComponentInit args)
    {
        entity.Comp.SetExitActionEntity = _actions.AddAction(entity, "STActionTargettingTeleporterSetExitPortal");
        entity.Comp.ExitActionEntity = _actions.AddAction(entity, "STActionTargettingTeleporterExit");
    }

    private void OnShutdown(Entity<TargetingTeleporterUserComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity, entity.Comp.SetExitActionEntity);
        _actions.RemoveAction(entity, entity.Comp.ExitActionEntity);
    }

    private void OnSetExit(Entity<TargetingTeleporterUserComponent> entity, ref TargettingTeleporterSetExitPortalEvent args)
    {
        if (_net.IsClient)
            return;

        if (args.Handled)
            return;

        if (entity.Comp.Teleporter is { } teleporter && entity.Comp.Eye is { } eye && TryComp<TargetingTeleporterComponent>(teleporter, out var comp))
        {
            SpawnExitPortal((teleporter, comp), Transform(eye).Coordinates);

            ClearEye((teleporter, comp));
            _mover.ResetCamera(entity);

            if (TryComp(entity, out EyeComponent? eyeComp))
            {
                _eye.SetDrawFov(entity, true, eyeComp);
            }

            RemComp<TargetingTeleporterUserComponent>(entity);
            RemComp<TargetingTeleporterComponent>(teleporter);

        }

        args.Handled = true;
    }

    private void OnExit(Entity<TargetingTeleporterUserComponent> entity, ref TargettingTeleporterExitEvent args)
    {
        if (_net.IsClient)
            return;

        if (args.Handled)
            return;

        if (entity.Comp.Teleporter is { } teleporter && TryComp<TargetingTeleporterComponent>(teleporter, out var comp))
        {
            ClearEye((teleporter, comp));
            _mover.ResetCamera(entity);

            if (TryComp(entity, out EyeComponent? eyeComp))
            {
                _eye.SetDrawFov(entity, true, eyeComp);
            }

            RemComp<TargetingTeleporterUserComponent>(entity);
        }

        args.Handled = true;
    }

}
