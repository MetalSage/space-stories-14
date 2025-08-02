using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Pontific;

[Serializable, NetSerializable]
public enum PontificState : byte
{
    Base,
    Flame,
    Prayer
}

[Serializable, NetSerializable]
public enum PontificVisuals : byte
{
    State
}
