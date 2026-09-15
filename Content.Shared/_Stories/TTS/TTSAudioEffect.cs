using Robust.Shared.Serialization;

namespace Content.Shared._Stories.TTS;

[Flags]
[Serializable, NetSerializable]
// ReSharper disable once InconsistentNaming
public enum TTSAudioEffect : byte
{
    None = 0,
    StandardRadio = 1 << 0,
    Announce = 1 << 1,
    Annonce = Announce,
}
