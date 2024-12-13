using Content.Server._Stories.ForceUser.Components;
using Content.Server.Ghost;

namespace Content.Server._Stories.ForceUser.Systems;
public sealed class PassiveGhostBooSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PassiveGhostBooComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            comp.ActiveSeconds -= frameTime;

            if (comp.ActiveSeconds <= 0)
            {
                comp.ActiveSeconds = comp.Seconds;

                Boo(uid, comp.Range, comp.MaxTargets);
            }
        }
    }
    public void Boo(EntityUid uid, float range, float maxTargets)
    {
        var entities = _lookup.GetEntitiesInRange(uid, range);

        var booCounter = 0;
        foreach (var ent in entities)
        {
            var ghostBoo = new GhostBooEvent();
            RaiseLocalEvent(ent, ghostBoo, true);

            if (ghostBoo.Handled)
                booCounter++;

            if (booCounter >= maxTargets)
                break;
        }
    }
}
