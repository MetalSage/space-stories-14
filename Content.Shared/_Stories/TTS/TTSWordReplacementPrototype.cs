using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.TTS;

/// <summary>
/// Prototype for TTS word replacements during text sanitization.
/// </summary>
[Prototype("ttsWordReplacement")]
public sealed partial class TTSWordReplacementPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public Dictionary<string, string> WordReplacements { get; private set; } = new();
}
