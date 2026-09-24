using Content.Shared._Stories.Explosion.Components;

namespace Content.Shared._Stories.Explosion;

public sealed partial class StoriesExplosionEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StoriesExplosionEffectComponent, StoriesExplosiveTriggeredEvent>(OnExplosiveTriggered);
    }

    private void OnExplosiveTriggered(Entity<StoriesExplosionEffectComponent> ent, ref StoriesExplosiveTriggeredEvent args)
    {
        if (ent.Comp.ShockWave is { } shockwave)
            SpawnNextToOrDrop(shockwave, ent);

        if (ent.Comp.Explosion is { } explosion)
            SpawnNextToOrDrop(explosion, ent);
    }
}
