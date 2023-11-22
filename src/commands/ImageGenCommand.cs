﻿using Flurl.Http;
using Ionic.Zip;
using nai;
using nai.db;
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
    protected virtual int GetCrownPrice() => 0;
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

        var uints = Gen.Random.Numbers.Longs(0, (long)(Math.Pow(2, 32) - 1));
        var seed = uints();

        var pams = new NovelAIParams
        {
            seed = seed
        };
        if (!IsSfw()) pams.uc += ", nsfw, nude, naked";

        pams.width = GetSize().x;
        pams.height = GetSize().y;

        var price = Db.CalculatePrice(NovelAIEngine.ByKey(GetEngineName()), pams) + GetAdditionalPrice();

        if (!User.IsAllowExecute(price))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Insufficient balance of 💎",
                cancellationToken: ct);
            return;
        }
        
        if (GetCrownPrice() != 0 && !User.IsAllowExecuteByCrown(GetCrownPrice()))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Insufficient balance of 👑",
                cancellationToken: ct);
            return;
        }

        //if (!await OnValidate())
        //    return;

        var novelAI = new NovelAI();

        var promt = new NovelAIinput(GetEngineName(), pams)
        {
            input = cmdText
        };

        await OnFillAdditionalData(promt);

        var stream = await novelAI.GenerateRequest(BotClient, CharId, Message, promt, ct);


        if (stream is null)
            return;


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
                CreateAction($"Enhance [14👑 + {price + (price * 0.3)}💎]", KeyboardAction.Enhance, seed, cmdText, pngPath, (price + (int)(price * 0.3), 14))
            },
            new []
            {
                CreateAction($"Variations [18👑 + {price * 3}💎]", KeyboardAction.Variations, seed, cmdText, pngPath, (price * 3, 18))
            }
        });

        if (GetCrownPrice() == 0)
        {
            var message = await BotClient.SendDocumentAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                document: inpf,
                caption: $"{seed}, paid {price} 💎",
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);

            await File.WriteAllTextAsync($"images/{User.Id}/{flsName}.png.msg", message.MessageId.ToString(), ct);
        }
        else
        {
            var message = await BotClient.SendDocumentAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                document: inpf,
                caption: $"{seed}, paid {price} 💎, {GetCrownPrice()} 👑",
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);

            await File.WriteAllTextAsync($"images/{User.Id}/{flsName}.png.msg", message.MessageId.ToString(), ct);
        }

        

        await User.GrantCoinsAsync(NovelUserAssets.CRYSTAL,-price);
        if (GetCrownPrice() != 0)
            await User.GrantCoinsAsync(NovelUserAssets.CROWN,-GetCrownPrice());
    }

    private InlineKeyboardButton CreateAction(string title, KeyboardAction action, long seed, string config, string pngPath, (long crystals, long crowns) price) =>
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
    KeyboardAction action, (long crystals, long crowns) price, (long height, long width) size)
{
    public Guid id { get; set; }

    public async Task TrimAsync() 
        => new ActionTable().Remote(id);
}