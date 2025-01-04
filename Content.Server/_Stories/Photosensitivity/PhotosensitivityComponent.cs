using Content.Shared.Damage;

namespace Content.Server._Stories.Photosensitivity;

[RegisterComponent]
public sealed partial class PhotosensitivityComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("enabled")]
    public bool Enabled = true;

    [ViewVariables(VVAccess.ReadWrite), DataField("damage")]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Heat", 1 }
        }
    };


    [ViewVariables(VVAccess.ReadWrite), DataField("damageInSpace")]
    public DamageSpecifier DamageInSpace = new()
    {
        DamageDict = new()
        {
            { "Heat", 5 }
        }
    };

    [DataField("darknessHealing")]
    public DamageSpecifier DarknessHealing = new()
    {
        DamageDict = new()
        {
            { "Blunt", -5 },
            { "Slash", -5 },
            { "Piercing", -5 },
            { "Heat", -5 },
            { "Shock", -5 }
        }
    };
}
