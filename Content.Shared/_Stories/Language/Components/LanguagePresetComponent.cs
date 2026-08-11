using Content.Shared._Stories.Language.Prototypes;
using Content.Shared._Stories.Language.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Language.Components;

[RegisterComponent]
[Access(typeof(SharedLanguageSystem))]
public sealed partial class LanguagePresetComponent : Component
{
    [DataField]
    public HashSet<ProtoId<LanguagePrototype>> SpokenLanguages = new();

    [DataField]
    public HashSet<ProtoId<LanguagePrototype>> UnderstoodLanguages = new();

    [DataField]
    public ProtoId<LanguagePrototype>? CurrentLanguage;

    [DataField]
    public ProtoId<LanguagePrototype>? DefaultLanguage;
}
