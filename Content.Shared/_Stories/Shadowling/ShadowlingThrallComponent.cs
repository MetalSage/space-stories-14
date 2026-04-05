using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Shadowling;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ShadowlingThrallComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color EyeColor = Color.Red;

    [DataField, AutoNetworkedField]
    public Color OldEyeColor = Color.Black;
}
