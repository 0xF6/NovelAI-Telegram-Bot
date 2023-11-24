using nai.i18n;
using Telegram.Bot;

public class BalanceCommand : Command
{
    public override List<string> Aliases
        => new() { "/balance", "/balance@novelai_sanrioslut_bot" };
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: string.Format(Locale.Get(Locales.YourBalance), User.CrystalCoin),
            cancellationToken: ct);
    }
}