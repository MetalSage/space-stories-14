using System.Threading;
using Content.Shared._Stories.DiscordAuth;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Network;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Client._Stories.DiscordAuth;

public sealed class DiscordAuthState : State
{
    private readonly CancellationTokenSource _checkTimerCancel = new();
    [Dependency] private readonly IClientNetManager _netManager = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;

    private DiscordAuthGui? _gui;

    protected override void Startup()
    {
        _gui = new DiscordAuthGui();
        _userInterfaceManager.StateRoot.AddChild(_gui);

        Timer.SpawnRepeating(TimeSpan.FromSeconds(5),
            () =>
            {
                _netManager.ClientSendMessage(new MsgDiscordAuthCheck());
            },
            _checkTimerCancel.Token);
    }

    protected override void Shutdown()
    {
        _checkTimerCancel.Cancel();
        _gui!.Dispose();
    }
}
