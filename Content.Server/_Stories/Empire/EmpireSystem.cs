using Content.Server.Roles;
using Content.Shared._Stories.Empire.Components;

namespace Content.Server._Stories.Empire;

public sealed class EmpireSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmpireMemberRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<HypnotizedEmpireMemberRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }

    private void OnGetBriefing(Entity<EmpireMemberRoleComponent> role, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("empire-briefing"));
    }

    private void OnGetBriefing(Entity<HypnotizedEmpireMemberRoleComponent> role, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("hypnosis-empire-briefing"));
    }
}
