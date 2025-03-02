using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Cards.Fan
{
    [Serializable, NetSerializable]
    public enum FanVisuals : byte
    {
        /// <summary>
        /// The amount of elements in the stack
        /// </summary>
        Actual,
        /// <summary>
        /// The total amount of elements in the stack. If unspecified, the visualizer assumes
        /// its
        /// </summary>
        MaxCount,
        Hide
    }
}
