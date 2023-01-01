using Telegram.Bot;

public class BalanceCommand : Command
{
    public override List<string> Aliases
        => new() { "/balance" };
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: $"Your balance \n" +
                  $"💎 {User.CrystalCoin}\n" +
                  $"👑 {User.CrownCoin}",
            cancellationToken: ct);
    }
}