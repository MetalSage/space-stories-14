using System.Linq;
using Content.Server.Fax;
using Content.Shared.Fax.Components;
using Content.Shared.GameTicking;
using Content.Shared.Paper;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Stories.StationGoal;

/// <summary>
/// System to spawn paper with station goal.
/// </summary>
public sealed partial class StationGoalPaperSystem : EntitySystem
{
    [Dependency] private FaxSystem _faxSystem = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
    }

    private void OnRoundStarted(RoundStartedEvent ev)
    {
        SendRandomGoal();
    }

    public void SendRandomGoal()
    {
        var availableGoals = _prototypeManager.EnumeratePrototypes<StationGoalPrototype>().ToList();

        availableGoals.RemoveAll(IsNotEnoughPlayers);

        if (availableGoals.Count == 0)
            return;

        var goal = _random.Pick(availableGoals);
        TrySendStationGoal(goal);
    }

    /// <summary>
    /// Send a station goal to all faxes which are authorized to receive it.
    /// </summary>
    /// <returns>True if at least one fax received paper</returns>
    private bool IsNotEnoughPlayers(StationGoalPrototype checkGoal)
    {
        return _playerManager.PlayerCount < checkGoal.OnlineLess;
    }

    public bool TrySendStationGoal(StationGoalPrototype goal)
    {
        var wasSent = false;
        var query = EntityQueryEnumerator<FaxMachineComponent>();
        while (query.MoveNext(out var uid, out var fax))
        {
            if (!fax.ReceiveStationGoal)
                continue;

            var printout = new FaxPrintout(
                Loc.GetString(goal.Text),
                Loc.GetString("station-goal-fax-paper-name"),
                null,
                null,
                "paper_stamp-centcom",
                new List<StampDisplayInfo>
                {
                    new()
                    {
                        StampedName = Loc.GetString("stamp-component-stamped-name-centcom"),
                        StampedColor = Color.FromHex("#006600"),
                    },
                });
            _faxSystem.Receive(uid, printout, null, fax);

            wasSent = true;
        }

        return wasSent;
    }
}
