using System.Linq;
using Content.Server.Objectives.Components;
using Content.Shared._Stories.Empire.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed class HypnosisConditionSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HypnosisConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, HypnosisConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        var hypnosised = EntityQuery<HypnotizedEmpireComponent>();

        args.Progress = hypnosised.Count() / (float)_number.GetTarget(uid);
    }
}
