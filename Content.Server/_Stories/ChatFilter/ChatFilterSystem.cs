using System.Linq;
using System.Text;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Shared.Database;

namespace Content.Server._Stories.ChatFilter;

public sealed class ChatFilterSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    private static readonly Dictionary<string, string> SlangReplace = new()
    {
        // Game
        { "кк", "красный код" },
        { "ск", "синий код" },
        { "зк", "зелёный код" },
        { "инжинер", "инженер" },
        { "инжинеру", "инженеру" },
        { "инжинера", "инженера" },
        { "инжинерам", "инженерам" },
        { "инжинерами", "инженерами" },
        { "инжинеры", "инженеры" },
        { "инжинеров", "инженеров" },
        { "дизарм", "толчок" },
        { "дизарму", "толчку" },
        { "дизарма", "толчка" },
        { "дизармам", "толчкам" },
        { "дизармамм", "толчкамм" },
        { "дизармы", "толчки" },
        { "дизармов", "толчков" },
        { "яо", "ядерный оперативник" },
        { "яой", "ядерный оперативник" },
        { "нюку", "ядерного оперативника" },
        { "нюка", "ядерный оперативник" },
        { "нюкам", "ядерным оперативникам" },
        { "нюками", "ядерными оперативниками" },
        { "нюки", "ядерные оперативники" },
        { "нюк", "ядерных оперативников" },
        { "нюкер", "ядерный оперативник" },
        { "нюкеру", "ядерному оперативнику" },
        { "нюкера", "ядерного оперативника" },
        { "нюкеры", "ядерные оперативники" },
        { "нюкеров", "ядерных оперативников" },
        { "нюкерам", "ядерным оперативникам" },
        { "нюкерами", "ядерным оперативниками" },
        { "сингу", "сингулярность" },
        { "синга", "сингулярность" },
        { "сингам", "сингулярностям" },
        { "сингами", "сингулярностями" },
        { "синги", "сингулярности" },
        { "дек", "детектив" },
        { "деку", "детективу" },
        { "дека", "детектива" },
        { "декам", "детективам" },
        { "деками", "детективами" },
        { "деки", "детективы" },
        { "деков", "детективов" },
        { "вард", "смотритель" },
        { "варден", "смотритель" },
        { "вардену", "смотрителю" },
        { "вардена", "смотрителя" },
        { "варденам", "смотрителям" },
        { "варденами", "смотрителями" },
        { "вардены", "смотрители" },
        { "варденов", "смотрителей" },
        { "разгерм", "разгерметизация" },
        { "разгерму", "разгерметизацию" },
        { "разгерма", "разгерметизация" },
        { "разгермам", "разгерметизациям" },
        { "разгермами", "разгерметизациями" },
        { "разгермы", "разгерметизации" },
        { "разгермой", "разгерметизацией" },
        { "синд", "синдикат" },
        { "дефиб", "дефибриллятор" },
        { "шатл", "шаттл" },
        { "шатлу", "шаттлу" },
        { "шатла", "шаттла" },
        { "шатлам", "шаттлам" },
        { "шатлами", "шаттлами" },
        { "шатлы", "шаттлы" },
        { "шатлов", "шаттлов" },
        // IC
        { "кст", "кстати" },
        { "плз", "пожалуйста" },
        { "пж", "пожалуйста" },
        { "спс", "спасибо" },
        { "сяб", "спасибо" },
        { "прив", "привет" },
        { "ок", "окей" },
        { "лан", "ладно" },
        { "збс", "заебись" },
        { "мб", "может быть" },
        { "омг", "боже мой" },
        { "нзч", "не за что" },
        { "ясн", "ясно" },
        { "всм", "всмысле" },
        { "чзх", "что за херня?" },
        { "гг", "хорошо сработано" },
        { "брух", "мда..." },
        { "хилл", "лечение" },
        { "хиллить", "лечить" },
        { "хильни", "полечи" },
        { "похиль", "полечи" },
        { "похилить", "полечить" },
        { "подхиль", "подлечи" },
        { "подхилить", "полечить" },
        { "хелп", "помоги" },
        { "хелпани", "помоги" },
        { "хелпанул", "помог" },
        { "крч", "короче говоря" },
        // OOC
        { "афк", "ссд" },
        { "набегатор", "грейтардер" },
        { "админ", "бог" },
        { "админу", "богу" },
        { "админа", "бога" },
        { "админам", "богам" },
        { "админами", "богами" },
        { "админы", "боги" },
        { "админов", "богов" },
        { "забанят", "покарают" },
        { "бан", "наказание" },
        { "нонрп", "плохо" },
        { "нрп", "плохо" },
        { "ерп", "ужас" },
        { "рдм", "плохо" },
        { "дм", "плохо" },
        { "гриф", "плохо" },
        { "фрикил", "плохо" },
        { "фрикилл", "плохо" },
        { "лкм", "левая рука" },
        { "пкм", "правая рука" }
    };

    private static readonly HashSet<string> BanPhrases = new()
    {
        "слава украине", "славаукраине", "слава россии", "славароссии",
        "пидр", "педр", "пидор", "пидар", "педар", "педик",
        "даун",
        "ватник",
        "хохол", "хохл",
        "нигер", "негр", "ниггер", "негер", "нигир",
        "куколд",
        "чурк", "чурок"
    };

    private static readonly HashSet<char> UnnecessaryChars = new()
    {
        '+', '/', '*', '=',
        '&',
        '\\',
        '_',
        '[', ']',
        '{', '}',
        '\"'
    };

    private bool IsContainsBanWords(string message, out string matchedBanWord)
    {
        matchedBanWord = string.Empty;
        if (string.IsNullOrEmpty(message))
            return false;

        var cleanSB = new StringBuilder(message.Length);
        foreach (char ch in message)
        {
            if (char.IsLetter(ch))
                cleanSB.Append(char.ToLowerInvariant(ch));
            else if (ch == ' ')
                cleanSB.Append(' ');
        }

        var cleanWords = cleanSB.ToString().Split().ToHashSet();

        foreach (string banPhrase in BanPhrases)
        {
            var banWords = banPhrase.Split();
            var foundBanWords = 0;

            foreach (var banWord in banWords)
            {
                foreach (var word in cleanWords)
                {
                    if (word.StartsWith(banWord, StringComparison.OrdinalIgnoreCase))
                    {
                        foundBanWords++;
                        break;
                    }
                }
            }

            if (foundBanWords >= banWords.Length)
            {
                matchedBanWord = banPhrase;
                return true;
            }
        }

        return false;
    }

    public string ReplaceWords(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var clearMessage = new string(message.Where(ch => !UnnecessaryChars.Contains(ch)).ToArray());

        var whiteSpaceMessage = string.Join(' ', clearMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return string.Join(" ", whiteSpaceMessage
            .Split()
            .Select(currentWord =>
            {
                var lowerWord = currentWord.ToLower();
                return SlangReplace.GetValueOrDefault(lowerWord, currentWord);
            }));
    }

    public void CatchBanWord(ref string message)
    {
        if (!IsContainsBanWords(message, out _))
            return;

        message = "кхем-кхем...";
    }

    public void CatchBanWord(EntityUid source, ref string message)
    {
        if (!IsContainsBanWords(message, out var banWord))
            return;

        _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(source):user} say ban word {banWord}");
        message = "кхем-кхем...";
    }

    public void CatchBanWord(EntityUid source, ref string message, ref InGameICChatType desiredType)
    {
        if (!IsContainsBanWords(message, out var banWord))
            return;

        _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(source):user} say ban word {banWord}");
        message = "кашляет";
        desiredType = InGameICChatType.Emote;
    }
}
