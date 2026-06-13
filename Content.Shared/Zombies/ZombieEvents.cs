using Content.Shared.Actions;
// Stories-Start
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
// Stories-End

namespace Content.Shared.Zombies;

/// <summary>
///     Event that is broadcast whenever an entity is zombified.
///     Used by the zombie gamemode to track total infections.
/// </summary>
[ByRefEvent]
public readonly struct EntityZombifiedEvent
{
    /// <summary>
    ///     The entity that was zombified.
    /// </summary>
    public readonly EntityUid Target;

    public EntityZombifiedEvent(EntityUid target)
    {
        Target = target;
    }
};

/// <summary>
///     Event raised when a player zombifies themself using the "turn" action
/// </summary>
public sealed partial class ZombifySelfActionEvent : InstantActionEvent { };

// Stories-Start
public sealed partial class ZombieLookUpActionEvent : InstantActionEvent
{
    [DataField("range")]
    public float Range = 15;
}

public sealed partial class ZombieRegenerativeSleepEvent : InstantActionEvent
{
    [DataField("duration")]
    public TimeSpan Duration = TimeSpan.FromSeconds(60);

    [DataField("passiveHeal")]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> PassiveHeal = new()
    {
        { "Blunt", -1 },
        { "Slash", -1 },
        { "Piercing", -1 },
        { "Heat", -0.35 },
        { "Shock", -0.35 },
    };
}
// Stories-End
