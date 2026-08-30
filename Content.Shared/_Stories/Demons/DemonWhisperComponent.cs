using Content.Shared.Actions;

namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class DemonWhisperComponent : Component
{
    [DataField]
    public float Range = 7f;

    [DataField]
    public int MaxMessageLength = 256;
}

public sealed partial class DemonWhisperActionEvent : EntityTargetActionEvent;
