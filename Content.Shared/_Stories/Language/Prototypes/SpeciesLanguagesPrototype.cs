using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Language.Prototypes;

[Prototype]
public sealed partial class SpeciesLanguagesPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public HashSet<ProtoId<LanguagePrototype>> SpokenLanguages = new();

    [DataField]
    public HashSet<ProtoId<LanguagePrototype>> UnderstoodLanguages = new();
}
