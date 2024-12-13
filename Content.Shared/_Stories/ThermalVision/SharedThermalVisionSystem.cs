using Robust.Shared.Timing;

namespace Content.Shared._Stories.ThermalVision;

public abstract class SharedThermalVisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThermalVisionComponent, MapInitEvent>(OnThermalVisionMapInit);
        SubscribeLocalEvent<ThermalVisionComponent, ComponentRemove>(OnThermalVisionRemove);

        SubscribeLocalEvent<ThermalVisionComponent, AfterAutoHandleStateEvent>(OnThermalVisionAfterHandle);

        SubscribeLocalEvent<ThermalVisionComponent, ToggleThermalVisionEvent>(OnToggle);
    }

    private void OnThermalVisionAfterHandle(Entity<ThermalVisionComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        ThermalVisionChanged(ent);
    }

    private void OnThermalVisionMapInit(Entity<ThermalVisionComponent> ent, ref MapInitEvent args)
    {
        ThermalVisionChanged(ent);
    }

    private void OnThermalVisionRemove(Entity<ThermalVisionComponent> ent, ref ComponentRemove args)
    {
        ThermalVisionRemoved(ent);
    }

    protected virtual void ThermalVisionChanged(Entity<ThermalVisionComponent> ent)
    {
    }

    protected virtual void ThermalVisionRemoved(Entity<ThermalVisionComponent> ent)
    {
    }
    private void OnToggle(EntityUid uid, ThermalVisionComponent component, ToggleThermalVisionEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;
        component.Enabled = !component.Enabled;
        var ent = new Entity<ThermalVisionComponent>(uid, component);
        ThermalVisionChanged(ent);
    }
}
