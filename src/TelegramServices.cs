namespace nai;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using nai.commands;
using nai.db;
using nai.i18n;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types;


public static class TelegramServices
{
    public static IServiceCollection UseTelegram(this IServiceCollection collection, Action<ITelegramCommandStorage> setup)
    {
        var storage = new TelegramCommandStorage();
        setup(storage);

        collection.AddSingleton(x => new TelegramBotClient(x.GetRequiredService<Config>().TelegramBotToken));
        collection.AddHostedService<TelegramWorker>();
        collection.AddSingleton(storage);
        return collection;
    }
}

public class TelegramWorker(TelegramBotClient client, IServiceScopeFactory scopeFactory, ILogger<TelegramWorker> logger) : BackgroundService, IUpdateHandler
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };
        client.StartReceiving(
            updateHandler: this,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(250, stoppingToken);
        }

        await client.LogOutAsync(stoppingToken).ConfigureAwait(false);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            await Handle(botClient, update, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Chat: {chatId}, user: {username}", update.Message.Chat.Id, update.Message.From.Username);
        }
    }

    public async Task Handle(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<Db>();
        var queue = scope.ServiceProvider.GetRequiredService<EngineQueue>();
        var commands = scope.ServiceProvider.GetRequiredService<TelegramCommandStorage>();
        var localization = scope.ServiceProvider.GetRequiredService<Localization>();
        var config = scope.ServiceProvider.GetRequiredService<Config>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        await db.LoadState();

        if (update.Message?.SuccessfulPayment is not null)
        {
            await bot.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                replyToMessageId: update.Message.MessageId,
                text: "Success payment, crowns and crystals add to your account.",
                cancellationToken: ct);
            return;
        }
        if (update.PreCheckoutQuery is not null)
        {
            await HandlePayment(bot, update.PreCheckoutQuery);
            return;
        }

        if (update.Message is { Type: MessageType.ChatMembersAdded } and { From: { Username: not null } })
        {
            var usw = await db.GetUser(update.Message.From);
            if (usw.CrystalCoin == 0)
                await usw.GrantCoinsAsync(db, 2000);
        }

        if (update.Message is null && update.CallbackQuery is not null)
        {
            await ProcessQuery(bot, update.CallbackQuery, scope);
            return;
        }
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;
        if (update.Message?.From?.Username is not { })
        {
            await bot.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                replyToMessageId: update.Message.MessageId,
                text: "No username on your account, please define username in settings and try again",
                cancellationToken: ct);
            return;
        }

        if (!messageText.StartsWith("/"))
            return;

        var chatId = message.Chat.Id;


        var entity = message.Entities?.FirstOrDefault();
        var entityVal = message.EntityValues?.FirstOrDefault();

        // skip not bot command
        if (entity?.Type is not MessageEntityType.BotCommand)
            return;

        if (entityVal is null)
            return;

        var regex = new Regex(@"\/?(\w+)(\@\w+)?");


        var match = regex.Match(entityVal);

        if (match is { Success: false } or { Groups.Count: < 2 })
        {
            logger.LogError("Bad command format '{entityVal}'", entityVal);
            return;
        }


        var commandName = match.Groups[1].Value;
        var user = await db.GetUser(message.From!);
        var command = commands.Summon(commandName, scope);


        if (command is null)
        {
            logger.LogWarning("No found command '{commandName}'", commandName);
            return;
        }

        var ctx = new CommandContext(message, user, localization, bot, db, config, loggerFactory.CreateLogger(command.GetType()));
        command.Context = ctx;

        if (command is not { })
            return;

        if (!config.CommandIsActive(command))
        {
            logger.LogWarning("Command {commandName} is not active", command.GetType().Name);
            return;
        }

        if (command.OnlyPrivateChat && message.Chat.Type != ChatType.Private)
            return;
        if (command.IsTrusted && !db.ChatIsAllowed(chatId))
        {
            await bot.SendTextMessageAsync(
                chatId: chatId,
                replyToMessageId: message.MessageId,
                text: "Insufficient Permissions [0x8293]\nTry authorize chat with '/authorize_chat' command.",
                cancellationToken: ct);
            return;
        }

        if (command.QueueName.Equals(EngineQueue.WithoutDelay))
            await command.ExecuteAsync(messageText.Replace(entityVal!, ""), ct);
        else
        {
            logger.LogDebug("Success stage to queue generation task, from @{username}", message.From!.Username);
            queue.Add(async ()
                => await command.ExecuteAsync(messageText.Replace(entityVal!, ""), ct).ConfigureAwait(false), $"ImageGen {message.From.Username}");
        }
    }

    public async Task HandlePayment(ITelegramBotClient bot, PreCheckoutQuery query)
    {
        try
        {
            await bot.AnswerPreCheckoutQueryAsync(query.Id);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed handle payment");
        }
    }



    public async Task ProcessQuery(ITelegramBotClient bot, CallbackQuery upd, IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<Db>();
        var queue = scope.ServiceProvider.GetRequiredService<EngineQueue>();
        var commands = scope.ServiceProvider.GetRequiredService<TelegramCommandStorage>();
        var localization = scope.ServiceProvider.GetRequiredService<Localization>();
        var config = scope.ServiceProvider.GetRequiredService<Config>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        await db.LoadState();

        var msg = upd.Message;
        var from = upd.From;
        var key = upd.Data;

        if (!Guid.TryParse(key, out var id))
            return;

        var user = await db.GetUser(from);

        var at = new ActionTable();

        var table = at.Load(id);

        if (table is null)
            return;

        if (!user.IsAllowExecute(table.price))
        {
            await bot.AnswerCallbackQueryAsync(upd.Id, localization.Get(Locales.InsufficientBalance));
            return;
        }

        var command = commands.KeyboardProcessor(table.action, scope);

        if (command is null)
            return;

        var ctx = new CommandContext(msg!, user, localization, bot, db, config, loggerFactory.CreateLogger(command.GetType()));

        if (command is Command cmd) cmd.Context = ctx;

        logger.LogInformation("Success stage to queue {action} task, from @{username}", command.Action, from.Username);

        if (command.QueueName == EngineQueue.WithoutDelay)
            await command.ProcessAction(table);
        else
            queue.Add(async ()
                    => await command.ProcessAction(table).ConfigureAwait(false), $"Image {command.Action} {from.Username}");

    }


    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogCritical(exception, ErrorMessage);
        return Task.CompletedTask;
    }
}




