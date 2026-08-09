using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.PrisonerCollar;

[Serializable, NetSerializable] 
public enum PrisonerCollarConsoleUiKey : byte
{
    Key,
}

[Serializable, NetSerializable] 
public sealed class PrisonerCollarStatusEntry
{
    public NetEntity Collar;
    public NetCoordinates? Coordinates;
    public bool IsWorn;
    public string LocationName;
    public int MaxDamage;
    public string MobState;
    public PrisonerCollarState State;
    public int TotalDamage;
    public NetEntity? Wearer;
    public string WearerName;

    public PrisonerCollarStatusEntry(
        NetEntity collar,
        NetEntity? wearer,
        string wearerName,
        string mobState,
        int totalDamage,
        int maxDamage,
        NetCoordinates? coordinates,
        string locationName,
        PrisonerCollarState state,
        bool isWorn)
    {
        Collar = collar;
        Wearer = wearer;
        WearerName = wearerName;
        MobState = mobState;
        TotalDamage = totalDamage;
        MaxDamage = maxDamage;
        Coordinates = coordinates;
        LocationName = locationName;
        State = state;
        IsWorn = isWorn;
    }
}

[Serializable, NetSerializable] 
public sealed class PrisonerCollarConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public List<PrisonerCollarStatusEntry> Collars;

    public PrisonerCollarConsoleBoundUserInterfaceState(List<PrisonerCollarStatusEntry> collars)
    {
        Collars = collars;
    }
}

[Serializable, NetSerializable] 
public sealed class PrisonerCollarConsoleDetonateMessage : BoundUserInterfaceMessage
{
    public NetEntity Collar;

    public PrisonerCollarConsoleDetonateMessage(NetEntity collar)
    {
        Collar = collar;
    }
}

[Serializable, NetSerializable] 
public sealed class PrisonerCollarConsoleShockMessage : BoundUserInterfaceMessage
{
    public NetEntity Collar;

    public PrisonerCollarConsoleShockMessage(NetEntity collar)
    {
        Collar = collar;
    }
}

[Serializable, NetSerializable] 
public sealed class PrisonerCollarConsoleUnlockMessage : BoundUserInterfaceMessage
{
    public NetEntity Collar;

    public PrisonerCollarConsoleUnlockMessage(NetEntity collar)
    {
        Collar = collar;
    }
}

[Serializable, NetSerializable] 
public sealed class PrisonerCollarConsoleUnlinkMessage : BoundUserInterfaceMessage
{
    public NetEntity Collar;

    public PrisonerCollarConsoleUnlinkMessage(NetEntity collar)
    {
        Collar = collar;
    }
}

[Serializable, NetSerializable] 
public sealed class PrisonerCollarConsoleScanPairMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable] 
public sealed class PrisonerCollarConsoleSelectCameraMessage : BoundUserInterfaceMessage
{
    public NetEntity? Collar;

    public PrisonerCollarConsoleSelectCameraMessage(NetEntity? collar)
    {
        Collar = collar;
    }
}

[Serializable, NetSerializable] 
public sealed partial class PrisonerCollarDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable] 
public sealed partial class PrisonerCollarRemovalDoAfterEvent : SimpleDoAfterEvent
{
}
