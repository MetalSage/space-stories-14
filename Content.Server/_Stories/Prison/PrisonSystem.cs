using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._Stories.SCCVars;
using Content.Shared.CCVar;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.GameTicking;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Station.Components;
using Content.Shared.StatusEffect;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Prison;

public sealed partial class PrisonSystem : EntitySystem
{
    private const float EscapedPrisonersPercent = 0.5f;

    private static readonly string PacifiedKey = "Pacified";

    [Dependency] private IBanManager _banManager = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IServerPreferencesManager _prefsManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    private ISawmill _sawmill = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        _sawmill = Logger.GetSawmill("prison");
        SubscribeLocalEvent<StationPrisonComponent, MapInitEvent>(OnStationInit);
        SubscribeLocalEvent<PrisonerComponent, ComponentInit>(OnPrisonerInit);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
    }

    private void OnPrisonerInit(EntityUid uid, PrisonerComponent component, ComponentInit args)
    {
        _statusEffects.TryAddStatusEffect<PacifiedComponent>(uid,
            PacifiedKey,
            TimeSpan.FromSeconds(component.PacifiedTime),
            true);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent args)
    {
        var query = EntityQueryEnumerator<PrisonComponent, StationDataComponent, StationJobsComponent>();
        while (query.MoveNext(out var stationUid, out var prisonComp, out _, out _))
        {
            if (!(_station.GetLargestGrid(stationUid) is { } prisonUid))
                continue;

            var prisonMapdId = Transform(prisonUid).MapID;

            var roundstartPrisoners = 0;
            var alivePrisoners = 0;
            HashSet<EntityUid> escapedPrisoners = new();

            var queryPrisonersMinds = EntityQueryEnumerator<MindRoleComponent>();
            while (queryPrisonersMinds.MoveNext(out var uid, out var job))
            {
                if (job.JobPrototype != null && prisonComp.PrisonerJobs.Contains(job.JobPrototype.Value))
                    roundstartPrisoners++;
            }

            if (roundstartPrisoners > 0)
            {
                var queryPrisoners = EntityQueryEnumerator<PrisonerComponent, MobStateComponent, TransformComponent>();
                while (queryPrisoners.MoveNext(out var uid, out var prisoner, out var mobState, out var xform))
                {
                    if (_mobState.IsAlive(uid))
                        alivePrisoners++;

                    if (_mobState.IsAlive(uid) && Transform(uid).MapID != prisonMapdId)
                        escapedPrisoners.Add(uid);
                }
            }

            string winString;

            if (roundstartPrisoners == 0)
                winString = "prison-no-prisoners";
            else if (alivePrisoners == 0)
                winString = "prisoner-dead";
            else if (escapedPrisoners.Count > 0 &&
                     escapedPrisoners.Count >= roundstartPrisoners * EscapedPrisonersPercent)
                winString = "prisoner-major";
            else if (escapedPrisoners.Count > 0)
                winString = "prisoner-minor";
            else if (alivePrisoners == roundstartPrisoners)
                winString = "prison-major";
            else
                winString = "prison-minor";

            args.AddLine(Loc.GetString(winString));
            args.AddLine(Loc.GetString($"{winString}-desc"));

            foreach (var entityUid in escapedPrisoners)
            {
                if (!_mind.TryGetMind(entityUid, out _, out var mind) || mind.OriginalOwnerUserId == null)
                    continue;

                if (!_player.TryGetPlayerData(mind.OriginalOwnerUserId.Value, out var data))
                    continue;
                args.AddLine(Loc.GetString("nukeops-list-name-user",
                    ("name", MetaData(entityUid).EntityName),
                    ("user", data.UserName)));
            }

            args.AddLine("\n");
        }
    }

    private void OnStationInit(EntityUid uid, StationPrisonComponent component, MapInitEvent args)
    {
        if (!_cfg.GetCVar(SCCVars.PrisonEnabled))
        {
            _sawmill.Info("Space prison is disabled by CVar (scc.prison_enabled = false). Skipping prison loading.");
            return;
        }

        if (!_prototypeManager.TryIndex(component.GameMap, out var prototype))
        {
            _sawmill.Error($"Failed to find game map prototype '{component.GameMap}' for prison.");
            return;
        }

        var profiles = GetReadyPlayerProfiles();
        if (!CheckPrisonRequirements(profiles, component, out var failReason))
        {
            _sawmill.Info($"Space prison conditions not met: {failReason}. Space prison will not be loaded.");
            return;
        }

        _map.CreateMap(out var mapId, false);
        _gameTicker.LoadGameMap(prototype, out mapId);

        var prison = _station.GetStationInMap(mapId);

        if (prison == null)
        {
            _map.DeleteMap(mapId);
            _sawmill.Error("Failed to find prison station on loaded map.");
            return;
        }

        _sawmill.Info("Space prison conditions met. Initializing prison map and station.");

        var prisonComp = EnsureComp<PrisonComponent>(prison.Value);
        prisonComp.Station = uid;

        var prisonerGroup = component.RequirementGroups.FirstOrDefault(g =>
            g.Name.Equals("prisoners", StringComparison.OrdinalIgnoreCase));
        if (prisonerGroup.Jobs.Count > 0)
            prisonComp.PrisonerJobs = new HashSet<ProtoId<JobPrototype>>(prisonerGroup.Jobs);

        component.Prison = prison;
        _map.InitializeMap(_map.GetMap(mapId));
    }

    private bool CheckPrisonRequirements(
        Dictionary<NetUserId, HumanoidCharacterProfile> profiles,
        StationPrisonComponent component,
        [NotNullWhen(false)] out string? failReason)
    {
        failReason = null;

        var groupCandidates = new Dictionary<string, HashSet<NetUserId>>();
        var allPrisonCandidates = new HashSet<NetUserId>();

        foreach (var group in component.RequirementGroups)
        {
            groupCandidates[group.Name] = new HashSet<NetUserId>();
        }

        foreach (var (userId, profile) in profiles)
        {
            var roleBans = _banManager.GetJobBans(userId);

            foreach (var group in component.RequirementGroups)
            {
                var isCandidateForGroup = false;

                foreach (var jobId in group.Jobs)
                {
                    if (roleBans != null && roleBans.Contains(jobId))
                        continue;

                    if (profile.JobPriorities.TryGetValue(jobId, out var priority) &&
                        priority > JobPriority.Never)
                    {
                        isCandidateForGroup = true;
                        break;
                    }
                }

                if (isCandidateForGroup)
                {
                    groupCandidates[group.Name].Add(userId);
                    allPrisonCandidates.Add(userId);
                }
            }
        }

        if (allPrisonCandidates.Count < component.MinTotalPlayers)
        {
            failReason =
                $"Total available prison candidates ({allPrisonCandidates.Count}) is less than minimum required ({component.MinTotalPlayers})";
            return false;
        }

        foreach (var group in component.RequirementGroups)
        {
            if (groupCandidates[group.Name].Count < group.Min)
            {
                failReason =
                    $"Not enough candidates for role group '{group.Name}' ({groupCandidates[group.Name].Count} < {group.Min})";
                return false;
            }
        }

        if (!CanSatisfyGroupsDistinctly(groupCandidates, component.RequirementGroups))
        {
            failReason = "Cannot satisfy minimum requirements across all role groups with distinct candidate players.";
            return false;
        }

        return true;
    }

    private bool CanSatisfyGroupsDistinctly(
        Dictionary<string, HashSet<NetUserId>> groupCandidates,
        List<PrisonJobRequirementGroup> groups)
    {
        var requiredList = new List<string>();
        foreach (var g in groups)
        {
            for (var i = 0; i < g.Min; i++)
            {
                requiredList.Add(g.Name);
            }
        }

        return TryAssign(0, new HashSet<NetUserId>(), requiredList, groupCandidates);
    }

    private bool TryAssign(
        int index,
        HashSet<NetUserId> used,
        List<string> requiredList,
        Dictionary<string, HashSet<NetUserId>> groupCandidates)
    {
        if (index >= requiredList.Count)
            return true;

        var groupName = requiredList[index];
        if (!groupCandidates.TryGetValue(groupName, out var candidates))
            return false;

        foreach (var candidate in candidates)
        {
            if (used.Contains(candidate))
                continue;

            used.Add(candidate);
            if (TryAssign(index + 1, used, requiredList, groupCandidates))
                return true;
            used.Remove(candidate);
        }

        return false;
    }

    private Dictionary<NetUserId, HumanoidCharacterProfile> GetReadyPlayerProfiles()
    {
        var readyPlayerProfiles = new Dictionary<NetUserId, HumanoidCharacterProfile>();
        var speciesBlacklist = new HashSet<string>(_cfg.GetCVar(CCVars.ICNewAccountSpeciesBlacklist).Split(","));

        foreach (var (userId, status) in _gameTicker.PlayerGameStatuses)
        {
            if (_gameTicker.LobbyEnabled && status != PlayerGameStatus.ReadyToPlay)
                continue;

            if (!_player.TryGetSessionById(userId, out var session))
                continue;

            HumanoidCharacterProfile profile;
            if (_prefsManager.TryGetCachedPreferences(userId, out var preferences))
                profile = preferences.SelectedCharacter;
            else
                profile = HumanoidCharacterProfile.Random(speciesBlacklist);

            readyPlayerProfiles.Add(userId, profile);
        }

        return readyPlayerProfiles;
    }
}
