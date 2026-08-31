using Content.Shared.Hands.Components; // Stories-RetractableItems
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.RetractableItemAction;

/// <summary>
/// Used for storing an unremovable item within an action and summoning it into your hand on use.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(RetractableItemActionSystem))]
public sealed partial class RetractableItemActionComponent : Component
{
    /// <summary>
    /// The item that will appear be spawned by the action.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId SpawnedPrototype;

    // Stories-RetractableItems-Start
    /// <summary>
    /// Target hand location (Left/Right) to deploy the item into. If null, uses active hand.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HandLocation? TargetHandLocation;
    // Stories-RetractableItems-End

    /// <summary>
    /// Sound to play when the item is summoned.
    /// </summary>
    [DataField]
    public SoundSpecifier? SummonSounds; // Stories-RetractableItems

    /// <summary>
    /// Sound to play when the summoned item is retracted back into the action.
    /// </summary>
    [DataField]
    public SoundSpecifier? RetractSounds; // Stories-RetractableItems

    /// <summary>
    /// The item managed by the action. Will be summoned and hidden as the action is used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ActionItemUid;

    /// <summary>
    /// The container ID used to store the item.
    /// </summary>
    public const string ContainerId = "item-action-item-container";
}
