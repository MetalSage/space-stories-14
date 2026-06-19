using Content.Client.UserInterface.Fragments;
using Content.Shared._Stories.Economy;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;

namespace Content.Client._Stories.Economy;

public sealed partial class BankCartridgeUi : UIFragment
{
    private BankCartridgeUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new BankCartridgeUiFragment();
        _fragment.OnTransfer += (target, amount) =>
            userInterface.SendMessage(new CartridgeUiMessage(new BankTransferMessage(target, amount)));
        _fragment.OnLink += () => userInterface.SendMessage(new CartridgeUiMessage(new BankLinkIdMessage()));
        _fragment.OnUnlink += () => userInterface.SendMessage(new CartridgeUiMessage(new BankUnlinkIdMessage()));
        _fragment.OnToggleNotifications += () =>
            userInterface.SendMessage(new CartridgeUiMessage(new BankToggleNotificationsMessage()));
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is BankCartridgeUiState bankState)
            _fragment?.UpdateState(bankState);
    }
}
