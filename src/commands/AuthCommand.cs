using nai.db;
using nai.i18n;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace nai.commands;

public class AuthCommand : Command, IKeyboardProcessor
{
    public override bool IsTrusted => false;

    public override List<string> Aliases => new()
    {
        "/authorize_chat"
    };

    public override string QueueName => "none";

    public static List<string> Keys = new List<string>()
    {
        "⭐️",
        "✨",
        "☄️",
        "🔥",
        "\U0001fa90",
        "🌊"
    };
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (Message.From is null)
            return;

        Console.WriteLine($"CHAT AUTH DATA: ");
        var keys = Keys.ToArray().ToList();
        Shuffle(keys);
        var allowedKey = string.Join(" ", keys);
        var allowedKeyArr = allowedKey.Split(' ').ToArray();
        
        Console.WriteLine(allowedKey);

        if (Config.MainAdministrator == 0)
        {
            Console.WriteLine($"No main administrator has been defined");
            return;
        }


        await BotClient.SendTextMessageAsync(Config.MainAdministrator, $"Запрос авторизации от \\@{Message.From!.Username} \\([User {Message.From!.FirstName} {Message.From!.LastName}](tg://user?id={Message.From!.Id})\\)\nCode: {allowedKey}", parseMode:ParseMode.MarkdownV2, cancellationToken: ct);

        Shuffle(keys);

        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            new []
            {
                CreateAction(keys[0], KeyboardAction.AuthCodeSegment, FindIndex(allowedKeyArr, keys[0]), keys[0], $"{allowedKey}", (0, 0)),
                CreateAction(keys[1], KeyboardAction.AuthCodeSegment, FindIndex(allowedKeyArr, keys[1]), keys[1], $"{allowedKey}", (0, 0)),
            },
            new []
            {
                CreateAction(keys[2], KeyboardAction.AuthCodeSegment, FindIndex(allowedKeyArr, keys[2]), keys[2], $"{allowedKey}", (0, 0)),
                CreateAction(keys[3], KeyboardAction.AuthCodeSegment, FindIndex(allowedKeyArr, keys[3]), keys[3], $"{allowedKey}", (0, 0)),
            },
            new []
            {
                CreateAction(keys[4], KeyboardAction.AuthCodeSegment, FindIndex(allowedKeyArr, keys[4]), keys[4], $"{allowedKey}", (0, 0)),
                CreateAction(keys[5], KeyboardAction.AuthCodeSegment, FindIndex(allowedKeyArr, keys[5]), keys[5], $"{allowedKey}", (0, 0)),
            }
        });


        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: Locale.Get(Locales.SendedAuthRequest),
            replyMarkup: inlineKeyboard,
            cancellationToken: ct);
    }

    private static long FindIndex<T>(IEnumerable<T> arr, T el) where T : IComparable
    {
        foreach (var (x,y) in arr.Select((x, y) => (x, y)))
        {
            if (x.Equals(el))
                return y;
        }

        return -1;
    }

    private static Random rng = new Random();
    public static void Shuffle<T>(IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    private InlineKeyboardButton CreateAction(string title, KeyboardAction action, long seed, string config, string pngPath, (long crystals, long crowns) price) =>
        InlineKeyboardButton.WithCallbackData(text: title,
            callbackData: new ActionTable().Create(new KeyboardImageGeneratorData(pngPath, seed, config, action, price, (Message.Chat.Id, User.Id))).ToString("N"));

    public static Dictionary<long, List<string>> Answers = new ();

    public async ValueTask ProcessAction(KeyboardImageGeneratorData context)
    {
        var allowedKey = context.pngPath.Split(' ');
        var currentIndex = (int)context.seed;
        var config = context.config;
        var chatId = context.size.height;

        if (!Answers.ContainsKey(chatId))
            Answers.Add(chatId, new ());

        if (allowedKey[currentIndex] == config) 
            Answers[chatId].Add(config);
        else
        {
            await BotClient.EditMessageTextAsync(chatId, Message.MessageId, Locale.Get(Locales.AuthFailed));
            await context.TrimAsync();
            return;
        }

        if (Answers[chatId].Count == allowedKey.Length)
        {
            if (string.Join("", allowedKey) == string.Join("", Answers[chatId]))
            {
                await BotClient.EditMessageTextAsync(chatId, Message.MessageId, Locale.Get(Locales.AuthSuccess));
                await Db.AddAllowedChat(chatId);
            }
            else
            {
                await BotClient.EditMessageTextAsync(chatId, Message.MessageId, Locale.Get(Locales.AuthFailed));
                await context.TrimAsync();
                return;
            }
        }
    }

    public KeyboardAction Action => KeyboardAction.AuthCodeSegment;
}