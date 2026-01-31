using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Cards.Deck;

[RegisterComponent]
public sealed partial class CardDeckComponent : Component
{
    [DataField] [ViewVariables(VVAccess.ReadWrite)]
    public int MaxCards = 5;

    [DataField] [ViewVariables(VVAccess.ReadWrite)]
    public float Offset = 0.02f;

    [DataField("shuffleSound")]
    public SoundSpecifier ShuffleSound = new SoundCollectionSpecifier("STShuffleDeck");
}

[Serializable] [NetSerializable]
public enum CardDeckVisuals : byte
{
    InBox,
}
