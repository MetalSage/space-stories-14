using Content.Server.Players.PlayTimeTracking;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.GameTicking;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;

namespace Content.Server._Stories.Antigrief;

public sealed partial class NewPlayerAntigriefSystem : EntitySystem
{
    [Dependency] private PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private PacificationSystem _pacification = default!;

    private float _timer;
    private const float CheckInterval = 30f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (ev.Player == null)
            return;

        var playtime = _playTimeTracking.GetOverallPlaytime(ev.Player);
        if (playtime.TotalHours < 2)
        {
            EnsureComp<PacifiedComponent>(ev.Mob);
            _pacification.SetAllowAttackingHostiles(ev.Mob, true);
            EnsureComp<NewPlayerPacifiedComponent>(ev.Mob);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _timer += frameTime;
        if (_timer < CheckInterval)
            return;

        _timer = 0f;

        var query = EntityQueryEnumerator<NewPlayerPacifiedComponent, ActorComponent>();
        while (query.MoveNext(out var uid, out var newPlayerComp, out var actor))
        {
            if (actor.PlayerSession == null)
                continue;

            var playtime = _playTimeTracking.GetOverallPlaytime(actor.PlayerSession);
            if (playtime.TotalHours >= 2)
            {
                RemComp<PacifiedComponent>(uid);
                RemComp<NewPlayerPacifiedComponent>(uid);
            }
        }
    }
}
