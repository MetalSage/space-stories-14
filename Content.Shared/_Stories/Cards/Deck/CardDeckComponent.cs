using Robust.Shared.Serialization;
using Robust.Shared.Audio;

namespace Content.Shared._Stories.Cards.Deck;

[RegisterComponent]
public sealed partial class CardDeckComponent : Component
{
    [DataField("shuffleSound")]
    public SoundSpecifier ShuffleSound = new SoundCollectionSpecifier("STShuffleDeck");
}

[Serializable, NetSerializable]
public enum CardDeckVisuals : byte
{
    InBox
}
