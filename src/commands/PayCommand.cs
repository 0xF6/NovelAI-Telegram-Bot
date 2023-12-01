namespace nai.commands;

using db;
using i18n;
using Telegram.Bot;

public class PayCommand : Command
{
    public override string Aliases => "pay";

    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (!uint.TryParse(cmdText, out var balance))
            return;


        var tgUser = Message.ReplyToMessage!.From;
        var toUser = await Db.GetUser(tgUser);

        var loginFrom = Message.From!.Username;
        var loginTo = tgUser.Username;

        if (!User.IsAllowExecute(balance, NovelUserAssets.CRYSTAL))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: Locale.Get(Locales.InsufficientBalance),
                cancellationToken: ct);
            return;
        }

        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: $"@{loginFrom} payed @{loginTo} {balance}{NovelUserAssets.CRYSTAL.GetEmojiFor()}",
            cancellationToken: ct);

        await User.GrantCoinsAsync(Db, -balance);
        await toUser.GrantCoinsAsync(Db, balance);
    }
}

