using Robust.Shared.Serialization;

namespace Content.Shared._Stories.TTS;

[Serializable, NetSerializable] 
public sealed class RequestGlobalTTSEvent : EntityEventArgs
{
    public RequestGlobalTTSEvent(string text, string voiceId)
    {
        Text = text;
        VoiceId = voiceId;
    }

    public string Text { get; }
    public string VoiceId { get; }
}
