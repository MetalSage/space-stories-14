namespace Content.Server._Stories.Debuff;

[RegisterComponent]
public sealed partial class FlashDebuffComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("enabled")]
    public bool Enabled { get; set; } = true;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("coefficientDuration")]
    public float CoefficientDuration = 2f;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("blockFlashImmunity")]
    public bool BlockFlashImmunity = false;
}
