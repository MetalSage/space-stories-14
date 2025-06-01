namespace Content.Shared._Stories.Cards.Stack;

public sealed class CardRemovedEvent : EntityEventArgs
{
    public EntityUid User;
    public EntityUid Card;

    public CardRemovedEvent(EntityUid user, EntityUid card)
    {
        User = user;
        Card = card;
    }
}
