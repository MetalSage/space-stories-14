using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class DemonImmunitiesComponent : Component
{
    [DataField]
    public bool SmashWalls = true;

    [DataField]
    public TimeSpan SmashInterval = TimeSpan.FromSeconds(0.5);

    [ViewVariables]
    public TimeSpan NextSmash = TimeSpan.Zero;

    [DataField(required: true)]
    public DamageSpecifier SmashDamage = default!;

    [DataField]
    public TimeSpan? VanishDelay = TimeSpan.FromSeconds(3);

    [DataField]
    public EntProtoId? HeartPrototype;
}
