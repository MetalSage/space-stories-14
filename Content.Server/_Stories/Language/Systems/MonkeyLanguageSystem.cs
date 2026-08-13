using Content.Server.Speech.Components;
using Content.Shared._Stories.Language;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared._Stories.Language.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Language.Systems;

public sealed partial class MonkeyLanguageSystem : EntitySystem
{
    [Dependency] private LanguageSystem _language = default!;

    private static readonly ProtoId<LanguagePrototype> MonkeyLanguage = "Monkey";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MonkeyAccentComponent, ComponentInit>(OnMonkeyInit);
        SubscribeLocalEvent<MonkeyAccentComponent, ComponentRemove>(OnMonkeyAccentRemoved);
    }

    private void OnMonkeyInit(EntityUid uid, MonkeyAccentComponent component, ComponentInit args)
    {
        _language.AddLanguage(uid, SharedLanguageSystem.CommonLanguage, addSpoken: false, addUnderstood: true, source: LanguageSource.Monkey);
        _language.AddLanguage(uid, MonkeyLanguage, source: LanguageSource.Monkey);
    }

    private void OnMonkeyAccentRemoved(EntityUid uid, MonkeyAccentComponent component, ComponentRemove args)
    {
        // ComponentRemove also fires while the entity is being deleted, where EnsureComp would
        // try to add a component to a terminating entity.
        if (TerminatingOrDeleted(uid))
            return;

        _language.AddLanguage(uid, SharedLanguageSystem.CommonLanguage, addSpoken: true, addUnderstood: true, source: LanguageSource.Monkey);
    }
}
