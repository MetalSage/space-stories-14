using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Content.Shared._Stories.Cards.Fan;

namespace Content.Client._Stories.Cards.Fan.UI;
public sealed class FanMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    private FanMenu? _menu;
    private EntityUid _owner;
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
        _menu = new(_owner, this, user.Value);

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
