using Robust.Shared.GameStates;

namespace Content.Shared._Stories.ProtectiveBubble.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ProtectedByProtectiveBubbleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ProtectiveBubble;
}
