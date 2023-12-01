namespace nai.commands;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;


public class EnhanceCommand : Command, IKeyboardProcessor
{
    public override string Aliases => "enhance";

    public override string QueueName => EngineQueue.ImageGeneration;

    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (Message.ReplyToMessage is null)
            return;
    }

    public async ValueTask ProcessAction(KeyboardImageGeneratorData context)
    {
        var settings = Config.GetNaiSettings();
        var engine = User.GetSelectedEngine(settings);
        var novelAI = new NovelAI(Config);

        var @params = NovelAIParams.Create(settings, engine, context.seed);

        @params.height = (int)(context.size.width * 1.5);
        @params.width = (int)(context.size.height * 1.5);
        @params.image = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(context.pngPath));
        @params.strength = 0.7f;
        @params.noise = 0.2f;

        var input = new NovelAIinput(engine, @params, context.config);

        var results = await novelAI.GenerateRequest(BotClient, CharId, Message, input);
        using var stream = results.Single();
        
        var inpf = InputFile.FromStream(stream, $"{context.seed}.enhanced.png");

        if (Message.ReplyToMessage!.From!.Id != User.Id)
        {
            await BotClient.SendDocumentAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                document: inpf,
                caption: $"Enhanced {context.seed} \n@{Message.ReplyToMessage!.From!.Username} @{User.TgLogin} paid {context.price} 💎",
                parseMode: ParseMode.MarkdownV2);
        }
        else
        {
            await BotClient.SendDocumentAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                document: inpf,
                caption: $"Enhanced {context.seed} \nPaid {context.price} 💎",
                parseMode: ParseMode.Html);
        }

        await System.IO.File.WriteAllBytesAsync($"{context.pngPath}.enhanced.png", stream.ToArray());

        await User.GrantCoinsAsync(Db, -context.price);
    }

    public KeyboardAction Action => KeyboardAction.Enhance;
}


public interface IKeyboardProcessor
{
    ValueTask ProcessAction(KeyboardImageGeneratorData context);
    string QueueName { get; }
    KeyboardAction Action { get; }
}