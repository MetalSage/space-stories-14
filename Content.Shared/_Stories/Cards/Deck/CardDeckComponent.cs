using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Cards.Deck;

[RegisterComponent]
public sealed partial class CardDeckComponent : Component
{
    [DataField]
    public int MaxCards = 5;

    [DataField]
    public float Offset = 0.02f;

    [DataField]
    public SoundSpecifier ShuffleSound = new SoundCollectionSpecifier("STShuffleDeck");
}

[Serializable] [NetSerializable]
public enum CardDeckVisuals : byte
{
    InBox,
}
