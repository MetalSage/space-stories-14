using Content.Shared._Stories.Language;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Language.Systems;

public sealed partial class SpeciesLanguageSystem : EntitySystem
{
    [Dependency] private LanguageSystem _language = default!;
    [Dependency] private IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<HumanoidProfileComponent, MapInitEvent>(OnHumanoidMapInit);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        GrantSpeciesLanguages(args.Mob, args.Profile.Species);
    }

    private void OnHumanoidMapInit(EntityUid uid, HumanoidProfileComponent component, MapInitEvent args)
    {
        GrantSpeciesLanguages(uid, component.Species);
    }

    private void GrantSpeciesLanguages(EntityUid mob, ProtoId<SpeciesPrototype> species)
    {
        if (!_prototype.TryIndex<SpeciesLanguagesPrototype>(species, out var languages))
            return;

        foreach (var language in languages.SpokenLanguages)
            _language.AddLanguage(mob, language, addSpoken: true, addUnderstood: languages.UnderstoodLanguages.Contains(language), source: LanguageSource.Species);

        foreach (var language in languages.UnderstoodLanguages)
        {
            if (!languages.SpokenLanguages.Contains(language))
                _language.AddLanguage(mob, language, addSpoken: false, addUnderstood: true, source: LanguageSource.Species);
        }
    }
}
