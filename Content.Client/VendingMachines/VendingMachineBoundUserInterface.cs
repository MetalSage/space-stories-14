// Stories-Economy
using Content.Client.UserInterface.Controls;
using Content.Client.VendingMachines.UI;
using Content.Shared.VendingMachines;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using System.Linq;
using Content.Shared.VendingMachines.Components;

namespace Content.Client.VendingMachines;

public sealed class VendingMachineBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private VendingMachineMenu? _menu;

    [ViewVariables]
    private List<VendingMachineInventoryEntry> _cachedInventory = new();

    private int? _lastBalance;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindowCenteredLeft<VendingMachineMenu>();
        _menu.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _menu.OnItemSelected += OnItemSelected;

        if (_lastBalance != null)
            _menu.UpdateBalance(_lastBalance);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not VendingMachineUIState uiState)
            return;

        _cachedInventory = uiState.Inventory;
        var enabled = EntMan.TryGetComponent(Owner, out VendingMachineEjectComponent? eject) && !eject.Ejecting;
        _menu?.Populate(_cachedInventory, enabled);

        if (_lastBalance != null)
            _menu?.UpdateBalance(_lastBalance);
    }

    public void UpdateAmounts()
    {
        var enabled = EntMan.TryGetComponent(Owner, out VendingMachineEjectComponent? eject) && !eject.Ejecting;
        _menu?.UpdateAmounts(_cachedInventory, enabled);
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);

        if (message is VendingMachineBalanceMessage balanceMessage)
        {
            _lastBalance = balanceMessage.Balance;
            _menu?.UpdateBalance(_lastBalance);
        }
    }

    private void OnItemSelected(GUIBoundKeyEventArgs args, ListData data)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        if (data is not VendorItemsListData itemData)
            return;

        if (_menu == null || itemData.ItemIndex < 0 || itemData.ItemIndex >= _menu.Inventory.Count)
            return;

        var selectedItem = _menu.Inventory[itemData.ItemIndex];
        _menu.SetButtonsDisabled(true);
        SendMessage(new VendingMachineEjectMessage(selectedItem.Type, selectedItem.ID));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        if (_menu == null)
            return;

        _menu.OnItemSelected -= OnItemSelected;
        _menu.OnClose -= Close;
        _menu.Dispose();
    }
}
