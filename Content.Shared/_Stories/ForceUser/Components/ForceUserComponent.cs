using Robust.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Damage;
using Robust.Shared.Serialization;
using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Content.Shared.Chemistry.Components;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;

namespace Content.Shared._Stories.ForceUser;

[RegisterComponent, AutoGenerateComponentState]
[Access(typeof(SharedForceUserSystem))]
public sealed partial class ForceUserComponent : Component
{
    [Dependency] private readonly IPrototypeManager _proto = default!; // TODO: ECS pls

    [DataField("preset", customTypeSerializer: typeof(PrototypeIdSerializer<ForcePresetPrototype>))]
    public string Preset = "Debug";
    public string Name() => _proto.TryIndex<ForcePresetPrototype>(Preset, out var proto) ? proto.Name : "Debug";
    public ForceSide Side() => _proto.TryIndex<ForcePresetPrototype>(Preset, out var proto) ? proto.Side : ForceSide.Debug;
    public string AlertType() => _proto.TryIndex<ForcePresetPrototype>(Preset, out var proto) ? proto.AlertType : "ForceVolume";

    [DataField("lightsaber"), AutoNetworkedField]
    public EntityUid? Lightsaber { get; set; } = null;

    [DataField("equipments")]
    public Dictionary<string, EntityUid>? Equipments { get; set; } = null;

    [DataField("tetherHand"), AutoNetworkedField]
    public EntityUid? TetherHand { get; set; } = null;

    /// <summary>
    /// Способность для открытия магазина. Не более.
    /// </summary>
    [DataField]
    public EntProtoId ShopAction = "ActionForceShop";

    [DataField, AutoNetworkedField]
    public EntityUid? ShopActionEntity;
}

