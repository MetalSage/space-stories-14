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
    }

    private void OnSyntheticInit(EntityUid uid, Component component, ComponentInit args)
    {
        _language.AddLanguage(uid, SharedLanguageSystem.CommonLanguage, source: LanguageSource.Synthetic);
        _language.AddLanguage(uid, MachineLanguage, source: LanguageSource.Synthetic);
    }
}
