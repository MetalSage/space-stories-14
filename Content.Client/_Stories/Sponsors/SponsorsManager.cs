using System.Diagnostics.CodeAnalysis;
using Content.Shared._Stories.Sponsors;
using Robust.Client.Player;
using Robust.Shared.Network;

namespace Content.Client._Stories.Sponsors;

public sealed class SponsorsManager
{
    [Dependency] private readonly IClientNetManager _netMgr = default!;
    [Dependency] private readonly IPlayerManager _playerMgr = default!;

    private SponsorInfo? _info;

    public void Initialize()
    {
        _netMgr.RegisterNetMessage<MsgSponsorInfo>(msg => _info = msg.Info);
    }

    public bool TryGetInfo([NotNullWhen(true)] out SponsorInfo? sponsor)
    {
        sponsor = _info;
        return _info != null;
    }

    public bool TryGetInfo(NetUserId userId, [NotNullWhen(true)] out SponsorInfo? sponsor)
    {
        sponsor = null;
        if (_playerMgr.LocalSession?.UserId != userId)
            return false;

        return TryGetInfo(out sponsor);
    }
}
