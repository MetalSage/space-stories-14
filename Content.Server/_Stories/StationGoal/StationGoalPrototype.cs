using Robust.Shared.Prototypes;

namespace Content.Server._Stories.StationGoal
{
    [Serializable, Prototype("stationGoal")]
    public sealed class StationGoalPrototype : IPrototype
    {
        [IdDataField] public string ID { get; } = default!;

        [DataField("text")] public string Text { get; set; } = string.Empty;

        [DataField("onlineLess")] public int? OnlineLess { get; set; }
    }
}
