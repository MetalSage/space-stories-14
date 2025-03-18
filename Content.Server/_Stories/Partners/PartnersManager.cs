using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Content.Shared._Stories.Partners;
using Robust.Shared.Network;

namespace Content.Server._Stories.Partners;

public sealed partial class PartnersManager
{
    [Dependency] private readonly IServerNetManager _netMgr = default!;
    [Dependency] private readonly IPartnersApiClient _apiClient = default!;

    private ISawmill _sawmill = default!;

    private readonly Dictionary<NetUserId, SponsorInfo> _cachedSponsors = new();

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("sponsors");

        _netMgr.RegisterNetMessage<MsgSponsorInfo>();

        _netMgr.Connecting += OnConnecting;
        _netMgr.Connected += OnConnected;
        _netMgr.Disconnect += OnDisconnect;

        _apiClient.Initialize();
    }

    public bool TryGetInfo(NetUserId userId, [NotNullWhen(true)] out SponsorInfo? sponsor)
    {
        return _cachedSponsors.TryGetValue(userId, out sponsor);
    }

    private async Task OnConnecting(NetConnectingArgs e)
    {
        var info = await LoadSponsorInfo(e.UserId);
        if (info?.Tier == null)
        {
            _cachedSponsors.Remove(e.UserId);
            return;
        }

        _cachedSponsors[e.UserId] = info;
    }

    private void OnConnected(object? sender, NetChannelArgs e)
    {
        var info = _cachedSponsors.TryGetValue(e.Channel.UserId, out var sponsor) ? sponsor : null;
        var msg = new MsgSponsorInfo() { Info = info };
        _netMgr.ServerSendMessage(msg, e.Channel);
    }

    private void OnDisconnect(object? sender, NetDisconnectedArgs e)
    {
        _cachedSponsors.Remove(e.Channel.UserId);
    }

    public async Task<SponsorInfo?> LoadSponsorInfo(NetUserId session)
    {
        var sponsorInfo = await _apiClient.GetSponsorInfoAsync(session);
        if (sponsorInfo == null)
            _sawmill.Warning($"Не удалось загрузить спонсорскую информацию для пользователя {session} от API.");

        return sponsorInfo;
    }

    public async Task<bool> DeductToken(NetUserId userId)
    {
        var result = await _apiClient.DeductTokenAsync(userId);
        if (result)
        {
            _sawmill.Debug($"Токен успешно списан для пользователя {userId}.");
            if (_cachedSponsors.TryGetValue(userId, out var sponsor))
                sponsor.Tokens--;
            else
                _sawmill.Warning($"Не удалось найти SponsorInfo в кэше для пользователя {userId} при списании токена, кэш не обновлен.");
        }
        else
        {
            _sawmill.Warning($"Не удалось списать токен для пользователя {userId} через API.");
        }
        return result;
    }
}
