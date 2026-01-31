namespace Content.Shared.GameTicking;

public sealed class RoundEndedEvent : EntityEventArgs
{
    public RoundEndedEvent(int roundId, TimeSpan roundDuration)
    {
        RoundId = roundId;
        RoundDuration = roundDuration;
    }

    public int RoundId { get; }
    public TimeSpan RoundDuration { get; }
}