public class TelegramCommandStorage : ITelegramCommandStorage
{
    public readonly List<Type> commandTypes = new();
    public ITelegramCommandStorage Use<T>() where T : Command
    {
        commandTypes.Add(typeof(T));
        return this;
    }


    public Command? Summon(string key, IServiceScope scope)
    {
        key = key.ToLower().Trim('/');
        var map = Populate<Command, string>(scope, x => x.Aliases);

        if (map.TryGetValue(key, out var val))
            return val;

        return null;
    }

    public IKeyboardProcessor? KeyboardProcessor(KeyboardAction action, IServiceScope scope)
    {
        var map = Populate<IKeyboardProcessor, KeyboardAction>(scope, x => x.Action);

        if (map.TryGetValue(action, out var val))
            return val;

        return null;
    }


    // not good solution
    private Dictionary<TKey, T> Populate<T, TKey>(IServiceScope scope, Func<T, TKey> keySelector)
    {
        var map = new Dictionary<TKey, T>();
        foreach (var type in commandTypes)
        {
            var cmd = ActivatorUtilities.CreateInstance(scope.ServiceProvider, type, Array.Empty<object>());

            if (cmd is T t)
                map.Add(keySelector(t), t);
        }

        return map;
    }
}

public interface ITelegramCommandStorage
{
    ITelegramCommandStorage Use<T>() where T : Command;
}