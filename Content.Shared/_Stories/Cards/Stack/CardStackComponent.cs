using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using Robust.Shared.Containers;

namespace Content.Shared._Stories.Cards.Stack;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CardStackComponent : Component
{
    [DataField("content")]
    public List<EntProtoId> InitialContent = [];

    [DataField, AutoNetworkedField]
    public List<EntityUid> Cards = [];

    [ViewVariables]
    public Container CardContainer = default!;

    [DataField("addCardSound")]
    public SoundSpecifier AddCard = new SoundCollectionSpecifier("STAddCard");

    [DataField("removeCardSound")]
    public SoundSpecifier RemoveCard = new SoundCollectionSpecifier("STRemoveCard");
}

[Serializable, NetSerializable]
public enum CardStackVisuals : byte
{
    CardsCount,
    Shuffled
}
