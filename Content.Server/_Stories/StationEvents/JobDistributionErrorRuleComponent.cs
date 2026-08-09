using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.StationEvents;

[RegisterComponent, Access(typeof(JobDistributionErrorRule))]
public sealed partial class JobDistributionErrorRuleComponent : Component
{
    [DataField]
    public List<ProtoId<JobPrototype>> Jobs = new();

    [DataField]
    public int MaxAmount = 3;

    [DataField]
    public int MaxJobs = 2;

    [DataField]
    public int MinAmount = 1;

    [DataField]
    public int MinJobs = 1;
}
