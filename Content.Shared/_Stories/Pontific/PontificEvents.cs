using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Pontific;

public sealed partial class PontificPrayerEvent : InstantActionEvent
{

    [DataField]
    public SoundSpecifier? PrayerSound = new SoundPathSpecifier("/Audio/_Stories/Pontific/pontific-prayer.ogg");

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(45);

}

public sealed partial class PontificFlameSwordsEvent : InstantActionEvent
{

    [DataField]
    public float DamageMultiplier = 1.75f;

    [DataField]
    public float SpeedMultiplier = 1.25f;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(45);

}

public sealed partial class CreateEntityEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId Proto { get; set; }
}
