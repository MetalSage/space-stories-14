using System.Text;
using Content.Shared._Stories.Language.Components;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared.Dataset;
using Content.Shared.Ghost;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Stories.Language.Systems;

public abstract partial class SharedLanguageSystem : EntitySystem
{
    [Dependency] protected IPrototypeManager _prototypeManager = default!;
    [Dependency] private IRobustRandom _random = default!;

    public static readonly ProtoId<LanguagePrototype> CommonLanguage = "GalacticCommon";

    private static readonly ProtoId<DatasetPrototype> CommonWordsDataset = "STLanguageCommonWords";

    private const float CommonWordBonus = 0.2f;
    private const float UncommonWordPenalty = -0.05f;

    private Dictionary<string, int>? _wordRanks;
    private Dictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> _wordRanksLookup;
    private int _roundSeed;

    public float GetWordCommonnessBonus(ReadOnlySpan<char> word)
    {
        EnsureWordRanks();

        if (_wordRanks!.Count == 0)
            return 0f;

        if (!_wordRanksLookup.TryGetValue(word, out var rank))
            return UncommonWordPenalty;

        var falloff = (float) rank / _wordRanks.Count * CommonWordBonus;
        return Math.Max(CommonWordBonus - falloff, UncommonWordPenalty);
    }

    private void EnsureWordRanks()
    {
        if (_wordRanks != null)
            return;

        _wordRanks = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (_prototypeManager.TryIndex(CommonWordsDataset, out var dataset))
        {
            for (var i = 0; i < dataset.Values.Count; i++)
            {
                _wordRanks.TryAdd(dataset.Values[i], i + 1);
            }
        }

        _wordRanksLookup = _wordRanks.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    public ProtoId<LanguagePrototype> GetCurrentLanguage(EntityUid entity)
    {
        return GetCurrentLanguage((entity, CompOrNull<LanguageComponent>(entity)));
    }

    public ProtoId<LanguagePrototype> GetCurrentLanguage(Entity<LanguageComponent?> ent)
    {
        if (!TryGetCurrentLanguage(ent, out var language))
            return CommonLanguage;

        return language;
    }

    public bool TryGetCurrentLanguage(Entity<LanguageComponent?> ent, out ProtoId<LanguagePrototype> language)
    {
        if (!Resolve(ent, ref ent.Comp, false))
        {
            language = default!;
            return false;
        }

        if (ent.Comp.CurrentLanguage is { } current)
        {
            language = current;
            return true;
        }

        if (ent.Comp.DefaultLanguage is { } fallback && ent.Comp.SpokenLanguages.Contains(fallback))
        {
            language = fallback;
            return true;
        }

        language = CommonLanguage;
        return true;
    }

    public bool CanSpeak(EntityUid entity, ProtoId<LanguagePrototype> language)
    {
        return CanSpeak((entity, CompOrNull<LanguageComponent>(entity)), language);
    }

    public bool CanSpeak(Entity<LanguageComponent?> ent, ProtoId<LanguagePrototype> language)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return language == CommonLanguage;

        return ent.Comp.SpokenLanguages.Contains(language);
    }

    public bool CanUnderstand(EntityUid entity, ProtoId<LanguagePrototype> language)
    {
        return CanUnderstand((entity, CompOrNull<LanguageComponent>(entity)), language);
    }

    public bool CanUnderstand(Entity<LanguageComponent?> ent, ProtoId<LanguagePrototype> language)
    {
        if (HasComp<GhostComponent>(ent))
            return true;

        if (!Resolve(ent, ref ent.Comp, false))
            return language == CommonLanguage;

        return ent.Comp.UnderstoodLanguages.Contains(language);
    }

    public float GetComprehension(EntityUid entity, ProtoId<LanguagePrototype> language)
    {
        return GetComprehension((entity, CompOrNull<LanguageComponent>(entity)), language);
    }

    public float GetComprehension(Entity<LanguageComponent?> ent, ProtoId<LanguagePrototype> language)
    {
        if (CanUnderstand(ent, language))
            return 1f;

        if (!Resolve(ent, ref ent.Comp, false))
            return 0f;

        if (ent.Comp.BlockedUnderstanding.Contains(language))
            return 0f;

        var best = 0f;
        foreach (var known in ent.Comp.UnderstoodLanguages)
        {
            if (!_prototypeManager.TryIndex(known, out var knownProto))
                continue;

            if (knownProto.MutualUnderstanding.TryGetValue(language, out var comprehension) && comprehension > best)
                best = comprehension;
        }

        if (ent.Comp.PartialUnderstanding.TryGetValue(language, out var granted))
        {
            foreach (var amount in granted.Values)
            {
                if (amount > best)
                    best = amount;
            }
        }

        return Math.Clamp(best, 0f, 1f);
    }

    public string ColorizeMessage(string escapedMessage, ProtoId<LanguagePrototype> language)
    {
        if (!_prototypeManager.TryIndex(language, out var languageProto) || languageProto.SpeechOverride.Color is not { } color)
            return escapedMessage;

        return $"[color={color.ToHex()}]{escapedMessage}[/color]";
    }

    public string LanguageIconMarkup(ProtoId<LanguagePrototype> language, float comprehension = 1f)
    {
        if (!_prototypeManager.TryIndex(language, out var languageProto) || languageProto.DisplayedLanguageIcon is not { } icon)
            return string.Empty;

        var partial = comprehension is > 0f and < 1f;
        return $"[langicon language=\"{icon}\" partial=\"{(partial ? "true" : "false")}\"][/langicon]";
    }

    public string ObfuscateMessage(string message, ProtoId<LanguagePrototype> language, float comprehension = 0f)
    {
        if (comprehension >= 1f)
            return message;

        if (!_prototypeManager.TryIndex(language, out var languageProto))
            return message;

        return ObfuscateMessageInternalWithComprehension(message, languageProto.ObfuscationMethod, languageProto.RandomizeObfuscation, comprehension);
    }

    protected string ObfuscateMessageInternalWithComprehension(
        string message,
        ObfuscationMethod obfuscationMethod,
        bool randomize,
        float comprehension)
    {
        var builder = new StringBuilder(message.Length);
        obfuscationMethod.ObfuscateInternalWithComprehension(builder, message, this, randomize, comprehension);
        return builder.ToString();
    }

    protected void ReseedObfuscationForRound()
    {
        _roundSeed = _random.Next();
        _wordRanks = null;
    }

    private static uint CombineSeed(int seed, int roundSeed)
    {
        unchecked
        {
            var x = (uint) seed ^ (uint) (roundSeed * 397);
            x ^= x >> 16;
            x *= 0x7feb352d;
            x ^= x >> 15;
            x *= 0x846ca68b;
            x ^= x >> 16;
            return x;
        }
    }

    public int PseudoRandomNumber(int seed, int min, int max)
    {
        return PseudoRandomNumber(seed, min, max, false);
    }

    public int PseudoRandomNumber(int seed, int min, int max, bool randomize)
    {
        if (min >= max)
            return min;

        var range = (long) max - min + 1;

        if (randomize)
            return (int) (min + (uint) _random.Next() % range);

        return (int) (min + CombineSeed(seed, _roundSeed) % range);
    }
}
