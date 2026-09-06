using Robust.Shared.Audio;

namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class DemonDeathSoundComponent : Component
{
    [DataField(required: true)]
    public SoundSpecifier Sound = default!;
}
