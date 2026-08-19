using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.LightSensitivity;

[RegisterComponent]
public sealed partial class LightSensitivityDamageComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier DamageOnLight = default!;

    [DataField]
    public DamageSpecifier? HealInDarkness;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

    [ViewVariables]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public ProtoId<AlertPrototype>? LightAlert;

    [DataField]
    public float LightSpeedMultiplier = 1f;

    [DataField]
    public float DarkSpeedMultiplier = 1f;

    [ViewVariables]
    public bool? WasInDarkness;
}
