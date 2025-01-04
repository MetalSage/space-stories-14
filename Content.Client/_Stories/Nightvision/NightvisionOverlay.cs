using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Content.Shared._Stories.Nightvision;

namespace Content.Client._Stories.Nightvision;
public sealed class NightvisionOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _nightvisionShader;
    private NightvisionComponent _nightvisionComponent = default!;

    public NightvisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _nightvisionShader = _prototypeManager.Index<ShaderPrototype>("Nightvision").InstanceUnique();
    }
    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalSession?.AttachedEntity, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        var playerEntity = _playerManager.LocalSession?.AttachedEntity;

        if (playerEntity == null)
            return false;

        if (!_entityManager.TryGetComponent<NightvisionComponent>(playerEntity, out var nightvisionComp))
            return false;

        _nightvisionComponent = nightvisionComp;

        if (!_nightvisionComponent.Enabled)
        {
            _lightManager.DrawHardFov = true;
            _lightManager.DrawShadows = true;
            _lightManager.DrawLighting = true;
        }

        return _nightvisionComponent.Enabled;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var playerEntity = _playerManager.LocalSession?.AttachedEntity;

        if (playerEntity == null)
            return;

        _lightManager.DrawHardFov = true;
        _lightManager.DrawShadows = false;
        _lightManager.DrawLighting = false;

        _nightvisionShader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        var worldHandle = args.WorldHandle;
        var viewport = args.WorldBounds;
        worldHandle.UseShader(_nightvisionShader);
        worldHandle.DrawRect(viewport, Color.White);
        worldHandle.UseShader(null);
    }
}
