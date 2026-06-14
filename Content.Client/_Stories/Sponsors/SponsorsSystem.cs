using System.Diagnostics.CodeAnalysis;
using Content.Shared._Stories.Sponsors;
using Robust.Shared.Network;

namespace Content.Client._Stories.Sponsors;

public sealed partial class SponsorsSystem : EntitySystem, ISharedSponsorsManager
{
    [Dependency] private SponsorsManager _sponsorsManager = default!;

    public bool TryGetInfo(NetUserId userId, [NotNullWhen(true)] out SponsorInfo? sponsor)
    {
        return _sponsorsManager.TryGetInfo(userId, out sponsor);
    }
}
