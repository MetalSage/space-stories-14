using Content.Server._Stories.Objectives.Components;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;

namespace Content.Server._Stories.Objectives.Systems;

public sealed partial class STSlaughterFluffConditionSystem : EntitySystem
{
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<STSlaughterFluffConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnAfterAssign(Entity<STSlaughterFluffConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        if (ent.Comp.Lines.Count == 0)
            return;

        var line = Loc.GetString(_random.Pick(ent.Comp.Lines));
        _metaData.SetEntityDescription(ent, line, args.Meta);
    }
}
