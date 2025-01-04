using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Stories.Conversion;

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class ConversionData
{
    [DataField]
    public NetEntity? Owner;

    [DataField("startTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan StartTime;

    [DataField("endTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? EndTime = null;

    [DataField]
    public ProtoId<ConversionPrototype> Prototype;
}
