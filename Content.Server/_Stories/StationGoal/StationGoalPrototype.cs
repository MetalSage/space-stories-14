using Robust.Shared.Prototypes;

namespace Content.Server._Stories.StationGoal
{
    [Serializable, Prototype("stationGoal")]
    public sealed partial class StationGoalPrototype : IPrototype
    {
        [IdDataField] 
        public string ID { get; private set; } = default!;

        [DataField("text")] 
        public string Text { get; private set; } = string.Empty;

        [DataField("onlineLess")] 
        public int? OnlineLess { get; private set; }
    }
}
