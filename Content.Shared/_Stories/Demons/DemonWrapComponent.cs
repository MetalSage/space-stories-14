using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class DemonWrapComponent : Component
{
    [DataField]
    public TimeSpan WrapDuration = TimeSpan.FromSeconds(4);

    [DataField(required: true)]
    public EntProtoId CocoonPrototype;

    [DataField]
    public bool IsWrapping;
}
