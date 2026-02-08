using System.Linq;
using JetBrains.Annotations;
using Content.Server._Stories.Sponsors;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Ghost.Roles.Raffles;

/// <summary>
/// Chooses the winner of a ghost role raffle entirely randomly, without any weighting.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed partial class RngGhostRoleRaffleDecider : IGhostRoleRaffleDecider
{
    public void PickWinner(IEnumerable<ICommonSession> candidates, Func<ICommonSession, bool> tryTakeover)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var sponsorsManager = IoCManager.Resolve<SponsorsManager>(); // Stories-Sponsors

        var choices = candidates.ToList();

        // Stories-Sponsors-Start
        while (choices.Count > 0)
        {
            var totalWeight = choices.Sum(s => sponsorsManager.TryGetInfo(s.UserId, out var info) ? info.GhostRolePriority : 1f);
            var r = random.NextFloat() * totalWeight;
            ICommonSession? winner = null;

            for (var i = 0; i < choices.Count; i++)
            {
                var p = choices[i];
                var weight = sponsorsManager.TryGetInfo(p.UserId, out var info) ? info.GhostRolePriority : 1f;
                r -= weight;
                if (r <= 0)
                {
                    winner = p;
                    break;
                }
            }

            winner ??= choices[^1];
            
            if (tryTakeover(winner))
                return;
            
            choices.Remove(winner);
        }
        // Stories-Sponsors-End
    }
}
