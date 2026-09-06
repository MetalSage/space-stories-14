using Content.Shared.Damage;

namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class SlaughterDemonComponent : Component
{
    public const string ConsumedContainerId = "slaughter_demon_consumed";

    [DataField]
    public DamageSpecifier ConsumeKillDamage = new()
    {
        DamageDict = new() { { "Blunt", 1000 } },
    };

    [DataField]
    public TimeSpan ConsumeDuration = TimeSpan.FromSeconds(9);

    [DataField(required: true)]
    public DamageSpecifier HealOnConsume = default!;

    [DataField]
    public DamageSpecifier? HealOnMeagreConsume;

    [DataField]
    public TimeSpan BoostDuration = TimeSpan.FromSeconds(6);

    [DataField]
    public float BoostMultiplier = 1.75f;

    [ViewVariables]
    public TimeSpan BoostEndTime;

    [ViewVariables]
    public bool BoostActive;
}
