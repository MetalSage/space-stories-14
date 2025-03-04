using System.Diagnostics.CodeAnalysis;
using Content.Server.GameTicking;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Random;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Server.Screens.Components;
using Robust.Shared.Timing;
using Content.Server.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffect;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Mobs.Systems;
using Robust.Server.Player;
using Content.Shared.Mind;
using Content.Server.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;

namespace Content.Server._Stories.Prison;

public sealed partial class PrisonSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    private readonly ProtoId<JobPrototype> _prisonerJobId = "PRISONPrisoner";
    private const string PacifiedKey = "Pacified";
    /// <summary>
    /// Процент сбежавших зеков для их полной победы.
    /// </summary>
    private const float EscapedPrisonersPercent = 0.5f;
    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        _sawmill = Logger.GetSawmill("prison");
        SubscribeLocalEvent<StationPrisonComponent, MapInitEvent>(OnStationInit);
        SubscribeLocalEvent<PrisonerComponent, ComponentInit>(OnPrisonerInit);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
    }

    private void OnPrisonerInit(EntityUid uid, PrisonerComponent component, ComponentInit args)
    {
        // Rooooooooooooooooooooundstart пацифизм на время, чтобы не было РДМ побегов за 5 секунд.
        _statusEffects.TryAddStatusEffect<PacifiedComponent>(uid, PacifiedKey, TimeSpan.FromSeconds(component.PacifiedTime), false);
    }

    private void OnRoundEndText(RoundEndTextAppendEvent args)
    {
        var query = EntityQueryEnumerator<PrisonComponent, StationDataComponent, StationJobsComponent>();
        while (query.MoveNext(out _, out _, out var stationData, out var stationJobs))
        {
            if (!(_station.GetLargestGrid(stationData) is { } prisonUid))
                continue;

            var prisonMapdId = Transform(prisonUid).MapID;

            int roundstartPrisoners = 0;
            int alivePrisoners = 0;
            HashSet<EntityUid> escapedPrisoners = new();


            var queryPrisonersMinds = EntityQueryEnumerator<MindRoleComponent>();
            while (queryPrisonersMinds.MoveNext(out var uid, out var job))
            {
                if (job.JobPrototype == _prisonerJobId)
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
            else if (escapedPrisoners.Count > 0 && escapedPrisoners.Count >= roundstartPrisoners * EscapedPrisonersPercent)
                winString = "prisoner-major";
            else if (escapedPrisoners.Count > 0)
                winString = "prisoner-minor";
            else if (alivePrisoners == roundstartPrisoners)
                winString = "prison-major";
            else winString = "prison-minor";

            args.AddLine(Loc.GetString(winString));
            args.AddLine(Loc.GetString($"{winString}-desc"));

            foreach (var entityUid in escapedPrisoners)
            {
                if (!_mind.TryGetMind(entityUid, out _, out var mind) || mind.OriginalOwnerUserId == null)
                    continue;

                if (!_player.TryGetPlayerData(mind.OriginalOwnerUserId.Value, out var data))
                    continue;
                // nukeops-list-name-user некорректно.
                args.AddLine(Loc.GetString("nukeops-list-name-user", ("name", MetaData(entityUid).EntityName), ("user", data.UserName)));
            }
            args.AddLine("\n");
        }
    }

    private void OnStationInit(EntityUid uid, StationPrisonComponent component, MapInitEvent args)
    {
        var prototype = _prototypeManager.Index(component.GameMap);

        _map.CreateMap(out var mapId, false);
        _gameTicker.LoadGameMap(prototype, out mapId, null);

        var prison = _station.GetStationInMap(mapId);

        if (prison == null)
        {
            _mapManager.DeleteMap(mapId);
            _sawmill.Error("Failed to find prison station");
            return;
        }

        var prisonComp = EnsureComp<PrisonComponent>(prison.Value);
        prisonComp.Station = uid;
        component.Prison = prison;

        _mapManager.DoMapInitialize(mapId);
    }
}
