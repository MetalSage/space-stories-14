using Content.Client._Stories.Vision.Overlays;
using Content.Shared._Stories.Vision.Components;
using Content.Shared._Stories.Vision.Systems;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._Stories.Vision.Systems;

public sealed partial class VisionSystem : SharedVisionSystem
{
    [Dependency] private ILightManager _lightManager = default!;
    [Dependency] private IOverlayManager _overlayManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<VisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<VisionComponent, AfterAutoHandleStateEvent>(OnStateHandled);
        SubscribeLocalEvent<VisionComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnPlayerAttached(EntityUid uid, VisionComponent comp, LocalPlayerAttachedEvent args)
    {
        ApplyVisionModifiers(comp);
    }

    private void OnPlayerDetached(EntityUid uid, VisionComponent comp, LocalPlayerDetachedEvent args)
    {
        ClearVisionModifiers();
    }

    private void OnStateHandled(EntityUid uid, VisionComponent comp, ref AfterAutoHandleStateEvent args)
    {
        if (uid == _playerManager.LocalEntity)
            ApplyVisionModifiers(comp);
    }

    private void OnComponentRemove(EntityUid uid, VisionComponent comp, ComponentRemove args)
    {
        if (uid == _playerManager.LocalEntity)
            ClearVisionModifiers();
    }

    private void ApplyVisionModifiers(VisionComponent comp)
    {
        if (!comp.IsActive)
        {
            ClearVisionModifiers();
            return;
        }

        _lightManager.DrawLighting = comp.DrawLighting;
        _lightManager.DrawShadows = comp.DrawShadows;
        _lightManager.DrawHardFov = comp.DrawFov;

        if (comp.AmbientColor != null)
        {
            if (!_overlayManager.HasOverlay<VisionAmbientOverlay>())
                _overlayManager.AddOverlay(new VisionAmbientOverlay());
        }
        else
        {
            _overlayManager.RemoveOverlay<VisionAmbientOverlay>();
        }

        if (comp.ThermalVision || comp.Shader != null)
        {
            if (!_overlayManager.HasOverlay<VisionOverlay>())
                _overlayManager.AddOverlay(new VisionOverlay());
        }
        else
        {
            _overlayManager.RemoveOverlay<VisionOverlay>();
        }
    }

    private void ClearVisionModifiers()
    {
        _lightManager.DrawLighting = true;
        _lightManager.DrawShadows = true;
        _lightManager.DrawHardFov = true;

        _overlayManager.RemoveOverlay<VisionAmbientOverlay>();
        _overlayManager.RemoveOverlay<VisionOverlay>();
    }
}
