using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class ShadowCocoonComponent : Component
{
    public const string ContainerId = "shadow_cocoon_body";

    [DataField]
    public float ExtinguishRange = 4f;

    [DataField]
    public TimeSpan PulseInterval = TimeSpan.FromSeconds(3);

    [ViewVariables]
    public TimeSpan NextPulse = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier? HallucinationSounds;

    [DataField]
    public float HallucinationChance = 0.2f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Silent = true;

    [ViewVariables]
    public Container? BodyContainer;
}
