using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Shadowling;

public sealed partial class ShadowlingHatchEvent : InstantActionEvent;

public sealed partial class ShadowlingEnthrallEvent : EntityTargetActionEvent;

public sealed partial class ShadowlingHypnosisEvent : EntityTargetActionEvent;

public sealed partial class ShadowlingGlareEvent : EntityTargetActionEvent;

public sealed partial class ShadowlingVeilEvent : InstantActionEvent;

public sealed partial class ShadowlingCollectiveMindEvent : InstantActionEvent;

public sealed partial class ShadowlingSonicScreechEvent : InstantActionEvent;

public sealed partial class ShadowlingBlindnessSmokeEvent : InstantActionEvent;

public sealed partial class ShadowlingDrainLifeEvent : InstantActionEvent;

public sealed partial class ShadowlingFlashFreezeEvent : InstantActionEvent;

public sealed partial class ShadowlingGlacialBlastEvent : InstantActionEvent;

public sealed partial class ShadowlingBlackRecuperationEvent : EntityTargetActionEvent;

public sealed partial class ShadowlingAnnihilateEvent : EntityTargetActionEvent;

public sealed partial class ShadowlingAscendanceEvent : InstantActionEvent;

public sealed partial class ShadowlingShadowWalkEvent : InstantActionEvent;

public sealed partial class ShadowlingPlaneShiftEvent : InstantActionEvent;

public sealed partial class ToggleAscendantBroadcastEvent : InstantActionEvent;

public sealed partial class ToggleShadowlingVisionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class ShadowlingHatchDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class ShadowlingEnthrallDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class ShadowlingAscendanceDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class ShadowlingWorldAscendanceEvent : EntityEventArgs
{
    public NetEntity Entity;
}

[Serializable, NetSerializable]
public sealed class ShadowlingHalfwayEvent : EntityEventArgs
{
}

[Serializable, NetSerializable]
public enum ShadowlingThrallVisuals : byte
{
    IsThrall
}
