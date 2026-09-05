using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Demons;

[Serializable, NetSerializable]
public sealed partial class SlaughterDemonConsumeDoAfterEvent : SimpleDoAfterEvent;
