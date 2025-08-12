using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using Robust.Shared.Containers;

namespace Content.Shared._Stories.Cards.Stack;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CardStackComponent : Component
{
    [ViewVariables, DataField("content")]
    public List<EntProtoId> InitialContent = [];

    [ViewVariables, AutoNetworkedField]
    public List<EntityUid> CardOrder = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public int MaxCards = 216;

    [ViewVariables]
    public Container CardContainer;

    [DataField]
    public SoundSpecifier AddCardSound = new SoundCollectionSpecifier("STAddCard");

    [DataField]
    public SoundSpecifier RemoveCardSound = new SoundCollectionSpecifier("STRemoveCard");
}

[Serializable, NetSerializable]
public enum CardStackVisuals : byte
{
    CardsCount,
    OrderEdited
}
