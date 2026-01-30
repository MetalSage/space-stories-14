using Content.Shared._Stories.Cards.Fan;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;

namespace Content.Client._Stories.Cards.Fan.UI;

public sealed class FanMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    private readonly EntityUid _owner;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    private FanMenu? _menu;

    public FanMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _owner = owner;
    }

    protected override void Open()
    {
        base.Open();
        var user = _playerManager.LocalEntity;
        if (user == null)
            return;
        _menu = new FanMenu(_owner, this, user.Value);

        var vpSize = _displayManager.ScreenSize;
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }

    public void OnCardSelected(NetEntity cardEntity, NetEntity user)
    {
        SendMessage(new CardSelectedMessage(cardEntity, user));
    }
}
