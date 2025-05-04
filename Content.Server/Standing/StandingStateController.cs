using Content.Server.DoAfter;
using Content.Shared.DoAfter;
using Content.Shared.Standing;

namespace Content.Server.Standing;
public sealed class StandingStateController : SharedStandingStateController
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    public override void HandleToggleStandingInput(EntityUid uid)
    {
        if (!TryComp<StandingStateComponent>(uid, out var standing) || !standing.CanCrawl || !standing.CanStandUp)
            return;

        if (standing.Standing)
        {
            var doAfterArgs = new DoAfterArgs(EntityManager, uid, standing.DownDelay, new FellDownEvent.DownDoAfterEvent(), uid)
            {
                BlockDuplicate = true,
                BreakOnDamage = false, // true // Stories-Crawling
            };
            _doAfter.TryStartDoAfter(doAfterArgs);
        }
        else
        {
            var doAfterArgs = new DoAfterArgs(EntityManager, uid, standing.StandDelay, new FellDownEvent.StandDoAfterEvent(), uid)
            {
                BlockDuplicate = true,
                BreakOnDamage = false, // true // Stories-Crawling
            };
            _doAfter.TryStartDoAfter(doAfterArgs);
        }
    }
}
