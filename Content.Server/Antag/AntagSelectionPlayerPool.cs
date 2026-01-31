using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Antag;

public sealed class AntagSelectionPlayerPool (List<List<ICommonSession>> orderedPools)
{
    // Stories-Sponsors-Start
    public bool TryPickAndTake(IRobustRandom random, [NotNullWhen(true)] out ICommonSession? session, Func<ICommonSession, float>? weightFunc = null)
    {
        session = null;

        foreach (var pool in orderedPools)
        {
            if (pool.Count == 0)
                continue;

            if (weightFunc == null)
            {
                session = random.PickAndTake(pool);
            }
            else
            {
                var totalWeight = pool.Sum(weightFunc);
                var r = random.NextFloat() * totalWeight;

                for (var i = 0; i < pool.Count; i++)
                {
                    var p = pool[i];
                    r -= weightFunc(p);
                    if (r <= 0)
                    {
                        session = p;
                        pool.RemoveAt(i);
                        break;
                    }
                }

                if (session == null && pool.Count > 0)
                {
                    session = pool[^1];
                    pool.RemoveAt(pool.Count - 1);
                }
            }
            break;
        }

        return session != null;
    }
    // Stories-Sponsors-End

    public int Count => orderedPools.Sum(p => p.Count);
}
