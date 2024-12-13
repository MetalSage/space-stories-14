using Robust.Shared.Physics;

namespace Content.Server._Stories.BlockMeleeAttack;

[RegisterComponent]
public sealed partial class BlockMeleeAttackUserComponent : Component
{
    /// <summary>
    /// The entity that's being used to block
    /// </summary>
    [DataField("blockingItem")]
    public EntityUid? BlockingItem;
}
