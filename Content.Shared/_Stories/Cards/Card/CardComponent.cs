using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Cards.Card;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CardComponent : Component
{
    [DataField("name", readOnly: true), AutoNetworkedField]
    public string Name;
}

[Serializable, NetSerializable]
public enum CardVisuals : byte
{
    State
}
