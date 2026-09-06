using Content.Shared._Stories.Language;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared._Stories.Language.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Language.Systems;

public sealed partial class SyntheticLanguageSystem : EntitySystem
{
    [Dependency] private LanguageSystem _language = default!;

    private static readonly ProtoId<LanguagePrototype> MachineLanguage = "Machine";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgChassisComponent, ComponentInit>(OnSyntheticInit);
        SubscribeLocalEvent<StationAiHeldComponent, ComponentInit>(OnSyntheticInit);
        SubscribeLocalEvent<BorgChassisComponent, ComponentRemove>(OnSyntheticRemoved);
        SubscribeLocalEvent<StationAiHeldComponent, ComponentRemove>(OnSyntheticRemoved);
    }

    private void OnSyntheticInit(EntityUid uid, Component component, ComponentInit args)
    {
        _language.AddLanguage(uid, SharedLanguageSystem.CommonLanguage, source: LanguageSource.Synthetic);
        _language.AddLanguage(uid, MachineLanguage, source: LanguageSource.Synthetic);
    }

    private void OnSyntheticRemoved(EntityUid uid, Component component, ComponentRemove args)
    {
        if (TerminatingOrDeleted(uid))
            return;

        _language.RemoveLanguage(uid, SharedLanguageSystem.CommonLanguage, source: LanguageSource.Synthetic);
        _language.RemoveLanguage(uid, MachineLanguage, source: LanguageSource.Synthetic);
    }
}
