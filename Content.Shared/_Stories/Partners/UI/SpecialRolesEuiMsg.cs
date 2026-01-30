using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Partners.UI;

public static class SpecialRolesEuiMsg
{
    [Serializable] [NetSerializable]
    public sealed class SendRoleData(
        string role,
        bool pickable,
        int? occurrences,
        int? minimumPlayers,
        int? earliestStart,
        int? maxOccurrences,
        int? timeSinceLastEvent,
        int? reoccurrenceDelay,
        StatusLabel? reason) : EuiMessageBase
    {
        public int? EarliestStart = earliestStart;
        public int? MaxOccurrences = maxOccurrences;
        public int? MinimumPlayers = minimumPlayers;
        public int? Occurrences = occurrences;
        public bool Pickable = pickable;
        public StatusLabel? Reason = reason;
        public int? ReoccurrenceDelay = reoccurrenceDelay;
        public string Role = role;
        public int? TimeSinceLastEvent = timeSinceLastEvent;
    }

    [Serializable] [NetSerializable]
    public sealed class GetRoleData(string role) : EuiMessageBase
    {
        public string Role = role;
    }
}
