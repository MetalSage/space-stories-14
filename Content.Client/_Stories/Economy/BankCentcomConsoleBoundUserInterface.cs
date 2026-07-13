using Content.Client._Stories.Economy.UI;
using Content.Shared._Stories.Economy;
using Robust.Client.UserInterface;

namespace Content.Client._Stories.Economy;

public sealed class BankCentcomConsoleBoundUserInterface : BoundUserInterface
{
    private BankCentcomConsoleWindow? _window;

    public BankCentcomConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) {}

    protected override void Open()
    {
        base.Open();
        _window = new BankCentcomConsoleWindow();
        _window.OnClose += Close;

        _window.OnRefresh += () => SendMessage(new CentcomRefreshMessage());
        _window.OnChangeStation += (station) => SendMessage(new CentcomChangeStationMessage(station));
        _window.OnCreateAccount += (station, name, bal) => SendMessage(new CentcomCreateAccountMessage(station, name, bal));
        _window.OnResetPin += (station, acc) => SendMessage(new CentcomResetPinMessage(station, acc));

        _window.OnIssueFine += (station, targetType, targetId, amount, reason, isPercent, notify, announce, customAnnouncement) =>
            SendMessage(new CentcomIssueFineMessage(station, targetType, targetId, amount, reason, isPercent, notify, announce, customAnnouncement));

        _window.OnSetSalary += (station, mod, freq) => SendMessage(new CentcomSetSalaryMessage(station, mod, freq));
        _window.OnEditAccount += (station, targetId, bal, del, isDept) => SendMessage(new CentcomEditAccountMessage(station, targetId, bal, del, isDept));

        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is BankCentcomConsoleState bankState)
            _window?.UpdateState(bankState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        _window?.Dispose();
    }
}
