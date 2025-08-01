using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Cards.Fan;

[RegisterComponent, NetworkedComponent]
public sealed partial class CardFanComponent : Component
{
    [DataField]
    public SoundSpecifier ShuffleSound = new SoundPathSpecifier("/Audio/_Stories/Items/Cards/FanShuffle.ogg");

    [DataField]
    public SoundSpecifier AddCardSound = new SoundCollectionSpecifier("STFanAdd");

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Radius = 0.2f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxCards = 10;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float StartAngle = 135f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float EndAngle = 225f;
}

[Serializable, NetSerializable]
public enum CardFanUiKey : byte
{
    Key
}
