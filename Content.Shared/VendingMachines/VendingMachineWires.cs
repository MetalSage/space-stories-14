using Robust.Shared.Serialization;

namespace Content.Shared.VendingMachines;

[Serializable, NetSerializable]
public enum ContrabandWireKey : byte
{
    StatusKey,
    TimeoutKey
}

[Serializable, NetSerializable]
public enum EjectWireKey : byte
{
    StatusKey
}

// Stories-Start
[Serializable, NetSerializable]
public enum FreeWireKey : byte
{
    StatusKey,
    TimeoutKey
}

[Serializable, NetSerializable]
public enum LogWireKey : byte
{
    StatusKey,
    TimeoutKey
}
// Stories-End
