using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Stories.TargetingTeleporter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class TargetingTeleporterComponent : Component
{

    [DataField, AutoNetworkedField]
    public EntityUid? EyeEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? GridUid;

    [DataField(readOnly: true)]
    public EntProtoId? EyeEntityProto = "STTargetingTeleporterEye";

    [DataField]
    public EntityWhitelist? StationWhitelist = null;

    [DataField]
    public EntityWhitelist? StationBlacklist = null;

    /// <summary>
    /// Можно ли будет вернуться обратно через портал выхода.
    /// </summary>
    [DataField]
    public bool Wayback = true;

    [DataField("exitPortalPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ExitPortalPrototype = "STBluspacePortalExit";

    [DataField("newPortalSound")]
    public SoundSpecifier NewPortalSound =
        new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg")
        {
            Params = AudioParams.Default.WithVolume(-2f)
        };
}
