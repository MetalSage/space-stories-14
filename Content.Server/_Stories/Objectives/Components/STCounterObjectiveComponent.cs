using Content.Server._Stories.Objectives.Systems;

namespace Content.Server._Stories.Objectives.Components;

[RegisterComponent, Access(typeof(STCounterObjectiveSystem))]
public sealed partial class STCounterObjectiveComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int Count;

    [ViewVariables]
    public HashSet<EntityUid> CountedTargets = new();
}
