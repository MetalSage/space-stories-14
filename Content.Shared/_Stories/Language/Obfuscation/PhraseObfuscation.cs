using System.Text;
using Content.Shared._Stories.Language.Systems;

namespace Content.Shared._Stories.Language;

public sealed partial class PhraseObfuscation : ReplacementObfuscation
{
    [DataField]
    public float ClearSentenceThreshold = 0.8f;

    [DataField]
    public int MinPhrases = 1;

    [DataField]
    public int MaxPhrases = 4;

    [DataField]
    public string Separator = " ";

    [DataField]
    public float Proportion = 1f / 3;

    internal override void ObfuscateInternalWithComprehension(
        StringBuilder builder,
        string message,
        SharedLanguageSystem context,
        bool randomize,
        float comprehension)
    {
        if (Replacement.Count == 0)
            return;

        var sentenceProcessor = new SentenceProcessor(
            message,
            context,
            Replacement,
            comprehension,
            randomize,
            ClearSentenceThreshold);
        sentenceProcessor.ProcessSentences(builder, MinPhrases, MaxPhrases, Separator, Proportion);
    }

    private readonly struct SentenceProcessor
    {
        private readonly string _message;
        private readonly SharedLanguageSystem _context;
        private readonly IReadOnlyList<string> _replacement;
        private readonly float _comprehension;
        private readonly bool _randomize;
        private readonly float _clearSentenceThreshold;

        public SentenceProcessor(string message, SharedLanguageSystem context,
            IReadOnlyList<string> replacement, float comprehension, bool randomize, float clearSentenceThreshold)
        {
            _message = message;
            _context = context;
            _replacement = replacement;
            _comprehension = comprehension;
            _randomize = randomize;
            _clearSentenceThreshold = clearSentenceThreshold;
        }

        public void ProcessSentences(StringBuilder builder, int minPhrases, int maxPhrases,
            string separator, float proportion)
        {
            var sentenceBeginIndex = 0;
            var hashCode = 0;

            for (var i = 0; i <= _message.Length; i++)
            {
                var atEnd = i == _message.Length;

                if (!atEnd)
                {
                    var ch = char.ToLowerInvariant(_message[i]);

                    if (!IsSentenceEndPunctuation(ch))
                    {
                        hashCode = hashCode * 31 + ch;
                        continue;
                    }
                }

                // i is exclusive here, so the trailing character of an unpunctuated message
                // is part of the sentence rather than being dropped.
                ProcessSentence(builder, sentenceBeginIndex, i, hashCode,
                    minPhrases, maxPhrases, separator, proportion);

                if (!atEnd)
                    builder.Append(_message[i]);

                hashCode = 0;
                sentenceBeginIndex = i + 1;
            }
        }

        private void ProcessSentence(StringBuilder builder, int sentenceBeginIndex, int sentenceEndIndex,
            int hashCode, int minPhrases, int maxPhrases, string separator, float proportion)
        {
            var length = sentenceEndIndex - sentenceBeginIndex;
            if (length <= 0)
                return;

            var sentence = _message.Substring(sentenceBeginIndex, length);

            if (_comprehension >= _clearSentenceThreshold)
            {
                builder.Append(sentence);
                return;
            }

            var obfuscationIntensity = (1.0f - _comprehension) * (1.0f - _comprehension);
            var basePhrases = CalculateBasePhraseCount(length, minPhrases, maxPhrases, proportion);
            var adjustedPhrases = Math.Max(1, (int) (basePhrases * obfuscationIntensity));

            AppendObfuscatedPhrases(builder, hashCode, adjustedPhrases, separator);
        }

        private static int CalculateBasePhraseCount(int length, int minPhrases, int maxPhrases, float proportion)
        {
            return (int) Math.Clamp(Math.Pow(length, proportion) - 1, minPhrases, maxPhrases);
        }

        private void AppendObfuscatedPhrases(StringBuilder builder, int hashCode, int phraseCount, string separator)
        {
            for (var i = 0; i < phraseCount; i++)
            {
                if (i > 0)
                    builder.Append(separator);

                var phraseIndex = _context.PseudoRandomNumber(hashCode + i, 0, _replacement.Count - 1, _randomize);
                builder.Append(_replacement[phraseIndex]);
            }
        }
    }
}
