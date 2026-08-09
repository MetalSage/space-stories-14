namespace Content.Server._Stories.BlockMeleeAttack;

[RegisterComponent]
public sealed partial class BlockMeleeAttackUserComponent : Component
{
    [DataField("blockingItem")]
    public EntityUid? BlockingItem;
}
