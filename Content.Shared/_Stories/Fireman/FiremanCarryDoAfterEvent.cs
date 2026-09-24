using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Fireman;

[Serializable, NetSerializable]
public sealed partial class FiremanCarryDoAfterEvent : SimpleDoAfterEvent;
