using System.Linq;
using Content.Server.Fax;
using Content.Shared.Fax.Components;
using Content.Shared.GameTicking;
using Content.Shared.Paper;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Stories.StationGoal
{
    /// <summary>
    ///     System to spawn paper with station goal.
    /// </summary>
    public sealed class StationGoalPaperSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly FaxSystem _faxSystem = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;

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

            foreach (var findGoal in availableGoals)
            {
                if (_playerManager.PlayerCount <= findGoal.OnlineLess)
                {
                    TrySendStationGoal(findGoal);
                    return;
                }
            }

            var goal = _random.Pick(availableGoals);
            TrySendStationGoal(goal);
        }

        /// <summary>
        ///     Send a station goal to all faxes which are authorized to receive it.
        /// </summary>
        /// <returns>True if at least one fax received paper</returns>
        public bool TrySendStationGoal(StationGoalPrototype goal)
        {
            var faxes = EntityManager.EntityQuery<FaxMachineComponent>();
            var wasSent = false;
            foreach (var fax in faxes)
            {
                if (!fax.ReceiveStationGoal) continue;

                var printout = new FaxPrintout(
                    Loc.GetString(goal.Text),
                    Loc.GetString("station-goal-fax-paper-name"),
                    null,
                    null,
                    "paper_stamp-centcom",
                    new List<StampDisplayInfo>
                    {
                        new() { StampedName = Loc.GetString("stamp-component-stamped-name-centcom"), StampedColor = Color.FromHex("#006600") },
                    });
                _faxSystem.Receive(fax.Owner, printout, null, fax);

                wasSent = true;
            }

            return wasSent;
        }
    }
}
