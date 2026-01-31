using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Polymorph;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Stories.ForceUser.Actions.Events;

// "Простые" события, которые не нуждаются в Content.Shared._Stories.ForceUser.Components
// ? Возможно стоит их убрать отсюда?

#region Simple

public sealed partial class LightningStrikeEvent : EntityTargetActionEvent
{
    [DataField]
    public EntProtoId Proto = "Lightning";
}

public sealed partial class EmpActionEvent : InstantActionEvent
{
    /// <summary>
    /// How long it disables targets in seconds
    /// </summary>
    [DataField("disableDuration")] [ViewVariables(VVAccess.ReadWrite)]
    public float DisableDuration = 60f;

    /// <summary>
    /// How much energy will be consumed per battery in range
    /// </summary>
    [DataField("energyConsumption")] [ViewVariables(VVAccess.ReadWrite)]
    public float EnergyConsumption;

    [DataField("range")] [ViewVariables(VVAccess.ReadWrite)]
    public float Range = 1.0f;
}

public sealed partial class FreedomActionEvent : InstantActionEvent
{
}

public sealed partial class RejuvenateActionEvent : InstantActionEvent
{
}

public sealed partial class FlashAreaEvent : InstantActionEvent
{
    [DataField("duration")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int FlashDuration { get; set; } = 8000;

    [DataField("range")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Range { get; set; } = 7f;

    [DataField("slowTo")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float SlowTo { get; set; } = 0.5f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("sound")]
    public SoundSpecifier Sound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/flash.ogg");
}

public sealed partial class RecliningPulseEvent : InstantActionEvent
{
    [DataField("stun")]
    public float StunTime { get; set; } = 3f;

    [DataField("strength")]
    public int Strength { get; set; } = 10;

    [DataField("range")]
    public float Range { get; set; } = 10f;
}

public sealed partial class HypnosisTargetActionEvent : EntityTargetActionEvent
{
}

public sealed partial class IgniteTargetActionEvent : EntityTargetActionEvent
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("stackAmount")]
    public float StackAmount = 1f;
}

public sealed partial class ForceDashActionEvent : WorldTargetActionEvent
{
    [DataField("strength")]
    public float Strength = 10;
}

#endregion

#region ForceProtectiveBubble

public sealed partial class CreateProtectiveBubbleEvent : InstantActionEvent
{
    [DataField("proto")]
    public EntProtoId Proto = "EffectProtectiveBubble";
}

public sealed partial class StopProtectiveBubbleEvent : InstantActionEvent
{
}

#endregion

// События работа которых завязана на Content.Shared._Stories.ForceUser.Components

#region ForceUser

public sealed partial class ForceShopActionEvent : InstantActionEvent
{
}

public sealed partial class ForceLookUpActionEvent : InstantActionEvent
{
    [DataField("range")]
    public float Range = 25;
}

public sealed partial class FrozeBulletsActionEvent : InstantActionEvent
{
    [DataField("seconds")]
    public float Seconds = 10;
}

public sealed partial class RecallLightsaberEvent : InstantActionEvent
{
}

public sealed partial class RecallEquipmentsEvent : InstantActionEvent
{
}

public sealed partial class InquisitorPolymorphEvent : InstantActionEvent
{
    /// <summary>
    /// How long the smoke stays for, after it has spread.
    /// </summary>
    [DataField] [ViewVariables(VVAccess.ReadWrite)]
    public float Duration = 10;

    /// <summary>
    /// Smoke entity to spawn.
    /// Defaults to smoke but you can use foam if you want.
    /// </summary>
    [DataField] [ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId SmokePrototype = "Smoke";

    /// <summary>
    /// Solution to add to each smoke cloud.
    /// </summary>
    /// <remarks>
    /// When using repeating trigger this essentially gets multiplied so dont do anything crazy like omnizine or lexorin.
    /// </remarks>
    [DataField] [ViewVariables(VVAccess.ReadWrite)]
    public Solution Solution = new();

    /// <summary>
    /// How much the smoke will spread.
    /// </summary>
    [DataField(required: true)] [ViewVariables(VVAccess.ReadWrite)]
    public int SpreadAmount = 15;

    [DataField("prototype", customTypeSerializer: typeof(PrototypeIdSerializer<PolymorphPrototype>))]
    public string PolymorphPrototype { get; set; } = "InquisitorGhost";
}

public sealed partial class HandTetherGunEvent : InstantActionEvent
{
}

#endregion

#region Strangle

[Serializable] [NetSerializable]
public sealed partial class StrangledEvent : SimpleDoAfterEvent
{
    [ViewVariables]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Asphyxiation", 7.5f },
        },
    };
}

public sealed partial class StrangleTargetEvent : EntityTargetActionEvent
{
    [DataField("doAfterTime")]
    public float DoAfterTime = 0.5f;
}

#endregion

public sealed partial class StealLifeTargetEvent : EntityTargetActionEvent
{
    [DataField("doAfterTime")]
    public float DoAfterTime = 0.5f;
}

[Serializable] [NetSerializable]
public sealed partial class LifeStolenEvent : SimpleDoAfterEvent
{
    [ViewVariables]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Cold", 7.5f },
        },
    };

    [ViewVariables]
    public HashSet<string> HealGroups = new()
    {
        "Brute",
        "Burn",
        "Airloss",
        "Toxin",
    };
}

public interface IForceActionEvent
{
    [DataField("volume")] float Volume { get; set; }

    [DataField("maxDebuff")]
    float MaxDebuff { get; set; } // Если дефафф больше или равен, то не получится применить способность.

    BaseActionEvent? BaseEvent { get; }
}

public sealed partial class InstantForceUserActionEvent : InstantActionEvent, IForceActionEvent
{
    [DataField("event")]
    [NonSerialized]
    public InstantActionEvent? Event;

    [DataField("volume")]
    public float Volume { get; set; }

    [DataField("maxDebuff")]
    public float MaxDebuff { get; set; } = 10f;

    public BaseActionEvent? BaseEvent => Event;
}

public sealed partial class EntityTargetForceUserActionEvent : EntityTargetActionEvent, IForceActionEvent
{
    [DataField("event")]
    [NonSerialized]
    public EntityTargetActionEvent? Event;

    [DataField("volume")]
    public float Volume { get; set; }

    [DataField("maxDebuff")]
    public float MaxDebuff { get; set; } = 10f;

    public BaseActionEvent? BaseEvent => Event;
}

public sealed partial class WorldTargetForceUserActionEvent : WorldTargetActionEvent, IForceActionEvent
{
    [DataField("event")]
    [NonSerialized]
    public WorldTargetActionEvent? Event;

    [DataField("volume")]
    public float Volume { get; set; }

    [DataField("maxDebuff")]
    public float MaxDebuff { get; set; } = 10f;

    public BaseActionEvent? BaseEvent => Event;
}

[Serializable] [NetSerializable]
public sealed partial class LightsaberDetachedEvent : SimpleDoAfterEvent
{
}

[Serializable] [NetSerializable]
public sealed partial class LightsaberConnectedEvent : SimpleDoAfterEvent
{
}

[Serializable] [NetSerializable]
public sealed partial class LightsaberHackedEvent : SimpleDoAfterEvent
{
}
