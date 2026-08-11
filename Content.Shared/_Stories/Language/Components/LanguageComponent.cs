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

    [DataField, AutoNetworkedField]
    public ProtoId<LanguagePrototype>? CurrentLanguage;

    [DataField, AutoNetworkedField]
    public ProtoId<LanguagePrototype>? DefaultLanguage;

    [DataField, AutoNetworkedField]
    public EntProtoId<LanguagePresetComponent>? Preset;
}
