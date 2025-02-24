using Content.Shared.Damage;

namespace Content.Shared._Stories.Weapons.Special.Garrote;

[RegisterComponent]
public sealed partial class GarroteComponent : Component
{
    [DataField("doAfterTime")]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(0.5f);

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
}
