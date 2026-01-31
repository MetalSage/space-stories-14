using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Stories.TargetingTeleporter;

[RegisterComponent] [NetworkedComponent] [AutoGenerateComponentState(true)]
public sealed partial class TargetingTeleporterComponent : Component
{
    [DataField("exitPortalPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ExitPortalPrototype = "STBluspacePortalExit";

    [DataField] [AutoNetworkedField]
    public EntityUid? EyeEntity;

    [DataField(readOnly: true)]
    public EntProtoId? EyeEntityProto = "STTargetingTeleporterEye";

    [DataField] [AutoNetworkedField]
    public EntityUid? GridUid;

    [DataField("newPortalSound")]
    public SoundSpecifier? NewPortalSound;

    [DataField]
    public EntityWhitelist? StationBlacklist;

    [DataField]
    public EntityWhitelist? StationWhitelist;

    /// <summary>
    /// Можно ли будет вернуться обратно через портал выхода.
    /// </summary>
    [DataField]
    public bool Wayback = true;
}
