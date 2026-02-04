using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Content.Shared._Stories.Sponsors;
using Content.Shared._Stories.SCCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Server._Stories.Sponsors;

public interface ISponsorsApiClient
{
    Task<SponsorInfo?> GetSponsorInfoAsync(NetUserId userId);
    void Initialize();
}

public sealed class SponsorsApiClient : ISponsorsApiClient
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private readonly HttpClient _httpClient = new();
    private string _apiUrl = string.Empty;

    private ISawmill _sawmill = default!;

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("sponsors.api");
        _cfg.OnValueChanged(SCCVars.SponsorsApiUrl, s => _apiUrl = s, true);

        if (string.IsNullOrEmpty(_apiUrl))
            _sawmill.Warning("URL веб-API спонсоров не настроен. Интеграция спонсоров отключена.");
    }

    public async Task<SponsorInfo?> GetSponsorInfoAsync(NetUserId userId)
    {
        if (string.IsNullOrEmpty(_apiUrl))
            return null;
        try
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{userId.ToString()}");

            if (!response.IsSuccessStatusCode)
            {
                _sawmill.Warning(
                    $"Ошибка получения информации о спонсоре для {userId}. Статус код: {response.StatusCode}, URL: {_apiUrl}/{userId}");
                return null;
            }

            var sponsorInfo = await response.Content.ReadFromJsonAsync<SponsorInfo>();
            return sponsorInfo;
        }
        catch (HttpRequestException e)
        {
            _sawmill.Error(
                $"Ошибка HTTP при запросе информации о спонсоре для {userId}. URL: {_apiUrl}/{userId}, Ошибка: {e.Message}");
            return null;
        }
        catch (JsonException e)
        {
            _sawmill.Error(
                $"Ошибка десериализации JSON при получении информации о спонсоре для {userId}. URL: {_apiUrl}/{userId}, Ошибка: {e.Message}");
            return null;
        }
    }
}
