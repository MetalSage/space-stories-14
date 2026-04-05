using Content.Shared._Stories.Vision.Components;
using Content.Shared.Mobs.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._Stories.Vision.Overlays;

public sealed class VisionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly TransformSystem _transform;
    private readonly SpriteSystem _sprite;

    private ShaderInstance? _screenShader;
    private string? _cachedScreenShaderId;

    private ShaderInstance? _thermalShader;
    private string? _cachedThermalShaderId;

    public VisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _transform = _entityManager.System<TransformSystem>();
        _sprite = _entityManager.System<SpriteSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out VisionComponent? vision) || !vision.IsActive)
            return;

        var handle = args.WorldHandle;
        var eye = args.Viewport.Eye;
        var eyeRot = eye?.Rotation ?? default;

        if (vision.Shader != null && ScreenTexture != null)
        {
            if (_screenShader == null || _cachedScreenShaderId != vision.Shader)
            {
                _screenShader = _prototypeManager.Index<ShaderPrototype>(vision.Shader).InstanceUnique();
                _cachedScreenShaderId = vision.Shader;
            }

            _screenShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

            if (vision.AmbientColor != null)
                _screenShader.SetParameter("ambient_color", vision.AmbientColor.Value);
            else
                _screenShader.SetParameter("ambient_color", new Color(1.0f, 1.0f, 1.0f, 1.0f));

            handle.UseShader(_screenShader);
            handle.DrawRect(args.WorldBounds, Color.White);
            handle.UseShader(null);
        }

        if (vision.ThermalVision)
        {
            if (vision.ThermalShader != null)
            {
                if (_thermalShader == null || _cachedThermalShaderId != vision.ThermalShader)
                {
                    _thermalShader = _prototypeManager.Index<ShaderPrototype>(vision.ThermalShader).InstanceUnique();
                    _cachedThermalShaderId = vision.ThermalShader;
                }
                handle.UseShader(_thermalShader);
            }
            else
            {
                handle.UseShader(null);
            }

            var entities = _entityManager.EntityQueryEnumerator<MobStateComponent, SpriteComponent, TransformComponent>();
            while (entities.MoveNext(out var uid, out _, out var sprite, out var xform))
            {
                if (xform.MapID != args.MapId)
                    continue;

                var (position, rotation) = _transform.GetWorldPositionRotation(xform);

                if (!args.WorldBounds.Contains(position))
                    continue;

                var colorCache = sprite.Color;
                if (vision.ThermalAmbientColor != null)
                {
                    _sprite.SetColor((uid, sprite), colorCache * vision.ThermalAmbientColor.Value);
                }

                _sprite.RenderSprite((uid, sprite), handle, eyeRot, rotation, position);

                if (vision.ThermalAmbientColor != null)
                {
                    _sprite.SetColor((uid, sprite), colorCache);
                }
            }

            handle.UseShader(null);
        }
    }
}
