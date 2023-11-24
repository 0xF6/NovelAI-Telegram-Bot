using Flurl.Http;
using Ionic.Zip;
using nai;
using nai.db;
using nai.i18n;
using nai.nai;
using RandomGen;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
// ReSharper disable ComplexConditionExpression

public abstract class ImageGenCommand : Command
{
    public override string QueueName => "image_generation";

    protected abstract bool IsSfw();
    protected abstract (int x, int y) GetSize();


    protected virtual int GetAdditionalPrice() => 0;
    protected string GetEngineName() => Config.NovelAiEngine; 


    protected virtual ValueTask OnFillAdditionalData(NovelAIinput input) 
        => ValueTask.CompletedTask;

    protected virtual ValueTask<bool> OnValidate() => ValueTask.FromResult(true);

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
        var seedFormula = new SeedFormula(settings.SeedFormula);
        var seed = seedFormula.GetSeed();

        var pams = NovelAIParams.Create(settings, seed);
        if (!IsSfw()) pams.negative_prompt += $", {settings.SfwNegateTags}";

        pams.width = GetSize().x;
        pams.height = GetSize().y;

        var price = Db.CalculatePrice(NovelAIEngine.ByKey(GetEngineName()), pams) + GetAdditionalPrice();

        if (!User.IsAllowExecute(price))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: Locale.Get(Locales.InsufficientBalance),
                cancellationToken: ct);
            return;
        }

        if (!await OnValidate())
            return;

        var novelAI = new NovelAI();

        var promt = new NovelAIinput(GetEngineName(), pams)
        {
            input = cmdText
        };

        await OnFillAdditionalData(promt);

        var result = await novelAI.GenerateRequest(BotClient, CharId, Message, promt, ct);
        using var stream = result.Single();
        

        if (!Directory.Exists($"images/{User.Id}/"))
            Directory.CreateDirectory($"images/{User.Id}/");
        var flsName = Guid.NewGuid().ToString("N");
        var pngPath = $"images/{User.Id}/{flsName}.png";
        await File.WriteAllBytesAsync(pngPath, stream.ToArray(), ct);
        await File.WriteAllTextAsync($"images/{User.Id}/{flsName}.png.seed", seed.ToString(), ct);
        await File.WriteAllTextAsync($"images/{User.Id}/{flsName}.png.config", cmdText, ct);
        
        var inpf = InputFile.FromStream(stream, $"{seed}.png");


        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            new []
            {
                CreateAction($"Enhance [{price + (price * 0.3)}💎]", KeyboardAction.Enhance, seed, cmdText, pngPath, (price + (int)(price * 0.3)))
            },
            new []
            {
                CreateAction($"Variations [{price * 3}💎]", KeyboardAction.Variations, seed, cmdText, pngPath, (price * 3))
            }
        });

        var message = await BotClient.SendDocumentAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            document: inpf,
            caption: $"{GetEngineName()} {seed}, paid {price} 💎",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard,
            cancellationToken: ct);

        await File.WriteAllTextAsync($"images/{User.Id}/{flsName}.png.msg", message.MessageId.ToString(), ct);

        await User.GrantCoinsAsync(NovelUserAssets.CRYSTAL,-price);
    }

    private InlineKeyboardButton CreateAction(string title, KeyboardAction action, long seed, string config, string pngPath, long price) =>
        InlineKeyboardButton.WithCallbackData(text: title,
            callbackData: new ActionTable().Create(new KeyboardImageGeneratorData(pngPath, seed, config, action, price, GetSize())).ToString("N"));
}

public enum KeyboardAction
{
    Enhance,
    Variations,
    AuthCodeSegment
}

public record KeyboardImageGeneratorData(
    string pngPath, 
    long seed, 
    string config, 
    KeyboardAction action, long price, (long height, long width) size)
{
    public Guid id { get; set; }

    public async Task TrimAsync() 
        => new ActionTable().Remote(id);
}