using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Prison;

[RegisterComponent]
public sealed partial class PrisonComponent : Component
{
    [DataField]
    public HashSet<ProtoId<JobPrototype>> PrisonerJobs = new() { "STPRISONPrisoner" };

    [DataField]
    public EntityUid? Station;
}
