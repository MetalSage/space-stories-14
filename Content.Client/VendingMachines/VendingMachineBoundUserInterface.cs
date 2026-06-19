// Stories-Economy
using Content.Client.UserInterface.Controls;
using Content.Client.VendingMachines.UI;
using Content.Shared.VendingMachines;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using System.Linq;

namespace Content.Client.VendingMachines
{
    public sealed class VendingMachineBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private VendingMachineMenu? _menu;

        private int? _lastBalance;

        public VendingMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

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

            _menu?.Populate(uiState.Inventory);
            
            if (_lastBalance != null)
                _menu?.UpdateBalance(_lastBalance);
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

            SendMessage(new VendingMachineEjectMessage(
                _menu!.Inventory[itemData.ItemIndex].Type,
                _menu!.Inventory[itemData.ItemIndex].ID));
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
}
