using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Shadowling;

[RegisterComponent] [NetworkedComponent]
public sealed partial class ShadowlingComponent : Component
{
    [DataField]
    public Dictionary<string, int> Actions = new();

    [DataField]
    public Dictionary<string, EntityUid> GrantedActions = new();

    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "ShadowlingFaction";
}

//
// Actions
//
public sealed partial class ShadowlingHatchEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingEnthrallEvent : EntityTargetActionEvent
{
}

public sealed partial class ShadowlingGlareEvent : EntityTargetActionEvent
{
}

public sealed partial class ShadowlingVeilEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingCollectiveMindEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingRapidReHatchEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingSonicScreechEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingBlindnessSmokeEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingBlackRecuperationEvent : EntityTargetActionEvent
{
}

public sealed partial class ShadowlingAnnihilateEvent : EntityTargetActionEvent
{
}

public sealed partial class ShadowlingLightningStormEvent : InstantActionEvent
{
}

public sealed partial class ShadowlingAscendanceEvent : InstantActionEvent
{
}

//
// Do Afters
//
[Serializable] [NetSerializable]
public sealed partial class ShadowlingHatchDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable] [NetSerializable]
public sealed partial class ShadowlingAscendanceDoAfterEvent : SimpleDoAfterEvent
{
}

//
// Gamerule
//

public sealed class ShadowlingWorldAscendanceEvent : EntityEventArgs
{
    public EntityUid EntityUid;
}
