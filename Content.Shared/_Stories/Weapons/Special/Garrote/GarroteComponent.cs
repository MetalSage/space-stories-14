using Content.Shared.Damage;

namespace Content.Shared._Stories.Weapons.Special.Garrote;

[RegisterComponent]
public sealed partial class GarroteComponent : Component
{
    [DataField("doAfterTime")]
    public TimeSpan DoAfterTime
    {
        get { return doAfterTime; }
        set
        {
            if (value.Seconds <= 0.5f)
            {
                doAfterTime = value;
                DurationStatusEffects = TimeSpan.FromSeconds(1f);
            }
            else
            {
                doAfterTime = value;
                DurationStatusEffects = value.Add(TimeSpan.FromSeconds(0.5f));
            }
        }
    }

    private TimeSpan doAfterTime = TimeSpan.FromSeconds(0.5f);
    public TimeSpan DurationStatusEffects = TimeSpan.FromSeconds(1f);

    [DataField("damage")]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Asphyxiation", 5 }
        }
    };

    [DataField("maxUseDistance")]
    public float MaxUseDistance = 0.5f;

    [DataField("checkDirection")]
    public bool CheckDirection = true;
}
