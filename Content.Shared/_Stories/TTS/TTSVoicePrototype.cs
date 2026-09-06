using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.TTS;

[Prototype("ttsVoice")]
public sealed partial class TTSVoicePrototype : IPrototype
{
    [DataField("name")]
    public string Name { get; private set; } = string.Empty;

    [DataField]
    public string Description { get; private set; } = string.Empty;

    [DataField("sex", required: true)]
    public Sex Sex { get; private set; }

    [ViewVariables(VVAccess.ReadWrite), DataField("speaker", required: true)]
    public string Speaker { get; private set; } = string.Empty;

    [DataField("roundStart")]
    public bool RoundStart { get; private set; } = true;

    [DataField("sponsorOnly")]
    public bool SponsorOnly { get; private set; }

    [IdDataField]
    public string ID { get; private set; } = default!;
}
