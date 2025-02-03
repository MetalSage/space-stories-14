using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Cards;

[Serializable, NetSerializable]
public sealed class ShuffleCardsEvent : EntityEventArgs
{
    public int Entity { get; }

    public ShuffleCardsEvent(int entity)
    {
        Entity = entity;
    }
}
