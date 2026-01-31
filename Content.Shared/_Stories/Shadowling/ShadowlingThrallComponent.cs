using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Shadowling;

[RegisterComponent] [NetworkedComponent]
public sealed partial class ShadowlingThrallComponent : Component
{
    [DataField]
    public Color EyeColor = Color.Red;

    [DataField]
    public Color OldEyeColor = Color.Black;
}
