using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.Gravity;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LiftingUpComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public float AnimationTime = 0.2f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public float AnimationDownTime = 0.1f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public Vector2 Offset = new(0, 0.3f);
    public readonly string AnimationKey = "liftingup";
    public readonly string AnimationDownKey = "liftingdown";
}
