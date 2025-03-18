using System.Linq;
using Content.Server._Stories.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.StationEvents;
using Content.Server.StationEvents.Components;
using Content.Shared._Stories.Partners;
using Content.Shared.Ghost;
using Content.Shared.Prototypes;
using Content.Shared.RatKing;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Partners.Systems;
public sealed class SpecialRolesSystem : EntitySystem
{
    private const string DefaultRevsRule = "Revolutionary";
    private const string DefaultThiefRule = "Thief";
    private const string DefaultTraitorRule = "Traitor";
    private const string DefaultShadowlingRule = "Shadowling";
    private const string GreenshiftGamePreset = "Greenshift";
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PartnersManager _partners = default!;

    public bool CanPick(ICommonSession session, ProtoId<SpecialRolePrototype> proto, out StatusLabel? reason)
    {
        if (!_proto.TryIndex(proto, out var prototype))
        {
            reason = StatusLabel.Error;
            return false;
        }

        if (!(session.AttachedEntity is { } uid))
        {
            reason = StatusLabel.NotInGame;
            return false;
        }

        // Partners

        if (!_partners.TryGetInfo(session.UserId, out var data)) // Используем PartnersManager
        {
            reason = StatusLabel.NotPartner;
            return false;
        }

        if (!data.AllowedAntags.ToHashSet().Contains(proto))
        {
            reason = StatusLabel.PartnerNotAllowedProto;
            return false;
        }

        // Проверка токенов
        if (data.Tokens <= 0) // Проверяем токены
        {
            reason = StatusLabel.NoTokens;
            return false;
        }

        // Mind

        if (!_mind.TryGetMind(uid, out var mindId, out _))
        {
            reason = StatusLabel.Error;
            return false;
        }

        if (_role.MindIsAntagonist(mindId))
        {
            reason = StatusLabel.AlreadyAntag;
            return false;
        }

        if (_job.MindTryGetJob(mindId, out var job) && !job.CanBeAntag)
        {
            reason = StatusLabel.CantBeAntag;
            return false;
        }

        // Rules

        if (!_event.EventsEnabled)
        {
            reason = StatusLabel.EventsDisabled;
            return false;
        }

        if (!_proto.TryIndex(prototype.GameRule, out var gameRuleProto))
        {
            reason = StatusLabel.Error;
            return false;
        }

        if (gameRuleProto.HasComponent<StationEventComponent>() && !_event.AvailableEvents().ContainsKey(gameRuleProto))
        {
            reason = StatusLabel.NotInAvailableEvents;
            return false;
        }

        foreach (var (_, rule) in _gameTicker.AllPreviousGameRules)
        {
            if (prototype.GameRulesBlacklist.Contains(rule))
            {
                reason = StatusLabel.GameRulesBlacklist;
                return false;
            }
        }

        // Status

        if (prototype.State != PlayerState.None && prototype.State != GetPlayerState(uid))
        {
            reason = StatusLabel.WrongPlayerState;
            return false;
        }

        // Greenshift

        if (_gameTicker.CurrentPreset?.ID == GreenshiftGamePreset)
        {
            reason = StatusLabel.Greenshift;
            return false;
        }

        reason = null;
        return true;
    }

    public async void Pick(ICommonSession session, ProtoId<SpecialRolePrototype> proto)
    {
        if (!_proto.TryIndex(proto, out var prototype))
            return;

        if (!(session.AttachedEntity is { } uid))
            return;

        if (!_mind.TryGetMind(uid, out var mindId, out _))
            return;

        if (_role.MindIsAntagonist(mindId))
            return;

        switch (prototype.GameRule)
        {
            case DefaultTraitorRule:
                _antag.ForceMakeAntag<TraitorRuleComponent>(session, prototype.GameRule);
                break;
            case DefaultRevsRule:
                _antag.ForceMakeAntag<RevolutionaryRuleComponent>(session, prototype.GameRule);
                break;
            case DefaultShadowlingRule:
                _antag.ForceMakeAntag<ShadowlingRuleComponent>(session, prototype.GameRule);
                break;
            case DefaultThiefRule:
                _antag.ForceMakeAntag<ThiefRuleComponent>(session, prototype.GameRule);
                break;
            default:
                _antag.ForceMakeAntag<RatKingComponent>(session, prototype.GameRule);
                break;
        }

        await _partners.DeductToken(session.UserId);
    }

    public PlayerState GetPlayerState(EntityUid uid)
    {
        if (HasComp<GhostComponent>(uid))
            return PlayerState.Ghost;

        if (_mind.TryGetMind(uid, out var mindId, out _) && _job.MindTryGetJob(mindId, out _))
            return PlayerState.CrewMember;

        return PlayerState.None;
    }
}
