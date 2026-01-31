using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Cards.Card;

[RegisterComponent] [NetworkedComponent] [AutoGenerateComponentState]
public sealed partial class CardComponent : Component
{
    [DataField("name")] [AutoNetworkedField]
    public string Name = "default";
}
