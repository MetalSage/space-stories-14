using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Components;

[RegisterComponent]
public sealed partial class ProtectiveBubbleUserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ProtectiveBubble;

    [DataField]
    public DamageSpecifier Regeneration = new()
    {
        DamageDict =
        {
            { "Blunt", -2.5f },
            { "Slash", -2.5f },
            { "Piercing", -5f },
            { "Heat", -2.5f },
        },
    };

    [DataField]
    public EntProtoId StopProtectiveBubbleAction = "ActionStopProtectiveBubble";

    [DataField] [AutoNetworkedField]
    public EntityUid? StopProtectiveBubbleActionEntity;

    [DataField]
    public float VolumeCost = 5f;
}
