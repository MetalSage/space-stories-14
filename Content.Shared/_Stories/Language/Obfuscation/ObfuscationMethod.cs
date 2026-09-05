using System.Text;
using Content.Shared._Stories.Language.Systems;

namespace Content.Shared._Stories.Language;

[ImplicitDataDefinitionForInheritors]
public abstract partial class ObfuscationMethod
{
    public static readonly ObfuscationMethod Default = new ReplacementObfuscation
    {
        Replacement = new List<string> { "<?>" }
    };

    internal abstract void ObfuscateInternalWithComprehension(
        StringBuilder builder,
        string message,
        SharedLanguageSystem context,
        bool randomize,
        float comprehension);

    protected static bool IsPunctuation(char ch) => ch is '.' or '!' or '?' or ',' or ':';

    protected static bool IsSentenceEndPunctuation(char ch) => ch is '.' or '!' or '?';
}
