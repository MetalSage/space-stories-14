namespace Content.Shared._Stories.PullTo;

[RegisterComponent] [AutoGenerateComponentState]
[Access(typeof(PullToSystem))]
public sealed partial class PulledToComponent : Component
{
    [DataField("slot")] [ViewVariables(VVAccess.ReadWrite)]
    public string Slot = "none";

    [DataField("pulledTo")] [AutoNetworkedField]
    public EntityUid? PulledTo { get; set; }

    [DataField("strength")]
    public float Strength { get; set; } = 10f;

    [DataField("duration")]
    public float? Duration { get; set; }

    [DataField("interval")]
    public float Interval { get; set; } = 0.5f;

    public float ActiveInterval { get; set; } = 0f;

    [DataField("onEnter")] [ViewVariables(VVAccess.ReadWrite)]
    public PulledToOnEnter OnEnter { get; set; } = PulledToOnEnter.None;
}

public enum PulledToOnEnter : byte
{
    None,
    PickUp,
    Equip,
}

public sealed class PulledToTimeOutEvent : HandledEntityEventArgs
{
    public readonly PulledToComponent Component;
    public readonly EntityUid EntityUid;

    public PulledToTimeOutEvent(EntityUid uid, PulledToComponent component)
    {
        EntityUid = uid;
        Component = component;
    }
}
