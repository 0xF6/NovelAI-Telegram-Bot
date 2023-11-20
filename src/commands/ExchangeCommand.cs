using nai.db;
using Telegram.Bot;

namespace nai.commands;

public class ExchangeCommand : Command
{
    public override List<string> Aliases => new()
    {
        "/exchange",
    };
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (!long.TryParse(cmdText, out var balance))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Enter amount of 💎",
                cancellationToken: ct);
            return;
        }


        if (!User.IsAllowExecute(balance, NovelUserAssets.CRYSTAL))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Insufficient balance of 💎",
                cancellationToken: ct);
            return;
        }

        var diff = balance % 20;
        var crystals = (balance - diff);
        var crowns = (balance - diff) / 20;

        await User.GrantCoinsAsync(NovelUserAssets.CRYSTAL, -crystals);
        await User.GrantCoinsAsync(NovelUserAssets.CROWN, crowns);

        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: $"You are success exchange {crystals} 💎 to {crowns} 👑!",
            cancellationToken: ct);
    }
}