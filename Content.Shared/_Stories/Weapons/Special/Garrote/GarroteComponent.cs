using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Weapons.Special.Garrote;

[RegisterComponent]
public sealed partial class GarroteComponent : Component
{
    [DataField("checkDirection")]
    public bool CheckDirection = true;

    [DataField("damage")]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2>
        {
            { "Asphyxiation", 5 },
        },
    };

    private TimeSpan doAfterTime = TimeSpan.FromSeconds(0.5f);
    public TimeSpan DurationStatusEffects = TimeSpan.FromSeconds(1f);

    [DataField("maxUseDistance")]
    public float MaxUseDistance = 0.8f;

    [DataField("doAfterTime")]
    public TimeSpan DoAfterTime
    {
        get => doAfterTime;
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
}
