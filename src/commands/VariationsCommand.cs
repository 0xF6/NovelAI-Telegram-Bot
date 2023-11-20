using Flurl.Http;
using nai.db;
using nai.nai;
using RandomGen;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace nai.commands;

public class VariationsCommand : Command, IKeyboardProcessor
{
    public override List<string> Aliases => new()
    {
        "/variations"
    };
    public override string QueueName => "image_generation";

    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmdText))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Provide promt",
                cancellationToken: ct);
            return;
        }

        if (!User.IsAllowExecute(450, NovelUserAssets.CRYSTAL))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Insufficient balance of 💎",
                cancellationToken: ct);
            return;
        }
        Console.WriteLine($"Start variations {User.TgLogin}");

        var random = Gen.Random.Numbers.Longs(0, (long)(Math.Pow(2, 32) - 1));
        var query = Enumerable.Range(0, 10)
            .Select(x => new NovelAIParams
            {
                seed = random()
            })
            .Select(x => new NovelAIinput(cmdText.Contains("nsfw") ? "nai-diffusion" : "safe-diffusion", x)
            {
                input = cmdText
            });

        var novelAI = new NovelAI();
        var disposable = new List<MemoryStream>();
        var files = new List<InputMediaDocument>();

        var swg = await BotClient.SendTextMessageAsync(CharId, "Wait...", replyToMessageId: Message.MessageId, cancellationToken: ct);
        var index = 0;
        foreach (var aIinput in query)
        {
            var result = await novelAI
                .Request()
                .AllowAnyHttpStatus()
                .PostJsonAsync(aIinput, cancellationToken: ct);

            if (result.StatusCode is not (200 or 201))
            {
                var strerr = await result.GetStringAsync();
                await BotClient.SendTextMessageAsync(
                    chatId: CharId,
                    replyToMessageId: Message.MessageId,
                    text: $"Unhandled error\n{result.StatusCode}, {strerr}, seed: {aIinput.parameters.seed}",
                    cancellationToken: ct);
                return;
            }

            var str = await result.GetStringAsync();
            var entities = str.Split('\n');
            var data = entities[2].Replace("data:", "");
            var bytes = Convert.FromBase64String(data);

            var stream = new MemoryStream(bytes);
            disposable.Add(stream);

            files.Add(new InputMediaDocument(InputFile.FromStream(stream, $"{aIinput.parameters.seed}.png")));

            Console.WriteLine($"Generated {aIinput.parameters.seed}.png");
            await Task.Delay(3000, ct);
            index++;

            await BotClient.EditMessageTextAsync(CharId, swg.MessageId, $"Wait...\n{index}/10", cancellationToken: ct);
        }
        await Task.Delay(30000, ct);
        var msg = await BotClient.SendMediaGroupAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            media: files.ToArray(), cancellationToken: ct);
        await BotClient.DeleteMessageAsync(CharId, swg.MessageId, ct);
        await BotClient.SendTextMessageAsync(chatId: CharId, text:
            $"Variation generated\npaid 450 💎",
            parseMode: ParseMode.MarkdownV2, replyToMessageId: msg.First().MessageId, cancellationToken: ct);

        await User.GrantCoinsAsync(NovelUserAssets.CRYSTAL, -450);

        foreach (var stream in disposable) await stream.DisposeAsync();

        await Task.Delay(30000, ct);
    }

    public async ValueTask ProcessAction(KeyboardImageGeneratorData context)
    {
        var uints = Gen.Random.Numbers.Longs(0, (long)(Math.Pow(2, 32) - 1));
        var seed = uints();

        var novelAI = new NovelAI();
        var pams = new NovelAIParams
        {
            seed = seed, //context.seed,
            height = (int)(context.size.width),
            width = (int)(context.size.height),
            image = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(context.pngPath)),
            strength = 0.8f,
            noise = 0.1f,
            n_samples = 3,
            stems = 50
        };

        var promt = new NovelAIinput("safe-diffusion", pams) { input = context.config };
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
        var entities = str.Split('\n')
            .Where(x => x.StartsWith("data:"))
            .Select(x => x.Replace("data:", ""))
            .Select(Convert.FromBase64String);
        var toDispose = new List<IDisposable>();
        var files = new List<InputMediaDocument>();

        foreach (var (x, i) in entities.Select((x, i) => (x, i)))
        {
            var stream = new MemoryStream(x);
            toDispose.Add(stream);
            await System.IO.File.WriteAllBytesAsync($"{context.pngPath}.variant.{i}.png", x);
            files.Add(new InputMediaDocument(InputFile.FromStream(stream, $"{context.seed}.variant.{i}.png")));
        }

        if (Message.ReplyToMessage!.From!.Id != User.Id)
        {
            var msg = await BotClient.SendMediaGroupAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                media: files.ToArray());

            await BotClient.SendTextMessageAsync(chatId: CharId, text:
                $"Variations {context.seed} \n@{Message.ReplyToMessage!.From!.Username}!\n@{User.TgLogin} paid {context.price.crystals} 💎, {context.price.crowns} 👑",
                parseMode: ParseMode.MarkdownV2, replyToMessageId: msg.First().MessageId);
        }
        else
        {

            var msg = await BotClient.SendMediaGroupAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                media: files.ToArray());

            await BotClient.SendTextMessageAsync(chatId: CharId, text:
                $"Variations {context.seed} \nPaid {context.price.crystals} 💎, {context.price.crowns} 👑",
                parseMode: ParseMode.MarkdownV2, replyToMessageId: msg.First().MessageId);
        }

        await User.GrantCoinsAsync(NovelUserAssets.CRYSTAL, -context.price.crystals);
        await User.GrantCoinsAsync(NovelUserAssets.CROWN, -context.price.crowns);
    }

    public KeyboardAction Action => KeyboardAction.Variations;
}