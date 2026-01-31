using Content.Shared.Flash;
using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Debuff;

[RegisterComponent] [NetworkedComponent] [AutoGenerateComponentState]
[Access(typeof(SharedFlashSystem))]
public sealed partial class FlashDebuffComponent : Component
{
    /// <summary>
    /// Does it interfere with eye protection?
    /// </summary>
    [DataField] [AutoNetworkedField]
    public bool BlockFlashImmunity;

    /// <summary>
    /// How many times does the duration increase?
    /// </summary>
    [DataField] [AutoNetworkedField]
    public float CoefficientDuration = 2f;

    /// <summary>
    /// Is this component currently enabled?
    /// </summary>
    [DataField] [AutoNetworkedField]
    public bool Enabled { get; set; } = true;
}
