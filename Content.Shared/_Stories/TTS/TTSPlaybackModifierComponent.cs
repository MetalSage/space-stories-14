namespace Content.Shared._Stories.TTS;

[RegisterComponent]
// ReSharper disable once InconsistentNaming
public sealed partial class TTSPlaybackModifierComponent : Component
{
    [DataField]
    public float VolumeMultiplier = 1f;

    [DataField]
    public float RangeMultiplier = 1f;

    [DataField]
    public float? MaxDistance;

    [DataField]
    public float? ReferenceDistance;

    [DataField]
    public float? RolloffFactor;

    [DataField]
    public TTSAudioEffect AudioEffects = TTSAudioEffect.None;
}
