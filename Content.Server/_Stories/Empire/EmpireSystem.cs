using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared._Stories.Empire.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Server.Audio;

namespace Content.Server._Stories.Empire;

public sealed class EmpireSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly SharedStunSystem _sharedStun = default!;

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
