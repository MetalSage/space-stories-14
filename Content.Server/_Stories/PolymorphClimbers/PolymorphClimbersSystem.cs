using Content.Server.Polymorph.Systems;
using Content.Shared.Climbing.Events;
using Content.Shared.Whitelist;

namespace Content.Server._Stories.PolymorphClimbers;

public sealed partial class PolymorphClimbersSystem : EntitySystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PolymorphClimbersComponent, ClimbedOnEvent>(OnClimbedOn);
    }

    private void OnClimbedOn(Entity<PolymorphClimbersComponent> entity, ref ClimbedOnEvent args)
    {
        if (_entityWhitelist.IsBlacklistPass(entity.Comp.Blacklist, args.Climber))
            return;

        if (_entityWhitelist.IsWhitelistFail(entity.Comp.Whitelist, args.Climber))
            return;

        _polymorph.PolymorphEntity(args.Climber, entity.Comp.Polymorph);
    }
}
