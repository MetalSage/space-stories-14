namespace Content.Shared.GameTicking;

public sealed class RoundStartedEvent : EntityEventArgs
{
    public RoundStartedEvent(int roundId)
    {
        RoundId = roundId;
    }

    public int RoundId { get; }
}
