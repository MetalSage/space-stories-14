

namespace Content.Server._Stories.TriggerOnLand
{

    [RegisterComponent]
    public sealed partial class TriggerOnLandComponent : Component
    {
        [DataField("Chance")] public float Prob = 0f;
    }

}