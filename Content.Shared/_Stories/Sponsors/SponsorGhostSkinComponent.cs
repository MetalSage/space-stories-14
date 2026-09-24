using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Stories.Sponsors;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class SponsorGhostSkinComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? Skin;
}

[RegisterComponent]
public sealed partial class SponsorGhostSkinInfoComponent : Component
{
    [DataField]
    public SpriteSpecifier? Sprite;
}
