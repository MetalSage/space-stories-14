using Content.Server.StationEvents.Components;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Content.Shared.Radiation.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Item;

namespace Content.Server.StationEvents.Events;

public sealed class RadiationOutburst : StationEventSystem<RadiationOutburstRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Started(EntityUid uid, RadiationOutburstRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        if (!TryGetRandomStation(out var station))
            return;

        var targetList = new List<Entity<ItemComponent>>();
        var query = EntityQueryEnumerator<ItemComponent, TransformComponent>();
        while (query.MoveNext(out var targetUid, out var target, out var xform))
        {
            if (StationSystem.GetOwningStation(targetUid, xform) != station)
                continue;

            targetList.Add((targetUid, target));
        }

        RobustRandom.Shuffle(targetList);

        var currentSeverity = component.severity;
        var Rads = 0;
        foreach (var target in targetList)
        {
            Rads = _random.Next(0, component.maxSeverity); // Либо меньше предметов с большей радиоактивностью или наоборот
            currentSeverity -= Rads;
            if (currentSeverity <= 0)
                break;

            var radiationComp = EnsureComp<RadiationSourceComponent>(target);
            radiationComp.Intensity = Rads;
        }

        ChatSystem.DispatchStationAnnouncement(
            station.Value,
            Loc.GetString("station-event-radiation-outburst-announcement"),
            playDefaultSound: false,
            colorOverride: Color.Gold
        );
    }
}