using Content.Server.StationEvents.Components;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;
using Content.Shared.Radiation.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Tag;
using Content.Shared.Construction.Components;

namespace Content.Server.StationEvents.Events;

public sealed class RadiationOutburstRule : StationEventSystem<RadiationOutburstRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly SharedContainerSystem _containerSystem = default!;
	[Dependency] private readonly TagSystem _tagSystem = default!;
	
	private EntityQuery<MobStateComponent> _mobStateQuery;
	private static readonly ProtoId<TagPrototype> HighRiskItemTag = "HighRiskItem";
	
	public override void Initialize()
	{
        base.Initialize();
		_mobStateQuery = GetEntityQuery<MobStateComponent>();
	}

    protected override void Started(EntityUid uid, RadiationOutburstRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        if (!TryGetRandomStation(out var station))
            return;
		
		MobStateComponent? mobState = null;

        var targetList = new List<Entity<ItemComponent>>();
        var query = EntityQueryEnumerator<ItemComponent, TransformComponent>();
        while (query.MoveNext(out var targetUid, out var target, out var xform))
        {
            // Проверки

            if (StationSystem.GetOwningStation(targetUid, xform) != station) // На выбранной ли станции объект
                continue;

            if (_containerSystem.TryFindComponentOnEntityContainerOrParent(targetUid, _mobStateQuery, ref mobState)) // Не относится ли объект к живому существу
                continue;
            if (_tagSystem.HasTag(targetUid, HighRiskItemTag)) // Не является ли объект хайриском
                continue;
            if (EntityManager.HasComponent<AnchorableComponent>(targetUid)) // Нельзя ли прикрутить этот объект (Станционные маяки, трубы и т.п.)
                continue;

            targetList.Add((targetUid, target));
        }

        RobustRandom.Shuffle(targetList);

        var currentSeverity = component.severity;
        var Rads = 0;
        foreach (var target in targetList)
        {
            Rads = _random.Next(1, component.maxSeverity); // Либо меньше предметов с большей радиоактивностью или наоборот
            currentSeverity -= Rads;
            if (currentSeverity <= 0)
                break;

            var radiationComp = EnsureComp<RadiationSourceComponent>(target);
            radiationComp.Intensity = Rads;
            Log.Debug($"Irradiating {target.Owner.Id} with {Rads} severity.");
        }

        ChatSystem.DispatchStationAnnouncement(
            station.Value,
            Loc.GetString("station-event-radiation-outburst-announcement"),
            playDefaultSound: false,
            colorOverride: Color.Gold
        );
    }
}