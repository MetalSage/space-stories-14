using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Sprite;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpriteSetRenderOrderComponent : Component
{
    [DataField, AutoNetworkedField]
    public int? RenderOrder;

    [DataField, AutoNetworkedField]
    public Vector2? Offset;

    [Serializable, NetSerializable]
    public enum Appearance : byte
    {
        Key,
        Offset,
    }
}
