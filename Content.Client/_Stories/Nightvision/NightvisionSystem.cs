using Robust.Client.Graphics;
using Robust.Client.Player;
using Content.Shared._Stories.Nightvision;
using Content.Shared.GameTicking;
using Robust.Shared.Player;

namespace Content.Client._Stories.Nightvision;

public sealed class NightvisionSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] ILightManager _lightManager = default!;
    private NightvisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightvisionComponent, ComponentInit>(OnBlindInit);
        SubscribeLocalEvent<NightvisionComponent, ComponentShutdown>(OnBlindShutdown);

        SubscribeLocalEvent<NightvisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<NightvisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeNetworkEvent<RoundRestartCleanupEvent>(RoundRestartCleanup);

        _overlay = new();
    }
    private void OnPlayerAttached(EntityUid uid, NightvisionComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, NightvisionComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
        _lightManager.DrawHardFov = true;
        _lightManager.DrawShadows = true;
        _lightManager.DrawLighting = true;
    }

    private void OnBlindInit(EntityUid uid, NightvisionComponent component, ComponentInit args)
    {
        if (_player.LocalEntity == uid)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnBlindShutdown(EntityUid uid, NightvisionComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity == uid)
        {
            _overlayMan.RemoveOverlay(_overlay);
            _lightManager.DrawHardFov = true;
            _lightManager.DrawShadows = true;
            _lightManager.DrawLighting = true;
        }
    }

    private void RoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _lightManager.DrawHardFov = true;
        _lightManager.DrawShadows = true;
        _lightManager.DrawLighting = true;
    }
}
