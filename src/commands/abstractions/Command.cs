using Microsoft.Extensions.Logging;
using nai;
using nai.db;
using nai.i18n;
using Telegram.Bot;
using Telegram.Bot.Types;


public abstract class Command
{
    protected Message Message => Context.Message;
    protected NovelUser User => Context.User;
    protected ITelegramBotClient BotClient => Context.BotClient;
    protected Localization Locale => Context.Localization;
    protected long CharId => Message.Chat.Id;
    public virtual bool IsTrusted => true;
    public virtual bool OnlyPrivateChat => false;
    public Db Db => Context.Db;
    public Config Config => Context.Config;
    public ILogger Logger => Context.Logger;


    private CommandContext? _context;

    protected internal CommandContext Context
    {
        get => _context ?? throw new InvalidOperationException("Incorrect setup of command");
        set
        {
            if (_context is null)
                _context = value;
            else
                throw new InvalidOperationException("Cannot set context, it already set");
        }
    }

    public virtual string QueueName => EngineQueue.WithoutDelay;
    public abstract ValueTask ExecuteAsync(string cmdText, CancellationToken ct);
    public abstract string Aliases { get; }
}


public class CommandContext(Message message, NovelUser user, Localization localization, ITelegramBotClient botClient, Db db, Config config, ILogger logger)
{
    public Message Message { get; } = message;
    public NovelUser User { get; } = user;
    public Localization Localization { get; } = localization;
    public ITelegramBotClient BotClient { get; } = botClient;
    public Db Db { get; } = db;
    public Config Config { get; } = config;
    public ILogger Logger { get; } = logger;
}