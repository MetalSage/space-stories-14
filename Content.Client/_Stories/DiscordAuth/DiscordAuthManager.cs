using Content.Shared._Stories.DiscordAuth;
using Robust.Client.State;
using Robust.Shared.Network;

namespace Content.Client._Stories.DiscordAuth;

public sealed partial class DiscordAuthManager
{
    [Dependency] private IClientNetManager _netManager = default!;
    [Dependency] private IStateManager _stateManager = default!;

    public string AuthUrl { get; private set; } = string.Empty;

    public void Initialize()
    {
        _netManager.RegisterNetMessage<MsgDiscordAuthCheck>();
        _netManager.RegisterNetMessage<MsgDiscordAuthRequired>(OnDiscordAuthRequired);
    }

    private void OnDiscordAuthRequired(MsgDiscordAuthRequired message)
    {
        if (_stateManager.CurrentState is not DiscordAuthState)
        {
            AuthUrl = message.AuthUrl;
            _stateManager.RequestStateChange<DiscordAuthState>();
        }
    }
}
