using System.Text.RegularExpressions;
using Content.Server._Stories.Speech.Components;
using Content.Server.Speech;
using Robust.Shared.Random;

namespace Content.Server._Stories.Speech.EntitySystems;

public sealed class KidanAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KidanAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, KidanAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // з => зз
        message = Regex.Replace(
            message,
            "з+",
            "зз"
        );
        // З => ЗЗ
        message = Regex.Replace(
            message,
            "З+",
            "ЗЗ"
        );
        // в => вв
        message = Regex.Replace(
            message,
            "в+",
            "вв"
        );
        // В => ВВ
        message = Regex.Replace(
            message,
            "В+",
            "ВВ"
        );
        // c => зз
        message = Regex.Replace(
            message,
            "с+",
            "зз"
        );
        // С => ЗЗ
        message = Regex.Replace(
            message,
            "С+",
            "ЗЗ"
        );
        // ц => зз
        message = Regex.Replace(
            message,
            "ц+",
            "зз"
        );
        // Ц => ЗЗ
        message = Regex.Replace(
            message,
            "Ц+",
            "ЗЗ"
        );

        args.Message = message;
    }
}
