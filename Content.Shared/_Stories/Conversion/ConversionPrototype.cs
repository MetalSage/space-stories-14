using Content.Shared.Mobs;
using Content.Shared.StatusIcon;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Stories.Conversion;

[Prototype("conversion")]
public sealed partial class ConversionPrototype : IPrototype
{
    [ViewVariables][IdDataField] public string ID { get; private set; } = default!;

    #region Other

    [DataField("statusIcon", customTypeSerializer: typeof(PrototypeIdSerializer<FactionIconPrototype>))]
    public string? StatusIcon = null;

    [DataField("channels")]
    public HashSet<string> Channels = new();

    [DataField]
    public float? Duration = null;
    #endregion

    #region Briefing
    [DataField]
    public ConversionBriefingData? Briefing;

    [DataField]
    public ConversionBriefingData? EndBriefing;
    #endregion

    #region Whitelist
    [DataField]
    public HashSet<MobState>? AllowedMobStates = [MobState.Alive];

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;
    #endregion

    #region Components
    [DataField]
    public ComponentRegistry Components = new();

    [DataField]
    public List<ProtoId<EntityPrototype>>? MindRoles;
    #endregion
}

[DataDefinition]
public partial struct ConversionBriefingData
{
    /// <summary>
    /// The text shown
    /// </summary>
    [DataField]
    public LocId? Text;

    /// <summary>
    /// The color of the text.
    /// </summary>
    [DataField]
    public Color? Color;

    /// <summary>
    /// The sound played.
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound;
}
