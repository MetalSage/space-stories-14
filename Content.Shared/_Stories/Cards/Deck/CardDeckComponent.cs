using Robust.Shared.Serialization;
using Robust.Shared.Audio;

namespace Content.Shared._Stories.Cards.Deck;

[RegisterComponent]
public sealed partial class CardDeckComponent : Component
{
    [DataField("shuffleSound")]
    public SoundSpecifier ShuffleSound = new SoundCollectionSpecifier("STShuffleDeck");

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Offset = 0.02f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxCards = 5;
}

[Serializable, NetSerializable]
public enum CardDeckVisuals : byte
{
    InBox
}
