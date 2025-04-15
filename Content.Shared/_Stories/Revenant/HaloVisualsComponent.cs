using Robust.Shared.Animations;
using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Revenant;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class HaloVisualsComponent : Component
{
    /// <summary>
    ///     How long should the halo animation last in seconds, before being randomized?
    /// </summary>
    public float HaloLength = 2.0f;

    /// <summary>
    ///     How far away from the entity should the halo be, before being randomized?
    /// </summary>
    public float HaloDistance = 1.0f;

    /// <summary>
    ///     How long should the halo stop animation last in seconds?
    /// </summary>
    public float HaloStopLength = 1.0f;
}
