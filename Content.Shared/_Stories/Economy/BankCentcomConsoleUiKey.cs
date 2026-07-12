using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Economy;

[Serializable, NetSerializable]
public enum BankCentcomConsoleUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class BankCentcomConsoleState : BoundUserInterfaceState
{
    public readonly List<StationDto> Stations;
    public readonly NetEntity? SelectedStation;
    public readonly List<FinancialLogDto> Logs;
    public readonly List<AccountDto> Accounts;
    public readonly float SalaryModifier;
    public readonly float SalaryFrequency;

    public BankCentcomConsoleState(List<StationDto> stations, NetEntity? selectedStation, List<FinancialLogDto> logs, List<AccountDto> accounts, float salaryModifier, float salaryFrequency)
    {
        Stations = stations;
        SelectedStation = selectedStation;
        Logs = logs;
        Accounts = accounts;
        SalaryModifier = salaryModifier;
        SalaryFrequency = salaryFrequency;
    }
}

[Serializable, NetSerializable]
public struct StationDto
{
    public NetEntity NetId;
    public string Name;
    public StationDto(NetEntity netId, string name) { NetId = netId; Name = name; }
}

[Serializable, NetSerializable]
public enum CentcomFineTarget { Crew, Department, AllCrew, AllDepartments, All }

[Serializable, NetSerializable]
public sealed class CentcomIssueFineMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Station;
    public readonly CentcomFineTarget TargetType;
    public readonly string TargetId;
    public readonly int Amount;
    public readonly string Reason;
    public readonly bool IsPercentage;
    public readonly bool SendNotification;
    public readonly bool AnnounceToStation;
    public readonly string? CustomAnnouncement;

    public CentcomIssueFineMessage(NetEntity station, CentcomFineTarget targetType, string targetId, int amount, string reason, bool isPercentage, bool sendNotification, bool announceToStation, string? customAnnouncement)
    {
        Station = station;
        TargetType = targetType;
        TargetId = targetId;
        Amount = amount;
        Reason = reason;
        IsPercentage = isPercentage;
        SendNotification = sendNotification;
        AnnounceToStation = announceToStation;
        CustomAnnouncement = customAnnouncement;
    }
}

[Serializable, NetSerializable]
public sealed class CentcomCreateAccountMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Station;
    public readonly string OwnerName;
    public readonly int StartingBalance;

    public CentcomCreateAccountMessage(NetEntity station, string ownerName, int startingBalance)
    {
        Station = station;
        OwnerName = ownerName;
        StartingBalance = startingBalance;
    }
}

[Serializable, NetSerializable]
public sealed class CentcomChangeStationMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Station;
    public CentcomChangeStationMessage(NetEntity station) { Station = station; }
}

[Serializable, NetSerializable]
public sealed class CentcomSetSalaryMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Station;
    public readonly float Modifier;
    public readonly float FrequencyMins;
    public CentcomSetSalaryMessage(NetEntity station, float mod, float freq) { Station = station; Modifier = mod; FrequencyMins = freq; }
}

[Serializable, NetSerializable]
public sealed class CentcomEditAccountMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Station;
    public readonly string TargetId;
    public readonly int NewBalance;
    public readonly bool Delete;
    public readonly bool IsDepartment;

    public CentcomEditAccountMessage(NetEntity station, string targetId, int newBalance, bool delete, bool isDept)
    {
        Station = station;
        TargetId = targetId;
        NewBalance = newBalance;
        Delete = delete;
        IsDepartment = isDept;
    }
}

[Serializable, NetSerializable]
public sealed class CentcomResetPinMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Station;
    public readonly string TargetId;

    public CentcomResetPinMessage(NetEntity station, string targetId)
    {
        Station = station;
        TargetId = targetId;
    }
}

[Serializable, NetSerializable]
public sealed class CentcomRefreshMessage : BoundUserInterfaceMessage { }
