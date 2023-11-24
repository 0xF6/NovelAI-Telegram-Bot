using nai;
using nai.commands;
using nai.commands.images;
using nai.db;
using System.Runtime.InteropServices;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;



Config.Init();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    Console.OutputEncoding = Encoding.Unicode;

var token = Config.TelegramBotToken;

using CancellationTokenSource cts = new();

var ct = cts.Token;

await Db.LoadState();

var botClient = new TelegramBotClient(token);
var q = new EngineQueue();

Task.Factory.StartNew(() => q.CycleAsync(ct));

var me = await botClient.GetMeAsync();
Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

// notifier mode
if (Environment.GetEnvironmentVariable("TELEGRAM_BOT_MODE_OF_CHANNEL") is not null)
{
    while (true)
    {
        Console.WriteLine("Enter char id:");
        var charId = Console.ReadLine();
        Console.WriteLine("Enter text");
        var text = Console.ReadLine();

        await botClient.SendTextMessageAsync(long.Parse(charId), text, parseMode: ParseMode.Html);
    }
    return;
}

// command list
var commands = new List<Command>()
{
    new PortraitNsfwImageGenCommand(),
    new PortraitSfwImageGenCommand(),
    new LandNsfwImageGenCommand(),
    new LandSfwImageGenCommand(),
    new BalanceCommand(), 
    new GrantBalanceCommand(),
    new EnhanceCommand(),
    new VariationsCommand(),
    new Img2ImgP(),
    new PayCommand(),
    new AuthCommand(),
    new InvoiceCommand(),
    new StartCommand(),
    new WallPaperGenCommand()
};

// start receiving events
botClient.StartReceiving(
    updateHandler: SafeHandle,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

// lock main thread
while (true)
    await Task.Delay(200);


// safe handle, when exception continue work
async Task SafeHandle(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
{
    try
    {
        await HandleUpdateAsync(bot, update, cancellationToken);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine($"Chat: @{update.Message?.Text} from @{update.Message?.Chat?.Username} ({update.Message?.Chat?.Id})");
    }
}

async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
{
    if (update.Message?.SuccessfulPayment is not null)
    {
        var udw = await Db.GetUser(update.Message!.From!);
        await bot.SendTextMessageAsync(
            chatId: update.Message.Chat.Id,
            replyToMessageId: update.Message.MessageId,
            text: "Success payment, crowns and crystals add to your account.",
            cancellationToken: cancellationToken);
        return;
    }
    if (update.PreCheckoutQuery is not null)
    {
        await HandlePayment(botClient, update.PreCheckoutQuery);
        return;
    }

    if (update.Message is not null and { Type: MessageType.ChatMembersAdded } and { From: { Username: not null } })
    {
        var usw = await Db.GetUser(update.Message.From);
        if (usw.CrystalCoin == 0)
            await usw.GrantCoinsAsync(NovelUserAssets.CRYSTAL, 2000);
    }

    if (update.Message is null && update.CallbackQuery is not null)
    {
        await ProcessQuery(bot, update.CallbackQuery);
        return;
    }
    if (update.Message is not { } message)
        return;
    if (message.Text is not { } messageText)
        return;
    if (update.Message?.From?.Username is not { })
        return;

    if (!messageText.StartsWith("/"))
        return;

    var chatId = message.Chat.Id;

    
    var entity = message.Entities?.FirstOrDefault();
    var entityVal = message.EntityValues?.FirstOrDefault()?.Split('@').FirstOrDefault();

    // skip not bot command
    if (entity?.Type is not MessageEntityType.BotCommand)
        return;

    var user = await Db.GetUser(message.From!);
    var command = commands.FirstOrDefault(x => x.Aliases.Contains(entityVal!))?.Create();

    if (command is ISetterContext setter) 
        setter.Set(message, user, botClient);

    if (command is not { })
        return;

    if (!Config.CommandIsActive(command))
    {
        Console.WriteLine($"Command {command.GetType().Name} is not active");
        return;
    }

    if (command.OnlyPrivateChat && message.Chat.Type != ChatType.Private)
        return;
    if (command.IsTrusted && !Db.ChatIsAllowed(chatId))
    {
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            replyToMessageId: message.MessageId,
            text: "Insufficient Permissions [0x8293]\nTry authorize chat with '/authorize_chat' command.",
            cancellationToken: cancellationToken);
        return;
    }

    if (command.QueueName == "none")
        await command.ExecuteAsync(messageText.Replace(entityVal!, ""), ct);
    else
    {
        Console.WriteLine($"Success stage to queue generation task, from @{message.From!.Username}");
        q.Add(async ()
            => await command.ExecuteAsync(messageText.Replace(entityVal!, ""), ct), $"ImageGen {message.From.Username}");
    }
}


async Task HandlePayment(ITelegramBotClient bot, PreCheckoutQuery query)
{
    try
    {
        await bot.AnswerPreCheckoutQueryAsync(query.Id);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}
async Task ProcessQuery(ITelegramBotClient bot, CallbackQuery upd)
{
    var msg = upd.Message;
    var from = upd.From;
    var key = upd.Data;

    if (!Guid.TryParse(key, out var id))
        return;

    var user = await Db.GetUser(from);

    var at = new ActionTable();

    var table = at.Load(id);

    if (table is null)
        return;

    if (!user.IsAllowExecute(table.price))
    {
        await bot.AnswerCallbackQueryAsync(upd.Id, $"У тебя не достаточно 💎");
        return;
    }


    var command = commands.OfType<IKeyboardProcessor>().FirstOrDefault(x => x.Action == table.action);

    if (command is null)
        return;

    if (command is ISetterContext setter)
        setter.Set(upd.Message!, user, botClient);

    Console.WriteLine($"Success stage to queue {command.Action} task, from @{from.Username}");
    if (command.QueueName == "none")
        await command.ProcessAction(table);
    else
        q.Add(async ()
            => await command.ProcessAction(table), $"Image {command.Action} {from.Username}");

}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}