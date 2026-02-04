using System.Diagnostics.CodeAnalysis;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;

namespace Content.Shared._Stories.Sponsors;

public interface ISharedSponsorsManager : IEntitySystem
{
    bool TryGetInfo(NetUserId userId, [NotNullWhen(true)] out SponsorInfo? sponsor);
}
