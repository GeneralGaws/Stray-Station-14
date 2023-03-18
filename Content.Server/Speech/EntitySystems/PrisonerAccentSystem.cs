using System.Globalization;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class PrisonerAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly Dictionary<string, string> DirectReplacements = new()
    {
        { "пидорас", "петух" },
        { "перма", "хата" },
        { "еда", "баланда" },
        { "пища", "жрачка" },
        { "мотоцикл", "агрегат" },
        { "айди", "ксиву" },
        { "варден", "начальник" },
        { "химик", "банкир"},
        { "гсб", "барин" },
        { "карго", "барыги"},
        { "сотрудник", "кент"},
        { "сб", "волчара" },
        { "щиткьюрити", "волчары позорные" },
        { "оружие", "волына" },
        { "жопу", "гудок" },
        { "уборщик", "шнырь" },
        { "уборщика", "шныря" },
        { "сбшник", "мент"},
        { "дет", "тихушник" },
        { "заебался", "сейчас бы раскумариться" },
        { "удо", "закосить" },
        { "космо", "дурь" },
        { "выпить", "чифир бы" },
        { "отдел", "масть" },
        { "обыск", "шмон" },
        { "обыщите", "обшмонайте"},
        { "обыскивайте", "шмонайте" },
        { "кинул меня", "заложил меня"},
        { "кинул его", "заложил его"},
        { "выебываешься", "понтуешься" },
        { "выебыватсья", "понтоваться" },
        { "кредиты", "бабки" },
        { "кредитов", "бабок" },
        { "скрытно", "втихаря" },
        { "стелс", "втихаря" },
        { "беги", "делай ноги"},
        { "бежим", "делай ноги" },
        { "понял", "догнал" },
        { "пон", "догнал" },
        { "не понял", "не догоняю" },
        { "не пон", "не догоняю" },
        { "болезнь", "загибаюсь" },
        { "заболел", "загибаюсь" },
        { "смотришь", "зыришь"},
        { "смотри", "зырь" },
        { "доебался", "капаешь на мозги"},
        { "че за хуйня", "че за кипиш"},
        { "умираю", "отдаю концы" },
        { "умер", "отдал концы" },
        { "хватит", "харе" },
        { "камера", "шконка" },
        { "камеру", "шконку" },
        { "камеры", "шконки" },
        { "срочник", "опущенный" },
        { "срочники", "опущенные" },
        { "ДАМ", "аппарат" },
        { "клоун", "балабан" },
        { "клуня", "балабан" },
        { "поесть", "баланды" },
        { "пожрать", "баланды" },
        { "еды", "баланды"},
        { "допрос", "баня" },
        { "наручники", "баранки"},
        { "нож", "иголка" },
        { "ножа", "иголки" },
        { "не ссы", "не кипишуй" },
        { "не ссыте", "не кипишуйте" },
        { "еврей", "лац" },
        { "шлюха", "лохудра" },
        { "ассистент", "Лёнька"},
        { "печать", "лепиху" },
        { "бухой", "лещ" },
        { "деньги", "лаве" },
        { "повар", "ложкарь" },
        { "повара", "ложкаря" },
        { "повару", "ложкарю" },
        { "Кли", "Лялька" },
        { "к Кли", "к Ляльке"},
        { "у Кли", "у Ляльке" },
        { "патроны", "маслята"},
        { "Ерп", "месит глину"},
        { "ЕРП", "месит глину" },
        { "убийство", "мокруха" },
        { "убийства", "мокрухи" },
        { "расстрел", "отправляют на луну" },
        { "расстрелять", "отправляют на луну" },
        { "имя", "погоняло" },
        { "признался", "раскололся"},
        { "признайся", "колись" },
        { "нога", "салазка" },
        { "повезло", "фортануло" },
        { "удачи", "фарта" },
        { "ЯО", "хачи" },
        { "золото", "цветняк" },
        { "золота", "цветняка"  },
        { "ERP", "цирк" },
        { "гитара", "цымбала" },
        { "гитару", "цымбалу" },
        { "хлоральгидрат", "черёмуха" },
        { "хлоральгидрата", "черёмухи"},
        { "АВД", "Шапиро" },
        { "авд", "Шапиро"},
        { "говно", "шняга"},
        { "говна", "шняги" },
        { "сперма", "элексир бодрости" },
        { "сперме", "элексире бодрости" },
        { "спермы", "элексира бодрости"}
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrisonerAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public string Accentuate(string message, PrisonerAccentComponent component)
    {
        // Order:
        // Do text manipulations first
        // Then prefix/suffix funnyies

        var msg = message;

        foreach (var (first, replace) in DirectReplacements)
        {
            msg = Regex.Replace(msg, $@"(?<!\w){first}(?!\w)", replace, RegexOptions.IgnoreCase);
        }

        // thinking -> thinkin'
        // king -> king
        msg = Regex.Replace(msg, @"(?<=\w\w)ing(?!\w)", "in'", RegexOptions.IgnoreCase);

        // or -> uh and ar -> ah in the middle of words (fuhget, tahget)
        msg = Regex.Replace(msg, @"(?<=\w)or(?=\w)", "uh", RegexOptions.IgnoreCase);
        msg = Regex.Replace(msg, @"(?<=\w)ar(?=\w)", "ah", RegexOptions.IgnoreCase);

        // Prefix
        if (_random.Prob(0.15f))
        {
            var pick = _random.Next(1, 2);

            // Reverse sanitize capital
            msg = msg[0].ToString().ToLower() + msg.Remove(0, 1);
            msg = Loc.GetString($"accent-mobster-prefix-{pick}") + " " + msg;
        }

        // Sanitize capital again, in case we substituted a word that should be capitalized
        msg = msg[0].ToString().ToUpper() + msg.Remove(0, 1);

        // Suffixes
        if (_random.Prob(0.4f))
        {
            if (component.Jailer)
            {
                var pick = _random.Next(1, 4);
                msg += Loc.GetString($"accent-mobster-suffix-boss-{pick}");
            }
            else
            {
                var pick = _random.Next(1, 3);
                msg += Loc.GetString($"accent-mobster-suffix-minion-{pick}");
            }
        }

        return msg;
    }

    private void OnAccentGet(EntityUid uid, PrisonerAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
