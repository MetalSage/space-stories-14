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
        if (_entityWhitelist.IsBlacklistFailOrNull(entity.Comp.Blacklist, args.Climber) && _entityWhitelist.IsWhitelistPassOrNull(entity.Comp.Blacklist, args.Climber))
            _polymorph.PolymorphEntity(args.Climber, entity.Comp.Polymorph);
    }
}
