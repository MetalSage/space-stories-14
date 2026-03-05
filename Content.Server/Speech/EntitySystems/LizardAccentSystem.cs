using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class LizardAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerS = new("s+", RegexOptions.Compiled);
    private static readonly Regex RegexUpperS = new("S+", RegexOptions.Compiled);
    private static readonly Regex RegexInternalX = new(@"(\w)x", RegexOptions.Compiled);
    private static readonly Regex RegexLowerEndX = new(@"\bx([\-|r|R]|\b)", RegexOptions.Compiled);
    private static readonly Regex RegexUpperEndX = new(@"\bX([\-|r|R]|\b)", RegexOptions.Compiled);

    private static readonly Regex RegexCyrLowerS = new("с+", RegexOptions.Compiled);
    private static readonly Regex RegexCyrUpperS = new("С+", RegexOptions.Compiled);
    private static readonly Regex RegexCyrLowerZ = new("з+", RegexOptions.Compiled);
    private static readonly Regex RegexCyrUpperZ = new("З+", RegexOptions.Compiled);
    private static readonly Regex RegexCyrLowerSh = new("ш+", RegexOptions.Compiled);
    private static readonly Regex RegexCyrUpperSh = new("Ш+", RegexOptions.Compiled);
    private static readonly Regex RegexCyrLowerCh = new("ч+", RegexOptions.Compiled);
    private static readonly Regex RegexCyrUpperCh = new("Ч+", RegexOptions.Compiled);

    [Dependency] private readonly IRobustRandom _random = default!; // Corvax-Localization

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LizardAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LizardAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // hissss
        message = RegexLowerS.Replace(message, "sss");
        // hiSSS
        message = RegexUpperS.Replace(message, "SSS");
        // ekssit
        message = RegexInternalX.Replace(message, "$1kss");
        // ecks
        message = RegexLowerEndX.Replace(message, "ecks$1");
        // eckS
        message = RegexUpperEndX.Replace(message, "ECKS$1");

        // Corvax-Localization-Start
        // c => ссс
        message = RegexCyrLowerS.Replace(
            message,
            _random.Pick(new List<string>() { "сс", "ссс" })
        );
        // С => CCC
        message = RegexCyrUpperS.Replace(
            message,
            _random.Pick(new List<string>() { "СС", "ССС" })
        );
        // з => ссс
        message = RegexCyrLowerZ.Replace(
            message,
            _random.Pick(new List<string>() { "сс", "ссс" })
        );
        // З => CCC
        message = RegexCyrUpperZ.Replace(
            message,
            _random.Pick(new List<string>() { "СС", "ССС" })
        );
        // ш => шшш
        message = RegexCyrLowerSh.Replace(
            message,
            _random.Pick(new List<string>() { "шш", "шшш" })
        );
        // Ш => ШШШ
        message = RegexCyrUpperSh.Replace(
            message,
            _random.Pick(new List<string>() { "ШШ", "ШШШ" })
        );
        // ч => щщщ
        message = RegexCyrLowerCh.Replace(
            message,
            _random.Pick(new List<string>() { "щщ", "щщщ" })
        );
        // Ч => ЩЩЩ
        message = RegexCyrUpperCh.Replace(
            message,
            _random.Pick(new List<string>() { "ЩЩ", "ЩЩЩ" })
        );
        // Corvax-Localization-End
        args.Message = message;
    }
}
