using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Conversion;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ConversionableComponent : Component
{
    [DataField("allowed", required: true)]
    public List<string> AllowedConversions = new();

    [DataField("active")]
    [AutoNetworkedField]
    public Dictionary<string, ConversionData> ActiveConversions = new();
}
public sealed class ConvertAttemptEvent(EntityUid target, EntityUid? performer, ConversionPrototype prototype) : CancellableEntityEventArgs
{
    public readonly EntityUid Target = target;
    public readonly EntityUid? Performer = performer;
    public readonly ConversionPrototype Prototype = prototype;
}
public sealed class RevertAttemptEvent(EntityUid target, EntityUid? performer, ConversionPrototype prototype) : CancellableEntityEventArgs
{
    public readonly EntityUid Target = target;
    public readonly EntityUid? Performer = performer;
    public readonly ConversionPrototype Prototype = prototype;
}
public sealed class ConvertedEvent(EntityUid target, EntityUid? performer, ConversionData data) : HandledEntityEventArgs
{
    public readonly EntityUid Target = target;
    public readonly EntityUid? Performer = performer;
    public readonly ConversionData Data = data;
}
public sealed class RevertedEvent(EntityUid target, EntityUid? performer, ConversionData data) : HandledEntityEventArgs
{
    public readonly EntityUid Target = target;
    public readonly EntityUid? Performer = performer;
    public readonly ConversionData Data = data;
}
