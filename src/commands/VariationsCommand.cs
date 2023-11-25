using Flurl.Http;
using nai.db;
using nai.nai;
using RandomGen;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;
using nai.i18n;

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

        var settings = Config.GetNaiSettings();
        var engine = User.GetSelectedEngine(settings);
        var seedFormula = new SeedFormula(settings.SeedFormula);

        var paramsList = new List<NovelAIinput>();
        var totalPrice = 0L;

        foreach (var _ in Enumerable.Range(0, Math.Min(8, settings.VariationSize)))
        {
            var seed = seedFormula.GetSeed();
            var @params = NovelAIParams.Create(settings, engine, seed);
            totalPrice += Db.CalculatePrice(engine, @params);
            paramsList.Add(new NovelAIinput(engine, @params, cmdText));
        }

        if (!User.IsAllowExecute(totalPrice, NovelUserAssets.CRYSTAL))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: Locale.Get(Locales.InsufficientBalance),
                cancellationToken: ct);
            return;
        }
        Console.WriteLine($"Start variations {User.TgLogin}");
        

        var novelAI = new NovelAI();
        var disposable = new List<MemoryStream>();
        var files = new List<InputMediaDocument>();

        var swg = await BotClient.SendTextMessageAsync(CharId, "Wait...", replyToMessageId: Message.MessageId, cancellationToken: ct);
        var index = 0;
        foreach (var aIinput in paramsList)
        {
            var result = await novelAI.GenerateRequest(BotClient, CharId, Message, aIinput, ct);
            
            disposable.AddRange(result);


            foreach (var (stream, i) in result.Select((x, i) => (x, i)))
            {
                files.Add(new InputMediaDocument(InputFile.FromStream(stream, $"{aIinput.parameters.seed}-{i}.png")));
                Console.WriteLine($"Generated {aIinput.parameters.seed}-{i}.png");

            }
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
            $"Variation generated\npaid {totalPrice} 💎",
            parseMode: ParseMode.MarkdownV2, replyToMessageId: msg.First().MessageId, cancellationToken: ct);

        await User.GrantCoinsAsync(NovelUserAssets.CRYSTAL, -totalPrice);

        foreach (var stream in disposable) await stream.DisposeAsync();

        await Task.Delay(30000, ct);
    }

    public async ValueTask ProcessAction(KeyboardImageGeneratorData context)
    {
        var settings = Config.GetNaiSettings();
        var engine = User.GetSelectedEngine(settings);
        var seedFormula = new SeedFormula(settings.SeedFormula);


        var novelAI = new NovelAI();

        var seed = seedFormula.GetSeed();
        var @params = NovelAIParams.Create(settings, engine, seed);

        @params.height = (int)(context.size.width);
        @params.width = (int)(context.size.height);
        @params.image = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(context.pngPath));
        @params.strength = 0.8f;
        @params.noise = 0.1f;
        @params.n_samples = 3;
        @params.steps = 50; // maybe need use step count from settings?
        

        var input = new NovelAIinput(engine, @params, context.config);
        var toDispose = new List<IDisposable>();
        var files = new List<InputMediaDocument>();

        try
        {
            var entities = await novelAI.GenerateRequest(BotClient, CharId, Message, input);

            foreach (var (stream, i) in entities.Select((x, i) => (x, i)))
            {
                toDispose.Add(stream);
                await File.WriteAllBytesAsync($"{context.pngPath}.variant.{i}.png", stream.ToArray());
                files.Add(new InputMediaDocument(InputFile.FromStream(stream, $"{context.seed}.variant.{i}.png")));
            }

            if (Message.ReplyToMessage!.From!.Id != User.Id)
            {
                var msg = await BotClient.SendMediaGroupAsync(
                    chatId: CharId,
                    replyToMessageId: Message.MessageId,
                    media: files.ToArray());

                await BotClient.SendTextMessageAsync(chatId: CharId, text:
                    $"Variations {context.seed} \n@{Message.ReplyToMessage!.From!.Username}!\n@{User.TgLogin} paid {context.price} 💎",
                    parseMode: ParseMode.MarkdownV2, replyToMessageId: msg.First().MessageId);
            }
            else
            {

                var msg = await BotClient.SendMediaGroupAsync(
                    chatId: CharId,
                    replyToMessageId: Message.MessageId,
                    media: files.ToArray());

                await BotClient.SendTextMessageAsync(chatId: CharId, text:
                    $"Variations {context.seed} \nPaid {context.price} 💎",
                    parseMode: ParseMode.MarkdownV2, replyToMessageId: msg.First().MessageId);
            }

            await User.GrantCoinsAsync(NovelUserAssets.CRYSTAL, -context.price);
        }
        finally
        {
            toDispose.ForEach(x => x.Dispose());
        }
    }

    public KeyboardAction Action => KeyboardAction.Variations;
}