using System.Linq;
using Content.Server._Stories.DiscordAuth;
using Content.Server.Connection;
using Content.Shared._Stories.JoinQueue;
using Content.Shared._Stories.SCCVars;
using Content.Shared.CCVar;
using Prometheus;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Stories.JoinQueue;

public sealed partial class JoinQueueManager
{
    private static readonly Gauge QueueCount = Metrics.CreateGauge(
        "join_queue_count",
        "Amount of players in queue.");

    private static readonly Counter QueueBypassCount = Metrics.CreateCounter(
        "join_queue_bypass_count",
        "Amount of players who bypassed queue by privileges.");

    private static readonly Histogram QueueTimings = Metrics.CreateHistogram(
        "join_queue_timings",
        "Timings of players in queue",
        new HistogramConfiguration
        {
            LabelNames = new[] { "type" },
            Buckets = Histogram.ExponentialBuckets(1, 2, 14),
        });

    private readonly List<ICommonSession> _queue = new();

    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IConnectionManager _connectionManager = default!;
    [Dependency] private DiscordAuthManager _discordAuthManager = default!;

    private bool _isEnabled;
    [Dependency] private IServerNetManager _netManager = default!;

    [Dependency] private IPlayerManager _playerManager = default!;

    public int PlayerInQueueCount => _queue.Count;

    public int ActualPlayersCount =>
        _playerManager.PlayerCount -
        PlayerInQueueCount;

    public void Initialize()
    {
        _netManager.RegisterNetMessage<MsgQueueUpdate>();

        _cfg.OnValueChanged(SCCVars.QueueEnabled, OnQueueCVarChanged, true);
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
        _discordAuthManager.PlayerVerified += OnPlayerVerified;
    }

    private void OnQueueCVarChanged(bool value)
    {
        _isEnabled = value;

        if (!value)
        {
            foreach (var session in _queue)
            {
                session.Channel.Disconnect("Queue was disabled");
            }
        }
    }

    private async void OnPlayerVerified(object? sender, ICommonSession session)
    {
        if (!_isEnabled)
        {
            SendToGame(session);
            return;
        }

        var isPrivileged = await _connectionManager.HavePrivilegedJoin(session.UserId);
        var currentOnline =
            _playerManager.PlayerCount -
            1;
        var haveFreeSlot = currentOnline < _cfg.GetCVar(CCVars.SoftMaxPlayers);
        if (isPrivileged || haveFreeSlot)
        {
            SendToGame(session);

            if (isPrivileged && !haveFreeSlot)
                QueueBypassCount.Inc();

            return;
        }

        _queue.Add(session);
        ProcessQueue(false, session.ConnectedTime);
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Disconnected)
        {
            var wasInQueue = _queue.Remove(e.Session);

            if (!wasInQueue &&
                e.OldStatus !=
                SessionStatus.InGame)
                return;

            ProcessQueue(true, e.Session.ConnectedTime);

            if (wasInQueue)
                QueueTimings.WithLabels("Unwaited").Observe((DateTime.UtcNow - e.Session.ConnectedTime).TotalSeconds);
        }
    }

    private void ProcessQueue(bool isDisconnect, DateTime connectedTime)
    {
        var players = ActualPlayersCount;
        if (isDisconnect)
            players--;

        var haveFreeSlot = players < _cfg.GetCVar(CCVars.SoftMaxPlayers);
        var queueContains = _queue.Count > 0;
        if (haveFreeSlot && queueContains)
        {
            var session = _queue.First();
            _queue.Remove(session);

            SendToGame(session);

            QueueTimings.WithLabels("Waited").Observe((DateTime.UtcNow - connectedTime).TotalSeconds);
        }

        SendUpdateMessages();
        QueueCount.Set(_queue.Count);
    }

    private void SendUpdateMessages()
    {
        for (var i = 0; i < _queue.Count; i++)
        {
            _queue[i]
                .Channel.SendMessage(new MsgQueueUpdate
                {
                    Total = _queue.Count,
                    Position = i + 1,
                });
        }
    }

    private void SendToGame(ICommonSession s)
    {
        Timer.Spawn(0, () => _playerManager.JoinGame(s));
    }
}
