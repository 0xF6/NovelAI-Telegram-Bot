using Flurl.Http;
using nai.db;
using nai.nai;
using RandomGen;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace nai.commands;

public class EnhanceCommand : Command, IKeyboardProcessor
{
    public override List<string> Aliases => new()
    {
        "/enhance",
    };

    public override string QueueName => "image_generation";

    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (Message.ReplyToMessage is null)
            return;
    }

    public async ValueTask ProcessAction(KeyboardImageGeneratorData context)
    {
        var uints = Gen.Random.Numbers.Longs(0, (long)(Math.Pow(2, 32) - 1));
        var seed = uints();

        var novelAI = new NovelAI();
        var pams = new NovelAIParams
        {
            seed = seed, //context.seed,
            height = (int)(context.size.width * 1.5),
            width = (int)(context.size.height * 1.5),
            image = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(context.pngPath)),
            strength = 0.7f,
            noise = 0.2f
        };

        var promt = new NovelAIinput("safe-diffusion", pams)
        {
            input = context.config
        };
        var result = await novelAI
            .Request()
            .AllowAnyHttpStatus()
            .PostJsonAsync(promt);

        if (result.StatusCode is not (200 or 201))
        {
            var strerr = await result.GetStringAsync();
            Console.WriteLine(strerr);
            return;
        }

        var str = await result.GetStringAsync();
        var entities = str.Split('\n');
        var data = entities[2].Replace("data:", "");
        var bytes = Convert.FromBase64String(data);

        using var stream = new MemoryStream(bytes);

        
        var inpf = InputFile.FromStream(stream, $"{context.seed}.enhanced.png");

        if (Message.ReplyToMessage!.From!.Id != User.Id)
        {
            await BotClient.SendDocumentAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                document: inpf,
                caption: $"Enhanced {context.seed} \n@{Message.ReplyToMessage!.From!.Username} @{User.TgLogin} paid {context.price.crystals} 💎, {context.price.crowns} 👑",
                parseMode: ParseMode.MarkdownV2);
        }
        else
        {
            await BotClient.SendDocumentAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                document: inpf,
                caption: $"Enhanced {context.seed} \nPaid {context.price.crystals} 💎, {context.price.crowns} 👑",
                parseMode: ParseMode.Html);
        }

        await System.IO.File.WriteAllBytesAsync($"{context.pngPath}.enhanced.png", bytes);

        await User.GrantCoinsAsync(NovelUserAssets.CRYSTAL, -context.price.crystals);
        await User.GrantCoinsAsync(NovelUserAssets.CROWN, -context.price.crowns);
    }

    public KeyboardAction Action => KeyboardAction.Enhance;
}


public interface IKeyboardProcessor
{
    ValueTask ProcessAction(KeyboardImageGeneratorData context);
    string QueueName { get; }
    KeyboardAction Action { get; }
}