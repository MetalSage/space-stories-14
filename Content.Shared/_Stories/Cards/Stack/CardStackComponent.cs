using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using Robust.Shared.Containers;

namespace Content.Shared._Stories.Cards.Stack;

[RegisterComponent, NetworkedComponent]
public sealed partial class CardStackComponent : Component
{
    [DataField("content")]
    public List<EntProtoId> InitialContent = [];

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
public sealed class CardAddedEvent : EntityEventArgs
{
    public EntityUid User;
    public EntityUid Card;
    public CardAddedEvent(EntityUid user, EntityUid card)
    {
        User = user;
        Card = card;
    }
}
