using Content.Client._Stories.Economy.UI;
using Content.Shared._Stories.Economy;

namespace Content.Client._Stories.Economy;

public sealed class AtmBoundUserInterface : BoundUserInterface
{
    private AtmWindow? _window;

    public AtmBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = new AtmWindow(this);
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is AtmBoundUserInterfaceState atmState)
            _window?.UpdateState(atmState);
    }

    public void Login(string acc, string pin)
    {
        SendMessage(new AtmLoginMessage(acc, pin));
    }

    public void Withdraw(int amount)
    {
        SendMessage(new AtmWithdrawMessage(amount));
    }

    public void Logout()
    {
        SendMessage(new AtmLogoutMessage());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _window?.Dispose();
    }
}
