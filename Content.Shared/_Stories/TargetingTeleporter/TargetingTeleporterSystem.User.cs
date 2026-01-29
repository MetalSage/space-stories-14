using Content.Shared.Movement.Components;

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

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = AllEntityQuery<EyeComponent, TargetingTeleporterUserComponent, InputMoverComponent>();

        while (query.MoveNext(out var uid, out var eye, out var user, out var mover))
        {
            if (user.Teleporter is { } teleporter && Comp<TargetingTeleporterComponent>(teleporter).GridUid is { } grid)
            {
                _eye.SetRotation(uid, -Transform(grid).LocalRotation);
                mover.RelativeEntity = grid;
                mover.RelativeRotation = 0f;
                mover.TargetRelativeRotation = 0f;
            }
        }
    }

    public virtual void OnInit(Entity<TargetingTeleporterUserComponent> entity, ref ComponentInit args)
    {
        entity.Comp.SetExitActionEntity = _actions.AddAction(entity, entity.Comp.SetExitAction);
        entity.Comp.ExitActionEntity = _actions.AddAction(entity, entity.Comp.ExitAction);
    }

    public virtual void OnShutdown(Entity<TargetingTeleporterUserComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.SetExitActionEntity);
        _actions.RemoveAction(entity.Owner, entity.Comp.ExitActionEntity);
    }

    private void OnSetExit(Entity<TargetingTeleporterUserComponent> entity,
        ref TargettingTeleporterSetExitPortalEvent args)
    {
        if (_net.IsClient)
            return;

        if (args.Handled)
            return;

        if (entity.Comp.Teleporter is { } teleporter && entity.Comp.Eye is { } eye &&
            TryComp<TargetingTeleporterComponent>(teleporter, out var comp))
        {
            SpawnExitPortal((teleporter, comp), Transform(eye).Coordinates);

            ClearEye((teleporter, comp));
            _mover.ResetCamera(entity);

            if (TryComp(entity, out EyeComponent? eyeComp))
                _eye.SetDrawFov(entity, true, eyeComp);

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
                _eye.SetDrawFov(entity, true, eyeComp);

            RemComp<TargetingTeleporterUserComponent>(entity);
        }

        args.Handled = true;
    }
}
