using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Stories.ForceUser;

[RegisterComponent] [AutoGenerateComponentState]
[Access(typeof(SharedForceUserSystem))]
public sealed partial class ForceUserComponent : Component
{
    [Dependency] private readonly IPrototypeManager _proto = default!; // TODO: ECS pls

    [DataField("preset", customTypeSerializer: typeof(PrototypeIdSerializer<ForcePresetPrototype>))]
    public string Preset = "Debug";

    /// <summary>
    /// Способность для открытия магазина. Не более.
    /// </summary>
    [DataField]
    public EntProtoId ShopAction = "ActionForceShop";

    [DataField] [AutoNetworkedField]
    public EntityUid? ShopActionEntity;

    [DataField("lightsaber")] [AutoNetworkedField]
    public EntityUid? Lightsaber { get; set; }

    [DataField("equipments")]
    public Dictionary<string, EntityUid>? Equipments { get; set; }

    [DataField("tetherHand")] [AutoNetworkedField]
    public EntityUid? TetherHand { get; set; }

    public string Name()
    {
        return _proto.TryIndex<ForcePresetPrototype>(Preset, out var proto) ? proto.Name : "Debug";
    }

    public ForceSide Side()
    {
        return _proto.TryIndex<ForcePresetPrototype>(Preset, out var proto) ? proto.Side : ForceSide.Debug;
    }

    public string AlertType()
    {
        return _proto.TryIndex<ForcePresetPrototype>(Preset, out var proto) ? proto.AlertType : "ForceVolume";
    }
}
