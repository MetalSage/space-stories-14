using Content.Shared._Stories.ThermalVision;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._Stories.ThermalVision;

public sealed class ThermalVisionSystem : SharedThermalVisionSystem
{
    [Dependency] private readonly ILightManager _light = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThermalVisionComponent, LocalPlayerAttachedEvent>(OnThermalVisionAttached);
        SubscribeLocalEvent<ThermalVisionComponent, LocalPlayerDetachedEvent>(OnThermalVisionDetached);
    }

    private void OnThermalVisionAttached(Entity<ThermalVisionComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        ThermalVisionChanged(ent);
    }

    private void OnThermalVisionDetached(Entity<ThermalVisionComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        Off(ent.Comp.Innate);
    }

    protected override void ThermalVisionChanged(Entity<ThermalVisionComponent> ent)
    {
        if (ent != _player.LocalEntity)
            return;

        if (ent.Comp.Enabled)
            On(ent.Comp.Innate);
        else
            Off(ent.Comp.Innate);
    }

    protected override void ThermalVisionRemoved(Entity<ThermalVisionComponent> ent)
    {
        if (ent != _player.LocalEntity)
            return;

        Off(ent.Comp.Innate);
    }

    private void Off(bool isInnate)
    {
        _overlay.RemoveOverlay(new ThermalVisionOverlay());

        if(isInnate)
        {
            _light.DrawShadows = true;
            _light.DrawLighting = true;
        }
    }

    private void On(bool isInnate)
    {
        _overlay.AddOverlay(new ThermalVisionOverlay());

        if(isInnate)
        {
            _light.DrawShadows = false;
            _light.DrawLighting = false;
        }
    }
}
