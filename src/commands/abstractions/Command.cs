using nai;
using nai.db;
using nai.i18n;
using Telegram.Bot;
using Telegram.Bot.Types;

public abstract class Command : ISetterContext
{
    protected Message Message { get; private set; }
    protected NovelUser User { get; private set; }
    protected ITelegramBotClient BotClient { get; private set; }
    protected Localization Locale { get; private set; }
    protected long CharId => Message.Chat.Id;
    public virtual bool IsTrusted => true;
    public virtual bool OnlyPrivateChat => false;
    void ISetterContext.Set(Message msg, NovelUser user, ITelegramBotClient botClient)
    {
        this.Message = msg;
        this.User = user;
        this.BotClient = botClient;
        this.Locale = new Localization(Config.DefaultLocale);
        this.Locale.Load();
    }

    public virtual string QueueName => "none";
    public abstract List<string> Aliases { get; }
    public abstract ValueTask ExecuteAsync(string cmdText, CancellationToken ct);
}