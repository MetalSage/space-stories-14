using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Inventory;

namespace Content.Shared._Stories.Vision.Events;

[ByRefEvent]
public record struct RefreshVisionEvent : IInventoryRelayEvent
{
    public bool IsActive = false;
    public bool DrawLighting = true;
    public bool DrawShadows = true;
    public bool DrawFov = true;
    public bool ThermalVision = false;
    public string? Shader = null;
    public string? ThermalShader = null;
    public Color? AmbientColor = null;
    public Color? ThermalAmbientColor = null;
    public int Priority = -1;

    public SlotFlags TargetSlots => SlotFlags.EYES | SlotFlags.HEAD | SlotFlags.MASK | SlotFlags.OUTERCLOTHING;

    public RefreshVisionEvent()
    {
    }
}

public sealed partial class ToggleVisionActionEvent : InstantActionEvent;

[DataDefinition]
public sealed partial class ToggleVisionAlertEvent : BaseAlertEvent;
