using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class DemonHeartComponent : Component
{
    [DataField(required: true)]
    public EntProtoId GrantedAction;

    [DataField]
    public ComponentRegistry Components = new();

    [DataField]
    public LocId ConsumedMessage = "demon-heart-consumed";

    [DataField]
    public LocId AlreadyHasAbilityMessage = "demon-heart-already";
}
