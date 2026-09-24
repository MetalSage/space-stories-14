using System.Numerics;

namespace Content.Shared._Stories.Sprite;

public sealed partial class StoriesSpriteSystem : EntitySystem
{
    public void SetRenderOrder(EntityUid ent, int order)
    {
        var sprite = EnsureComp<SpriteSetRenderOrderComponent>(ent);
        sprite.RenderOrder = order;
        Dirty(ent, sprite);
    }

    public void SetOffset(EntityUid ent, Vector2 offset)
    {
        var sprite = EnsureComp<SpriteSetRenderOrderComponent>(ent);
        sprite.Offset = offset;
        Dirty(ent, sprite);
    }
}
