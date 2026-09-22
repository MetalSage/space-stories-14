using System.Diagnostics.CodeAnalysis;
using Content.Shared._Stories.SCCVars;
using Content.Shared._Stories.Sponsors;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Client._Stories.Sponsors;

public sealed partial class SponsorsManager
{
    private SponsorInfo? _info;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IClientNetManager _netMgr = default!;
    [Dependency] private readonly IPlayerManager _playerMgr = default!;

    public event Action<SponsorInfo?>? OnSponsorInfoLoaded;

    public void Initialize()
    {
        _netMgr.RegisterNetMessage<MsgSponsorInfo>(OnReceiveSponsorInfo);
        _netMgr.RegisterNetMessage<MsgSelectGhostSkin>();
    }

    private void OnReceiveSponsorInfo(MsgSponsorInfo msg)
    {
        _info = msg.Info;
        OnSponsorInfoLoaded?.Invoke(_info);

        var savedSkin = _cfg.GetCVar(SCCVars.SelectedGhostSkin);
        if (!string.IsNullOrEmpty(savedSkin))
            SendGhostSkinSelection(savedSkin);
    }

    public void SendGhostSkinSelection(string? skinId)
    {
        var msg = new MsgSelectGhostSkin { SkinId = skinId };
        _netMgr.ClientSendMessage(msg);
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
