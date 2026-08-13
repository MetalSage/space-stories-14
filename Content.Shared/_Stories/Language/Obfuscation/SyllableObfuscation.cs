using System.Text;
using Content.Shared._Stories.Language.Systems;

namespace Content.Shared._Stories.Language;

public sealed partial class SyllableObfuscation : ReplacementObfuscation
{
    private const char EndOfFile = (char) 0;

    [DataField]
    public int AdditionalLengthLow = -1;

    [DataField]
    public int AdditionalLengthHigh = 3;

    [DataField]
    public float SpaceChance = 0.2f;

    [DataField]
    public List<string> SpecialCharacters = new();

    internal override void ObfuscateInternalWithComprehension(
        StringBuilder builder,
        string message,
        SharedLanguageSystem context,
        bool randomize,
        float comprehension)
    {
        if (Replacement.Count == 0)
            return;

        var wordProcessor = new WordProcessor(message, context, Replacement, SpecialCharacters, comprehension, randomize);
        wordProcessor.ProcessWords(builder, this);
    }

    private readonly struct WordProcessor
    {
        private readonly string _message;
        private readonly SharedLanguageSystem _context;
        private readonly IReadOnlyList<string> _replacement;
        private readonly IReadOnlyList<string> _specialCharacters;
        private readonly float _comprehension;
        private readonly bool _randomize;

        public WordProcessor(string message, SharedLanguageSystem context,
            IReadOnlyList<string> replacement, IReadOnlyList<string> specialCharacters,
            float comprehension, bool randomize)
        {
            _message = message;
            _context = context;
            _replacement = replacement;
            _specialCharacters = specialCharacters;
            _comprehension = comprehension;
            _randomize = randomize;
        }

        public void ProcessWords(StringBuilder builder, SyllableObfuscation settings)
        {
            var wordBeginIndex = 0;
            var hashCode = 0;
            var sentenceStart = true;

            for (var i = 0; i <= _message.Length; i++)
            {
                var ch = i < _message.Length ? char.ToLowerInvariant(_message[i]) : EndOfFile;
                var isWordEnd = char.IsWhiteSpace(ch) || IsPunctuation(ch) || ch == EndOfFile;

                if (!isWordEnd)
                {
                    hashCode = hashCode * 31 + ch;
                    continue;
                }

                ProcessWord(builder, wordBeginIndex, i, hashCode, settings, ref sentenceStart);

                if (isWordEnd && ch != EndOfFile)
                {
                    builder.Append(ch);
                    if (IsSentenceEndPunctuation(ch))
                        sentenceStart = true;
                }

                hashCode = 0;
                wordBeginIndex = i + 1;
            }
        }

        private void ProcessWord(StringBuilder builder, int wordBeginIndex, int wordEndIndex,
            int hashCode, SyllableObfuscation settings, ref bool sentenceStart)
        {
            var wordLength = wordEndIndex - wordBeginIndex;
            if (wordLength <= 0)
                return;

            if (_comprehension > 0f && WordUnderstood(hashCode, wordBeginIndex, wordLength))
            {
                builder.Append(_message, wordBeginIndex, wordLength);
                sentenceStart = false;
                return;
            }

            var shouting = IsShouting(wordBeginIndex, wordLength);
            ObfuscateWordCompletely(builder, hashCode, wordLength, settings, shouting, sentenceStart);
            sentenceStart = false;
        }

        private bool IsShouting(int wordBeginIndex, int wordLength)
        {
            if (wordLength < 2)
                return false;

            var hasLetter = false;
            for (var i = 0; i < wordLength; i++)
            {
                var ch = _message[wordBeginIndex + i];
                if (!char.IsLetter(ch))
                    continue;

                hasLetter = true;
                if (char.IsLower(ch))
                    return false;
            }

            return hasLetter;
        }

        private bool WordUnderstood(int hashCode, int wordBeginIndex, int wordLength)
        {
            var word = _message.Substring(wordBeginIndex, wordLength).ToLowerInvariant();
            var chance = Math.Clamp(_comprehension + _context.GetWordCommonnessBonus(word), 0f, 1f);

            var roll = _context.PseudoRandomNumber(hashCode, 0, 999, _randomize);
            return roll < (int) (chance * 1000f);
        }

        private void ObfuscateWordCompletely(StringBuilder builder, int hashCode, int wordLength,
            SyllableObfuscation settings, bool shouting, bool capitalizeFirst)
        {
            var slack = _context.PseudoRandomNumber(hashCode, settings.AdditionalLengthLow, settings.AdditionalLengthHigh, _randomize);
            var targetLength = Math.Max(1, wordLength + slack);
            var startIndex = builder.Length;

            AppendRandomSyllables(builder, hashCode, targetLength, settings);

            if (shouting)
            {
                for (var i = startIndex; i < builder.Length; i++)
                    builder[i] = char.ToUpperInvariant(builder[i]);
            }
            else if (capitalizeFirst && builder.Length > startIndex)
            {
                builder[startIndex] = char.ToUpperInvariant(builder[startIndex]);
            }
        }

        private const int SyllableSafetyCap = 24;

        private void AppendRandomSyllables(StringBuilder builder, int hashCode, int targetLength, SyllableObfuscation settings)
        {
            var startIndex = builder.Length;
            var syllables = 0;

            while (syllables < SyllableSafetyCap
                   && (syllables == 0 || builder.Length - startIndex < targetLength))
            {
                if (syllables > 0)
                    AppendSyllableSeparator(builder, hashCode, syllables, settings);

                var index = _context.PseudoRandomNumber(hashCode + syllables, 0, _replacement.Count - 1, _randomize);
                builder.Append(_replacement[index]);
                syllables++;
            }
        }

        private void AppendSyllableSeparator(StringBuilder builder, int hashCode, int syllable, SyllableObfuscation settings)
        {
            var roll = _context.PseudoRandomNumber(hashCode * 31 + syllable, 0, 999, _randomize);

            if (roll < (int) (settings.SpaceChance * 1000f))
            {
                builder.Append(' ');
                return;
            }

            if (_specialCharacters.Count == 0)
                return;

            if (roll >= 990)
            {
                var index = _context.PseudoRandomNumber(hashCode - syllable, 0, _specialCharacters.Count - 1, _randomize);
                builder.Append(_specialCharacters[index]);
            }
        }
    }
}
