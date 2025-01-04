using Content.Server._Stories.ForceUser.Components;
using Content.Shared.Projectiles;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Content.Shared.Explosion.Components.OnTrigger;
using Content.Server.Explosion.EntitySystems;

namespace Content.Server._Stories.ForceUser.Systems;

public sealed class FrozeBulletsSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FrozeBulletsComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            var ents = _lookup.GetEntitiesInRange<ProjectileComponent>(_xform.GetMapCoordinates(uid), _random.NextFloat(comp.MinRange, comp.MaxRange));
            foreach (var (ent, component) in ents)
            {
                _popup.PopupCoordinates("Остановлено!", Transform(ent).Coordinates);
                if (HasComp<ExplodeOnTriggerComponent>(ent))
                    _trigger.Trigger(ent);
                else QueueDel(ent);
            }
        }
    }
}
