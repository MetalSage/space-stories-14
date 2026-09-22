using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Content.Shared._Stories.Sponsors;
using Content.Shared.Ghost.Components;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._Stories.Sponsors;

public sealed partial class SponsorsManager
{
    private readonly Dictionary<NetUserId, SponsorInfo> _cachedSponsors = new();
    private readonly Dictionary<NetUserId, string> _selectedGhostSkins = new();

    [Dependency] private readonly IEntityManager _entMgr = default!;
    [Dependency] private readonly IPlayerManager _playerMgr = default!;
    [Dependency] private ISponsorsApiClient _apiClient = default!;
    [Dependency] private IServerNetManager _netMgr = default!;

    private ISawmill _sawmill = default!;

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("sponsors");

        _netMgr.RegisterNetMessage<MsgSponsorInfo>();
        _netMgr.RegisterNetMessage<MsgSelectGhostSkin>(OnSelectGhostSkin);

        _netMgr.Connecting += OnConnecting;
        _netMgr.Connected += OnConnected;
        _netMgr.Disconnect += OnDisconnect;

        _apiClient.Initialize();
    }

    public bool TryGetInfo(NetUserId userId, [NotNullWhen(true)] out SponsorInfo? sponsor)
    {
        return _cachedSponsors.TryGetValue(userId, out sponsor);
    }

    public string? GetSelectedGhostSkin(NetUserId userId)
    {
        if (_selectedGhostSkins.TryGetValue(userId, out var skinId) &&
            _cachedSponsors.TryGetValue(userId, out var info) &&
            IsGhostSkinAllowed(info, skinId))
        {
            return skinId;
        }

        return null;
    }

    public bool IsGhostSkinAllowed(SponsorInfo info, string skinId)
    {
        if (string.IsNullOrEmpty(skinId) || skinId.Equals("default", StringComparison.OrdinalIgnoreCase))
            return true;

        return info.AllowedGhostSkins.Any(allowed =>
            string.Equals(allowed, skinId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(allowed, "all", StringComparison.OrdinalIgnoreCase));
    }

    private void OnSelectGhostSkin(MsgSelectGhostSkin msg)
    {
        var channel = msg.MsgChannel;
        var userId = channel.UserId;

        if (string.IsNullOrEmpty(msg.SkinId) || msg.SkinId.Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            _selectedGhostSkins.Remove(userId);
            UpdatePlayerGhostSkin(userId, null);
            return;
        }

        if (!_cachedSponsors.TryGetValue(userId, out var info) || !IsGhostSkinAllowed(info, msg.SkinId))
        {
            _sawmill.Warning($"User {userId} attempted to select unauthorized ghost skin '{msg.SkinId}'.");
            return;
        }

        _selectedGhostSkins[userId] = msg.SkinId;
        UpdatePlayerGhostSkin(userId, msg.SkinId);
    }

    private void UpdatePlayerGhostSkin(NetUserId userId, string? skinId)
    {
        if (!_playerMgr.TryGetSessionById(userId, out var session))
            return;

        if (session.AttachedEntity is not { } attached)
            return;

        if (!_entMgr.HasComponent<GhostComponent>(attached))
            return;

        if (string.IsNullOrEmpty(skinId))
        {
            _entMgr.RemoveComponent<SponsorGhostSkinComponent>(attached);
        }
        else
        {
            var comp = _entMgr.EnsureComponent<SponsorGhostSkinComponent>(attached);
            comp.Skin = skinId;
            _entMgr.Dirty(attached, comp);
        }
    }

    private async Task OnConnecting(NetConnectingArgs e)
    {
        var info = await LoadSponsorInfo(e.UserId);
        if (info == null)
        {
            _cachedSponsors.Remove(e.UserId);
            return;
        }

        _cachedSponsors[e.UserId] = info;
    }

    private void OnConnected(object? sender, NetChannelArgs e)
    {
        var info = _cachedSponsors.TryGetValue(e.Channel.UserId, out var sponsor) ? sponsor : null;
        var msg = new MsgSponsorInfo { Info = info };
        _netMgr.ServerSendMessage(msg, e.Channel);
    }

    private void OnDisconnect(object? sender, NetDisconnectedArgs e)
    {
        _cachedSponsors.Remove(e.Channel.UserId);
        _selectedGhostSkins.Remove(e.Channel.UserId);
    }

    public async Task<SponsorInfo?> LoadSponsorInfo(NetUserId session)
    {
        var sponsorInfo = await _apiClient.GetSponsorInfoAsync(session);
        if (sponsorInfo == null)
            _sawmill.Warning($"Не удалось загрузить спонсорскую информацию для пользователя {session} от API.");

        return sponsorInfo;
    }
}
