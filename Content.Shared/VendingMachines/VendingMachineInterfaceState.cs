using Robust.Shared.Serialization;

namespace Content.Shared.VendingMachines
{
    // Stories-Economy-Start
    [Serializable, NetSerializable]
    public sealed class VendingMachineUIState : BoundUserInterfaceState
    {
        public readonly List<VendingMachineInventoryEntry> Inventory;
        public readonly bool IsAuthorized;

        public VendingMachineUIState(List<VendingMachineInventoryEntry> inventory, bool isAuthorized)
        {
            Inventory = inventory;
            IsAuthorized = isAuthorized;
        }
    }

    [Serializable, NetSerializable]
    public sealed class VendingMachineBalanceMessage : BoundUserInterfaceMessage
    {
        public readonly int Balance;
        public VendingMachineBalanceMessage(int balance) => Balance = balance;
    }
    // Stories-Economy-End

    [Serializable, NetSerializable]
    public sealed class VendingMachineEjectMessage : BoundUserInterfaceMessage
    {
        public readonly InventoryType Type;
        public readonly string ID;
        public VendingMachineEjectMessage(InventoryType type, string id)
        {
            Type = type;
            ID = id;
        }
    }

    [Serializable, NetSerializable]
    public enum VendingMachineUiKey
    {
        Key,
    }
}
