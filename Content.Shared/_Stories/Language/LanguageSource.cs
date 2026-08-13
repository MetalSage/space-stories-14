namespace Content.Shared._Stories.Language;

public static class LanguageSource
{
    public const string Species = "Species";
    public const string Synthetic = "Synthetic";
    public const string Shadowling = "Shadowling";
    public const string NuclearOperative = "NuclearOperative";
    public const string Monkey = "Monkey";
    public const string Relay = "Relay";
    public const string Preset = "Preset";
    public const string Admin = "Admin";
    public const string Learned = "Learned";

    public static readonly IReadOnlySet<string> MindBound = new HashSet<string>
    {
        Admin,
        Learned,
    };
}
