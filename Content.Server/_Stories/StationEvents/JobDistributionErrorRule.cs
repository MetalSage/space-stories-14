using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;

namespace Content.Server._Stories.StationEvents;

public sealed class JobDistributionErrorRule : StationEventSystem<JobDistributionErrorRuleComponent>
{
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;

    protected override void Started(EntityUid uid,
        JobDistributionErrorRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation, HasComp<StationJobsComponent>))
            return;

        var jobsAdded = RobustRandom.Next(component.MinJobs, component.MaxJobs);

        for (var i = 0; i < jobsAdded; i++)
        {
            var slotsAdded = RobustRandom.Next(component.MinAmount, component.MaxAmount);
            var chosenJob = RobustRandom.PickAndTake(component.Jobs);

            _stationJobs.TryAdjustJobSlot(chosenStation.Value, chosenJob, slotsAdded, true, true);
        }
    }
}
