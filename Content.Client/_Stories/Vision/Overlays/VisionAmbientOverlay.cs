using Content.Shared._Stories.Vision.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;

namespace Content.Client._Stories.Vision.Overlays;

public sealed partial class VisionAmbientOverlay : Overlay
{
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    public override OverlaySpace Space => OverlaySpace.BeforeLighting;

    public VisionAmbientOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.DrawingHandle is not DrawingHandleWorld worldHandle)
            return;

        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out VisionComponent? vision) || !vision.IsActive)
            return;

        if (vision.AmbientColor != null)
            worldHandle.DrawRect(args.WorldAABB, vision.AmbientColor.Value);
    }
}
