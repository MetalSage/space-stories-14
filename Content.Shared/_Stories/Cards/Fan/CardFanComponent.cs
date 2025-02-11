using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Cards.Fan;

[RegisterComponent, NetworkedComponent]
public sealed partial class CardFanComponent : Component
{
    public SoundSpecifier ShuffleSound = new SoundPathSpecifier("/Audio/_Stories/Items/Cards/FanShuffle.ogg");

    [DataField("addSound")]
    public SoundSpecifier AddCard = new SoundCollectionSpecifier("STFanAdd");
}
[Serializable, NetSerializable]
public enum CardFanStackVisuals : byte
{
    CardsCount
}


[Serializable, NetSerializable]
public enum CardFanUiKey : byte
{
    Key
}
