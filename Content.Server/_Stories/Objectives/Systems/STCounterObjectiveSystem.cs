using Content.Server._Stories.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server._Stories.Objectives.Systems;

public sealed partial class STCounterObjectiveSystem : EntitySystem
{
    [Dependency] private NumberObjectiveSystem _number = default!;
    [Dependency] private SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<STCounterObjectiveComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, STCounterObjectiveComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var target = _number.GetTarget(uid);

        if (target == 0 || comp.Count >= target)
        {
            args.Progress = 1f;
            return;
        }

        args.Progress = (float) comp.Count / target;
    }

    public void IncrementForMind(EntityUid demon, EntityUid? uniqueTarget = null)
    {
        if (!_mind.TryGetMind(demon, out _, out var mind))
            return;

        foreach (var objective in mind.Objectives)
        {
            if (!TryComp<STCounterObjectiveComponent>(objective, out var comp))
                continue;

            if (uniqueTarget is { } target && !comp.CountedTargets.Add(target))
                continue;

            comp.Count++;
        }
    }
}
