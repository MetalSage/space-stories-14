using Content.Shared.FixedPoint;

namespace Content.Shared._Stories.Force;

[RegisterComponent]
public sealed partial class ForceComponent : Component
{
    #region Volume
    [ViewVariables(VVAccess.ReadWrite), DataField("volume")]
    public FixedPoint2 Volume { get; set; }

    [ViewVariables(VVAccess.ReadWrite), DataField("passiveVolume")]
    public FixedPoint2 PassiveVolume { get; set; }

    [ViewVariables(VVAccess.ReadWrite), DataField("maxVolume")]
    public FixedPoint2 MaxVolume { get; set; }
    #endregion

    #region Debuff
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 CurrentDebuff { get; set; } = 0f;
    [ViewVariables(VVAccess.ReadOnly)]
    public float PassiveDebuff { get; set; } = -1f;
    #endregion
}
