using Content.Client.Eye;
using Content.Shared._Stories.TargetingTeleporter;

namespace Content.Client._Stories.TargetingTeleporter;

public sealed class TargetingTeleporterSystem : SharedTargetingTeleporterSystem
{
    public override void OnInit(Entity<TargetingTeleporterUserComponent> entity, ref ComponentInit args)
    {
        base.OnInit(entity, ref args);
        RemComp<LerpingEyeComponent>(entity);
    }

    public override void OnShutdown(Entity<TargetingTeleporterUserComponent> entity, ref ComponentShutdown args)
    {
        base.OnShutdown(entity, ref args);
        EnsureComp<LerpingEyeComponent>(entity);
    }
}
