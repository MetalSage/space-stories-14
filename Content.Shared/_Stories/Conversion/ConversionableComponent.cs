using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Conversion;

[RegisterComponent] [NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ConversionableComponent : Component
{
    [DataField("active")]
    [AutoNetworkedField]
    public Dictionary<string, ConversionData> ActiveConversions = new();

    [DataField("allowed", required: true)]
    public List<string> AllowedConversions = new();
}

public sealed class ConvertAttemptEvent(EntityUid target, EntityUid? performer, ConversionPrototype prototype)
    : CancellableEntityEventArgs
{
    public readonly EntityUid? Performer = performer;
    public readonly ConversionPrototype Prototype = prototype;
    public readonly EntityUid Target = target;
}

public sealed class RevertAttemptEvent(EntityUid target, EntityUid? performer, ConversionPrototype prototype)
    : CancellableEntityEventArgs
{
    public readonly EntityUid? Performer = performer;
    public readonly ConversionPrototype Prototype = prototype;
    public readonly EntityUid Target = target;
}

public sealed class ConvertedEvent(EntityUid target, EntityUid? performer, ConversionData data) : HandledEntityEventArgs
{
    public readonly ConversionData Data = data;
    public readonly EntityUid? Performer = performer;
    public readonly EntityUid Target = target;
}

public sealed class RevertedEvent(EntityUid target, EntityUid? performer, ConversionData data) : HandledEntityEventArgs
{
    public readonly ConversionData Data = data;
    public readonly EntityUid? Performer = performer;
    public readonly EntityUid Target = target;
}
