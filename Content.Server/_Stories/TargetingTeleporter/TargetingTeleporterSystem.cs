using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Server.Actions;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._Stories.TargetingTeleporter;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Station;
using Content.Shared.Teleportation.Components;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._Stories.TargetingTeleporter;

public sealed class TargetingTeleporterSystem : SharedTargetingTeleporterSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        SubscribeLocalEvent<TargetingTeleporterComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<TargetingTeleporterComponent> entity, ref ComponentInit args)
    {
        // Станции еще не заспавнились
        if (_gameTicker.RunLevel == GameRunLevel.PreRoundLobby)
            return;

        var query = EntityQueryEnumerator<TargetingTeleporterComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            component.GridUid = GetEyeSpawnPoint((uid, component));
            Dirty((Entity<TargetingTeleporterComponent>)(uid, component));
        }
    }

    private void OnRoundStarted(RoundStartedEvent args)
    {
        var query = EntityQueryEnumerator<TargetingTeleporterComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (component.GridUid != null)
                continue;

            component.GridUid = GetEyeSpawnPoint((uid, component));
            Dirty((Entity<TargetingTeleporterComponent>)(uid, component));
        }
    }

    private EntityUid? GetEyeSpawnPoint(Entity<TargetingTeleporterComponent> entity)
    {

        var stations = _station.GetStationsSet();
        var neededStations = new HashSet<EntityUid>();

        foreach (var station in stations)
        {
            if (_entityWhitelist.IsWhitelistFail(entity.Comp.StationWhitelist, station))
                continue;

            if (_entityWhitelist.IsBlacklistPass(entity.Comp.StationBlacklist, station))
                continue;

            neededStations.Add(station);
        }

        var stationUid = _random.Pick(neededStations);
        var grid = _station.GetLargestGrid(Comp<StationDataComponent>(stationUid));

        if (grid is { } gridUid)
            return gridUid;
        else
            return null;
    }
}
