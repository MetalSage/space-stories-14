using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared._Stories.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class ProtectiveBubbleGeneratorComponent : Component
{
    [DataField]
    public EntityUid? ProtectiveBubble;

    [DataField("boltType", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string BubbleType = "SmallProtectiveBubbleSyndicate";

    [DataField("selectableTypes", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> SelectableTypes = new();
}
