using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.EUI;
using Content.Server.Flash;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Revolutionary;
using Content.Server.Revolutionary.Components;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Revolutionary.Components;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Cuffs.Components;
using Content.Server._Stories.GameTicking.Rules.Components;
using Content.Shared._Stories.Shadowling;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Robust.Server.Audio;
using Robust.Shared.Player;
using Content.Server.GameTicking;
using Robust.Shared.Console;
using Content.Server.Nuke;
using Content.Server._Stories.Conversion;
using Content.Server._Stories.Shadowling;
using Content.Shared._Stories.Conversion;
using Robust.Shared.Random;
using System.Linq;
using Content.Server.Polymorph.Components;

namespace Content.Server._Stories.GameTicking.Rules;

public sealed class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly EuiManager _euiMan = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly NukeSystem _nuke = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ConversionSystem _conversion = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;

    [ValidatePrototypeId<ConversionPrototype>]
    public const string ShadowlingThrallConversion = "ShadowlingThrall";

    /// <summary>
    /// Шанс того, что слуга просто расконвертируется, вместо передачи.
    /// </summary>
    public const float ShadowlingThrallProbOfLost = 0.5f;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingWorldAscendanceEvent>(OnWorldAscendance);
        SubscribeLocalEvent<ShadowlingComponent, MobStateChangedEvent>(OnMobStateChanged);
    }
    private void OnMobStateChanged(EntityUid uid, ShadowlingComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var ruleUid, out _, out var _, out _))
        {
            // FIXME: Плохой код.

            HashSet<EntityUid> shadowlings = new();
            var shadowlingsQuery = AllEntityQuery<ShadowlingComponent, MobStateComponent>();
            while (shadowlingsQuery.MoveNext(out var shadowlingUid, out _, out var mobState))
                if (_mobState.IsAlive(shadowlingUid, mobState))
                    shadowlings.Add(shadowlingUid);

            shadowlings.ExceptWith(_antag.GetAliveAntags(ruleUid).ToHashSet());

            if (shadowlings.Count == 0)
            {
                foreach (var ent in _conversion.GetEntitiesConvertedBy(uid, ShadowlingThrallConversion))
                {
                    _conversion.TryRevert(ent, ShadowlingThrallConversion);
                }
                continue;
            }

            var shadowling = _random.Pick(shadowlings);

            foreach (var ent in _conversion.GetEntitiesConvertedBy(uid, ShadowlingThrallConversion))
            {
                if (_random.Prob(ShadowlingThrallProbOfLost))
                    _conversion.TryRevert(ent, ShadowlingThrallConversion);
                else if (_conversion.TryGetConversion(ent, ShadowlingThrallConversion, out var conversion))
                {
                    conversion.Owner = GetNetEntity(shadowling);
                }
            }

            _shadowling.RefreshActions(shadowling);
        }
    }
    private void CheckWin()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var ruleUid, out _, out var comp, out _))
        {
            if (comp.WinType == ShadowlingWinType.Won)
                continue;

            if (!_antag.AnyAliveAntags(ruleUid))
                comp.WinType = ShadowlingWinType.Lost;
            else
                comp.WinType = ShadowlingWinType.Stalemate;
        }
    }
    private void OnWorldAscendance(ShadowlingWorldAscendanceEvent args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var component, out _))
        {
            component.WinType = ShadowlingWinType.Won;

            var announcementString = Loc.GetString(component.AscendanceAnnouncement);
            _chat.DispatchGlobalAnnouncement(announcementString, colorOverride: component.AscendanceAnnouncementColor);
            _audio.PlayGlobal(component.AscendanceGlobalSound, Filter.Broadcast(), true);
            _roundEnd.EndRound(component.RoundEndTime);
        }
    }
    protected override void AppendRoundEndText(EntityUid uid, ShadowlingRuleComponent component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        CheckWin();

        var winText = Loc.GetString($"shadowling-{component.WinType.ToString().ToLower()}");
        args.AddLine(winText);

        var sessionData = _antag.GetAntagIdentifiers(uid);
        args.AddLine(Loc.GetString("shadowling-count", ("initialCount", sessionData.Count)));
        foreach (var (mind, data, name) in sessionData)
        {
            args.AddLine(Loc.GetString("shadowling-list-name-user",
                ("name", name),
                ("user", data.UserName)));
        }

        args.AddLine("\n");
    }
}
