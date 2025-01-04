using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Reflectors;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ReflectorComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public List<string> BlockedDirections = new();

    [DataField]
    public Direction? ReflectionDirection;

    [DataField, AutoNetworkedField]
    public ReflectorType State = ReflectorType.Simple;
}

[Serializable, NetSerializable]
public enum  ReflectorType
{
    Simple,
    Angular,
}
