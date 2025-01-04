using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared._Stories.Empire.Components;
using Content.Shared.Stunnable;
using Content.Shared.Chat;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Server.Roles;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Content.Shared._Stories.Mindshield;

namespace Content.Server._Stories.Empire;

public sealed class EmpireSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _sharedStun = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmpireMemberRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<HypnotizedEmpireMemberRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }
    private void OnGetBriefing(EntityUid uid, EmpireMemberRoleComponent comp, ref GetBriefingEvent args)
    {
        if (!TryComp<MindComponent>(uid, out var mind) || mind.OwnedEntity == null)
            return;

        args.Append(Loc.GetString("empire-briefing"));
    }
    private void OnGetBriefing(EntityUid uid, HypnotizedEmpireMemberRoleComponent comp, ref GetBriefingEvent args)
    {
        if (!TryComp<MindComponent>(uid, out var mind) || mind.OwnedEntity == null)
            return;

        args.Append(Loc.GetString("hypnosis-empire-briefing"));
    }
}
