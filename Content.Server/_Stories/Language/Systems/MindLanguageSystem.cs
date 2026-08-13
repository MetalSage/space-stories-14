using Content.Shared._Stories.Language;
using Content.Shared._Stories.Language.Components;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared.Mind.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Language.Systems;

public sealed partial class MindLanguageSystem : EntitySystem
{
    [Dependency] private LanguageSystem _language = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindContainerComponent, MindAddedMessage>(OnMindAdded);
    }

    private void OnMindAdded(Entity<MindContainerComponent> ent, ref MindAddedMessage args)
    {
        if (args.TransferEntity is not { } previous || previous == ent.Owner)
            return;

        if (!TryComp<LanguageComponent>(previous, out var from))
            return;

        TransferMindBound(previous, from, ent.Owner);
    }

    private void TransferMindBound(EntityUid previous, LanguageComponent from, EntityUid target)
    {
        foreach (var (language, sources) in CopySources(from.SpokenLanguageSources))
        {
            foreach (var source in sources)
            {
                _language.AddLanguage(target, language, addSpoken: true, addUnderstood: false, source: source);
                _language.RemoveLanguage(previous, language, removeSpoken: true, removeUnderstood: false, source: source);
            }
        }

        foreach (var (language, sources) in CopySources(from.UnderstoodLanguageSources))
        {
            foreach (var source in sources)
            {
                _language.AddLanguage(target, language, addSpoken: false, addUnderstood: true, source: source);
                _language.RemoveLanguage(previous, language, removeSpoken: false, removeUnderstood: true, source: source);
            }
        }

        foreach (var (language, sources) in CopyPartial(from.PartialUnderstanding))
        {
            foreach (var (source, amount) in sources)
            {
                _language.AddPartialUnderstanding(target, language, amount, source);
                _language.RemovePartialUnderstanding(previous, language, source);
            }
        }
    }

    private static List<(ProtoId<LanguagePrototype>, List<string>)> CopySources(
        Dictionary<ProtoId<LanguagePrototype>, HashSet<string>> sources)
    {
        var result = new List<(ProtoId<LanguagePrototype>, List<string>)>();

        foreach (var (language, owners) in sources)
        {
            var bound = new List<string>();
            foreach (var owner in owners)
            {
                if (LanguageSource.MindBound.Contains(owner))
                    bound.Add(owner);
            }

            if (bound.Count > 0)
                result.Add((language, bound));
        }

        return result;
    }

    private static List<(ProtoId<LanguagePrototype>, List<(string, float)>)> CopyPartial(
        Dictionary<ProtoId<LanguagePrototype>, Dictionary<string, float>> partial)
    {
        var result = new List<(ProtoId<LanguagePrototype>, List<(string, float)>)>();

        foreach (var (language, owners) in partial)
        {
            var bound = new List<(string, float)>();
            foreach (var (source, amount) in owners)
            {
                if (LanguageSource.MindBound.Contains(source))
                    bound.Add((source, amount));
            }

            if (bound.Count > 0)
                result.Add((language, bound));
        }

        return result;
    }
}
