using System.Linq;
using Content.Server._Stories.Conversion;
using Content.Server._Stories.GameTicking.Rules.Components;
using Content.Server._Stories.Shadowling;
using Content.Server.AlertLevel;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.RoundEnd;
using Content.Server.Station.Systems;
using Content.Shared._Stories.Conversion;
using Content.Shared._Stories.Shadowling;
using Content.Shared.GameTicking.Components;
using Robust.Server.Audio;
using Robust.Shared.Player;

namespace Content.Server._Stories.GameTicking.Rules;

public sealed partial class ShadowlingRuleSystem : GameRuleSystem<ShadowlingRuleComponent>
{
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    [Dependency] private AlertLevelSystem _alertLevel = default!;
    [Dependency] private StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingWorldAscendanceEvent>(OnWorldAscendance);
        SubscribeLocalEvent<ShadowlingHalfwayEvent>(OnHalfway);
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

    private void OnHalfway(ShadowlingHalfwayEvent args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var ruleUid, out _, out var rule, out _))
        {
            if (rule.HalfwayWarningSent)
                continue;

            rule.HalfwayWarningSent = true;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("stories-shadowling-halfway-warning"), null, true, null, Color.Red);

            foreach (var station in _station.GetStations())
            {
                _alertLevel.SetLevel(station, "gamma", true, true, true, false);
            }
        }
    }

    private void OnWorldAscendance(ShadowlingWorldAscendanceEvent args)
    {
        var query = QueryActiveRules();
        if (query.MoveNext(out var uid, out _, out var component, out _))
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

        var winText = Loc.GetString($"stories-shadowling-{component.WinType.ToString().ToLower()}");
        args.AddLine(winText);

        var sessionData = _antag.GetAntagIdentifiers(uid).ToList();
        args.AddLine(Loc.GetString("stories-shadowling-count", ("initialCount", sessionData.Count)));

        foreach (var (_, data, name) in sessionData)
        {
            args.AddLine(Loc.GetString("stories-shadowling-list-name-user", ("name", name), ("user", data.UserName)));
        }

        args.AddLine("\n");
    }
}
