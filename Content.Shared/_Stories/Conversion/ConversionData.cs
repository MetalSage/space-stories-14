using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Stories.Conversion;

[DataDefinition]
[Serializable] [NetSerializable]
public sealed partial class ConversionData
{
    [DataField("endTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? EndTime;

    [DataField]
    public NetEntity? Owner;

    [DataField]
    public ProtoId<ConversionPrototype> Prototype;

    [DataField("startTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan StartTime;
}
