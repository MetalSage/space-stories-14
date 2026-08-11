using Content.Shared._Stories.Language;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared.NukeOps;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Language.Systems;

public sealed partial class NuclearOperativeLanguageSystem : EntitySystem
{
    [Dependency] private LanguageSystem _language = default!;

    private static readonly ProtoId<LanguagePrototype> CodespeakLanguage = "Codespeak";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeOperativeComponent, ComponentInit>(OnNukeOperativeInit);
    }

    private void OnNukeOperativeInit(EntityUid uid, NukeOperativeComponent component, ComponentInit args)
    {
        _language.AddLanguage(uid, CodespeakLanguage, source: LanguageSource.NuclearOperative);
    }
}
