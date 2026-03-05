using System.Text.RegularExpressions;
using Content.Server._Stories.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server._Stories.Speech.EntitySystems;

public sealed class KidanAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerZ = new("з+", RegexOptions.Compiled);
    private static readonly Regex RegexUpperZ = new("З+", RegexOptions.Compiled);
    private static readonly Regex RegexLowerV = new("в+", RegexOptions.Compiled);
    private static readonly Regex RegexUpperV = new("В+", RegexOptions.Compiled);
    private static readonly Regex RegexLowerC = new("с+", RegexOptions.Compiled);
    private static readonly Regex RegexUpperC = new("С+", RegexOptions.Compiled);
    private static readonly Regex RegexLowerTs = new("ц+", RegexOptions.Compiled);
    private static readonly Regex RegexUpperTs = new("Ц+", RegexOptions.Compiled);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KidanAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, KidanAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = RegexLowerZ.Replace(message, "зз");
        message = RegexUpperZ.Replace(message, "ЗЗ");

        message = RegexLowerV.Replace(message, "вв");
        message = RegexUpperV.Replace(message, "ВВ");

        message = RegexLowerC.Replace(message, "зз");
        message = RegexUpperC.Replace(message, "ЗЗ");

        message = RegexLowerTs.Replace(message, "зз");
        message = RegexUpperTs.Replace(message, "ЗЗ");

        args.Message = message;
    }
}
