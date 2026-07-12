using Content.Client._Stories.Economy.UI;
using Content.Shared._Stories.Economy;
using Robust.Client.UserInterface;

namespace Content.Client._Stories.Economy;

public sealed class BankSecurityConsoleBoundUserInterface : BoundUserInterface
{
    private BankSecurityConsoleWindow? _window;

    public BankSecurityConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) {}

    protected override void Open()
    {
        base.Open();
        _window = new BankSecurityConsoleWindow();
        _window.OnClose += Close;
        
        _window.OnRefresh += () => SendMessage(new BankSecurityRefreshMessage());
        _window.OnIssueFine += (targetId, amount, reason) => 
            SendMessage(new BankSecurityIssueFineMessage(targetId, amount, reason));

        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is BankSecurityConsoleState bankState)
            _window?.UpdateState(bankState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        _window?.Dispose();
    }
}
