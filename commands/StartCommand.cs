using NAIBot.locales;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NAIBot.commands;

public class StartCommand : Command
{
    public override List<string> Aliases => new()
    {
        "/start"
    };

    public override bool OnlyPrivateChat => true;
    public override bool IsTrusted => false;



    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
        => await BotClient.SendTextMessageAsync(Message.Chat.Id, Locale.Get(Locales.MessageStart), ParseMode.Html, cancellationToken: ct);
}