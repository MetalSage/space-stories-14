using Robust.Shared.Audio;

namespace Content.Server._Stories.BlockMeleeAttack;

/// <summary>
/// This component goes on an item that you want to use to block
/// </summary>
[RegisterComponent]
public sealed partial class BlockMeleeAttackComponent : Component
{
    [DataField("blockProb")] [ViewVariables(VVAccess.ReadWrite)] [AutoNetworkedField]
    public float BlockProb = 0.5f;

    /// <summary>
    /// The sound to be played when you get hit while actively blocking
    /// </summary>
    [DataField("blockSound")]
    public SoundSpecifier BlockSound = new SoundPathSpecifier("/Audio/Weapons/block_metal1.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.2f),
    };

    [DataField("enabled")] [ViewVariables(VVAccess.ReadWrite)] [AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    /// The entity that's blocking
    /// </summary>
    [ViewVariables] [AutoNetworkedField]
    public EntityUid? User;
}
