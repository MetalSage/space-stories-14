using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Cards.Stack;

[RegisterComponent] [NetworkedComponent] [AutoGenerateComponentState]
public sealed partial class CardStackComponent : Component
{
    [DataField]
    public SoundSpecifier AddCardSound = new SoundCollectionSpecifier("STAddCard");

    [ViewVariables]
    public Container CardContainer;

    [ViewVariables] [AutoNetworkedField]
    public List<EntityUid> CardOrder = new();

    [ViewVariables] [DataField("content")]
    public List<EntProtoId> InitialContent = [];

    [ViewVariables(VVAccess.ReadWrite)]
    public int MaxCards = 216;

    [DataField]
    public SoundSpecifier RemoveCardSound = new SoundCollectionSpecifier("STRemoveCard");
}

[Serializable] [NetSerializable]
public enum CardStackVisuals : byte
{
    CardsCount,
    OrderEdited,
}
