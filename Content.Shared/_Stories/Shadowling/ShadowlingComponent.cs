using Robust.Shared.GameStates;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared.Actions;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;
using Content.Shared.StatusIcon;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "ShadowlingFaction";

    [DataField]
    public Dictionary<string, int> Actions = new();

    [DataField]
    public Dictionary<string, EntityUid> GrantedActions = new();
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
[Serializable, NetSerializable]
public sealed partial class ShadowlingHatchDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class ShadowlingAscendanceDoAfterEvent : SimpleDoAfterEvent
{
}

//
// Gamerule
//

public sealed partial class ShadowlingWorldAscendanceEvent : EntityEventArgs
{
    public EntityUid EntityUid;
}
