using Content.Shared._Stories.Sprite;
using Robust.Client.GameObjects;

namespace Content.Client._Stories.Sprite;

public sealed partial class StoriesSpriteVisualizerSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    public override void FrameUpdate(float frameTime)
    {
        var query = AllEntityQuery<SpriteSetRenderOrderComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var set, out var sprite))
        {
            if (set.RenderOrder != null)
                sprite.RenderOrder = (uint) set.RenderOrder.Value;

            if (set.Offset != null)
                _sprite.SetOffset((uid, sprite), set.Offset.Value);
        }
    }
}
