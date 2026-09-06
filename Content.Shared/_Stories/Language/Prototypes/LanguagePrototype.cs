using Content.Shared._Stories.Language;
using Content.Shared.Speech;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Stories.Language.Prototypes;

[Prototype]
public sealed partial class LanguagePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Name = default!;

    [DataField]
    public string? Description;

    [DataField]
    public bool IsVisibleLanguage;

    [DataField]
    public string? TypefaceId;

    [DataField]
    public int? TextSize;

    [DataField]
    public bool ShowLanguageName;

    [DataField]
    public bool ShowLanguageIcon = true;

    [DataField]
    public SpriteSpecifier? LanguageIcon;

    [DataField]
    public int Priority;

    [DataField]
    public bool CanUseRadio = true;

    [DataField]
    public bool NeedsSpeech = true;

    [DataField]
    public bool NeedsLOS;

    [DataField]
    public ObfuscationMethod ObfuscationMethod = ObfuscationMethod.Default;

    [DataField]
    public bool RandomizeObfuscation;

    [DataField]
    public SpeechOverrideInfo SpeechOverride = new();

    [DataField]
    public Dictionary<ProtoId<LanguagePrototype>, float> MutualUnderstanding = new();

    public string LocalizedName => Loc.GetString($"language-{ID}-name");
    public string ChatName => Loc.GetString($"chat-language-{ID}-name");
    public string? LocalizedDescription => Description == null ? null : Loc.GetString($"language-{ID}-description");
    public string? DisplayedLanguageIcon => ShowLanguageIcon ? ID : null;
}

[DataDefinition]
public sealed partial class SpeechOverrideInfo
{
    [DataField]
    public Color? Color;

    [DataField]
    public InGameICChatType? ChatTypeOverride;

    [DataField]
    public List<LocId>? SpeechVerbOverrides;

    [DataField]
    public ProtoId<SpeechSoundsPrototype>? SpeechSoundsOverride;

    [DataField]
    public Dictionary<InGameICChatType, LocId> MessageWrapOverrides = new();
}

[Serializable, NetSerializable]
public enum InGameICChatType : byte
{
    Speak,
    Emote,
    Whisper
}
