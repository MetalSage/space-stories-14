using Robust.Shared.Audio;

namespace Content.Server._Stories.BlockMeleeAttack;

[RegisterComponent]
public sealed partial class BlockMeleeAttackComponent : Component
{
    [DataField("blockProb"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]  
    public float BlockProb = 0.5f;

    [DataField("blockSound")]
    public SoundSpecifier BlockSound = new SoundPathSpecifier("/Audio/Weapons/block_metal1.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.2f),
    };

    [DataField("enabled"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]  
    public bool Enabled = true;

    [ViewVariables, AutoNetworkedField] 
    public EntityUid? User;
}
