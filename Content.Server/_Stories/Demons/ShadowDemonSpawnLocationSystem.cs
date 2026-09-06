using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server._Stories.Demons;

public sealed partial class ShadowDemonSpawnLocationSystem : GameRuleSystem<ShadowDemonSpawnLocationComponent>
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowDemonSpawnLocationComponent, AntagSelectLocationEvent>(OnSelectLocation);
    }

    protected override void Added(EntityUid uid, ShadowDemonSpawnLocationComponent comp, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, comp, gameRule, args);

        EntityCoordinates? lastAttempt = null;

        for (var i = 0; i < comp.Attempts; i++)
        {
            if (!TryFindRandomTile(out _, out _, out _, out var coords))
                continue;

            lastAttempt = coords;

            if (IsDark(coords, comp.DarkRadius))
            {
                comp.Coords = coords;
                return;
            }
        }

        comp.Coords = lastAttempt;
    }

    private bool IsDark(EntityCoordinates coords, float radius)
    {
        foreach (var light in _lookup.GetEntitiesInRange<PointLightComponent>(coords, radius))
        {
            if (light.Comp.Enabled)
                return false;
        }

        return true;
    }

    private void OnSelectLocation(Entity<ShadowDemonSpawnLocationComponent> ent, ref AntagSelectLocationEvent args)
    {
        if (ent.Comp.Coords is { } coords)
            args.Coordinates.Add(_transform.ToMapCoordinates(coords));
    }
}
