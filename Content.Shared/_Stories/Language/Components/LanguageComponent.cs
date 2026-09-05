using Content.Shared._Stories.Language.Prototypes;
using Content.Shared._Stories.Language.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Language.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedLanguageSystem))]
public sealed partial class LanguageComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<LanguagePrototype>> SpokenLanguages = new();

    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<LanguagePrototype>> UnderstoodLanguages = new();

    [DataField]
    public Dictionary<ProtoId<LanguagePrototype>, HashSet<string>> SpokenLanguageSources = new();

    [DataField]
    public Dictionary<ProtoId<LanguagePrototype>, HashSet<string>> UnderstoodLanguageSources = new();

    [DataField]
    public Dictionary<ProtoId<LanguagePrototype>, Dictionary<string, float>> PartialUnderstanding = new();

    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<LanguagePrototype>> BlockedSpeaking = new();

    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<LanguagePrototype>> BlockedUnderstanding = new();

    [DataField, AutoNetworkedField]
    public ProtoId<LanguagePrototype>? CurrentLanguage;

    [DataField, AutoNetworkedField]
    public ProtoId<LanguagePrototype>? DefaultLanguage;
}
