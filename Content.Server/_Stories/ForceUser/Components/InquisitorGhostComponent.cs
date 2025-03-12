using Robust.Shared.Prototypes;

namespace Content.Server._Stories.ForceUser.Components;

[RegisterComponent]
public sealed partial class InquisitorGhostComponent : Component
{
    [DataField("revertAction")]
    public EntProtoId RevertActionPrototype = "ActionInquisitorRevertPolymorph";

    [DataField("range")]
    public float Range = 5f;
}
