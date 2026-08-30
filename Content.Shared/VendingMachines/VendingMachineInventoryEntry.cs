using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.VendingMachines;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class VendingMachineInventoryEntry(InventoryType type, EntProtoId id, uint amount, uint price = 0)
{
    [DataField]
    public InventoryType Type = type;

    [DataField]
    public EntProtoId ID = id;

    [DataField]
    public uint Amount = amount;

    // Stories-Economy-Start
    [DataField]
    public uint Price = price;
    // Stories-Economy-End

    public VendingMachineInventoryEntry(VendingMachineInventoryEntry entry) : this(entry.Type, entry.ID, entry.Amount, entry.Price) { }
}

[Serializable, NetSerializable]
public enum InventoryType : byte
{
    Regular,
    Emagged,
    Contraband
}

[Serializable, NetSerializable]
public sealed class VendingMachineComponentState : ComponentState
{
    public Dictionary<string, VendingMachineInventoryEntry> Inventory = new();

    public Dictionary<string, VendingMachineInventoryEntry> EmaggedInventory = new();

    public Dictionary<string, VendingMachineInventoryEntry> ContrabandInventory = new();

    public bool Contraband;

    public bool Broken;
}
