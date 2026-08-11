using System.Text;
using Content.Shared._Stories.Language.Systems;

namespace Content.Shared._Stories.Language;

public sealed partial class SyllableObfuscation : ReplacementObfuscation
{
    private const char EndOfFile = (char) 0;

    [DataField]
    public int MinSyllables = 1;

    [DataField]
    public int MaxSyllables = 4;

    internal override void ObfuscateInternalWithComprehension(
        StringBuilder builder,
        string message,
        SharedLanguageSystem context,
        bool randomize,
        float comprehension)
    {
        if (Replacement.Count == 0)
            return;

        var wordProcessor = new WordProcessor(message, context, Replacement, comprehension, randomize);
        wordProcessor.ProcessWords(builder, MinSyllables, MaxSyllables);
    }

    private readonly struct WordProcessor
    {
        private readonly string _message;
        private readonly SharedLanguageSystem _context;
        private readonly IReadOnlyList<string> _replacement;
        private readonly float _comprehension;
        private readonly bool _randomize;

        public WordProcessor(string message, SharedLanguageSystem context,
            IReadOnlyList<string> replacement, float comprehension, bool randomize)
        {
            _message = message;
            _context = context;
            _replacement = replacement;
            _comprehension = comprehension;
            _randomize = randomize;
        }

        public void ProcessWords(StringBuilder builder, int minSyllables, int maxSyllables)
        {
            var wordBeginIndex = 0;
            var hashCode = 0;

            for (var i = 0; i <= _message.Length; i++)
            {
                var ch = i < _message.Length ? char.ToLowerInvariant(_message[i]) : EndOfFile;
                var isWordEnd = char.IsWhiteSpace(ch) || IsPunctuation(ch) || ch == EndOfFile;

                if (!isWordEnd)
                {
                    hashCode = hashCode * 31 + ch;
                    continue;
                }

                ProcessWord(builder, wordBeginIndex, i, hashCode, minSyllables, maxSyllables);

                if (isWordEnd && ch != EndOfFile)
                    builder.Append(ch);

                hashCode = 0;
                wordBeginIndex = i + 1;
            }
        }

        private void ProcessWord(StringBuilder builder, int wordBeginIndex, int wordEndIndex,
            int hashCode, int minSyllables, int maxSyllables)
        {
            var wordLength = wordEndIndex - wordBeginIndex;
            if (wordLength <= 0)
                return;

            if (_comprehension > 0f && WordUnderstood(hashCode))
            {
                builder.Append(_message, wordBeginIndex, wordLength);
                return;
            }

            ObfuscateWordCompletely(builder, hashCode, minSyllables, maxSyllables);
        }

        private bool WordUnderstood(int hashCode)
        {
            var roll = _context.PseudoRandomNumber(hashCode, 0, 999, _randomize);
            return roll < (int) (_comprehension * 1000f);
        }

        private void ObfuscateWordCompletely(StringBuilder builder, int hashCode, int minSyllables, int maxSyllables)
        {
            var syllableCount = _context.PseudoRandomNumber(hashCode, minSyllables, maxSyllables, _randomize);
            AppendRandomSyllables(builder, hashCode, syllableCount);
        }

        private void AppendRandomSyllables(StringBuilder builder, int hashCode, int syllableCount)
        {
            for (var i = 0; i < syllableCount; i++)
            {
                var index = _context.PseudoRandomNumber(hashCode + i, 0, _replacement.Count - 1, _randomize);
                builder.Append(_replacement[index]);
            }
        }
    }
}
