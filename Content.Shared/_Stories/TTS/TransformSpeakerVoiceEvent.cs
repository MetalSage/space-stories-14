using Content.Shared.Inventory;

namespace Content.Shared._Stories.TTS;

public sealed class TransformSpeakerVoiceEvent : EntityEventArgs, IInventoryRelayEvent
{
    public EntityUid Sender;
    public string VoiceId;

    public TransformSpeakerVoiceEvent(EntityUid sender, string voiceId)
    {
        Sender = sender;
        VoiceId = voiceId;
    }

    public SlotFlags TargetSlots => SlotFlags.MASK;
}
