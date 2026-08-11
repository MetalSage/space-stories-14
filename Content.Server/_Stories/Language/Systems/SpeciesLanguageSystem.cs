using Content.Shared._Stories.Language.Prototypes;
using Content.Shared.GameTicking;
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
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        if (!_prototype.TryIndex<SpeciesLanguagesPrototype>(args.Profile.Species, out var languages))
            return;

        foreach (var language in languages.SpokenLanguages)
            _language.AddLanguage(args.Mob, language, addSpoken: true, addUnderstood: languages.UnderstoodLanguages.Contains(language));

        foreach (var language in languages.UnderstoodLanguages)
        {
            if (!languages.SpokenLanguages.Contains(language))
                _language.AddLanguage(args.Mob, language, addSpoken: false, addUnderstood: true);
        }
    }
}
