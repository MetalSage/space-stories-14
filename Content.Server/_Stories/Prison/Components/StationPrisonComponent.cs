using Content.Shared.Maps;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Prison;

[DataDefinition]
public partial struct PrisonJobRequirementGroup
{
    [DataField("name")]
    public string Name = string.Empty;

    [DataField("jobs", required: true)]
    public HashSet<ProtoId<JobPrototype>> Jobs = new();

    [DataField("min")]
    public int Min = 1;

    public PrisonJobRequirementGroup()
    {
    }
}

[RegisterComponent]
public sealed partial class StationPrisonComponent : Component
{
    [DataField]
    public ProtoId<GameMapPrototype> GameMap = "STPrison";

    [DataField]
    public int MinTotalPlayers = 4;

    [DataField]
    public EntityUid? Prison;

    [DataField]
    public List<PrisonJobRequirementGroup> RequirementGroups = new()
    {
        new PrisonJobRequirementGroup
        {
            Name = "security",
            Min = 2,
            Jobs = new HashSet<ProtoId<JobPrototype>> { "STPRISONHeadOfPrison", "STPRISONOfficer" },
        },
        new PrisonJobRequirementGroup
        {
            Name = "prisoners",
            Min = 2,
            Jobs = new HashSet<ProtoId<JobPrototype>> { "STPRISONPrisoner" },
        },
    };
}
