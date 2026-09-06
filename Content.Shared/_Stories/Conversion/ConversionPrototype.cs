using Content.Shared.Mobs;
using Content.Shared.StatusIcon;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Stories.Conversion;

[Prototype]
public sealed partial class ConversionPrototype : IPrototype
{
    [ViewVariables, IdDataField]  public string ID { get; private set; } = default!;

    #region Other

    [DataField("statusIcon")]
    public ProtoId<FactionIconPrototype>? StatusIcon;

    [DataField("channels")]
    public HashSet<string> Channels = new();

    [DataField]
    public float? Duration;

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
    public List<EntProtoId>? MindRoles;

    #endregion
}

[DataDefinition]
public partial struct ConversionBriefingData
{
    [DataField]
    public LocId? Text;

    [DataField]
    public Color? Color;

    [DataField]
    public SoundSpecifier? Sound;
}
