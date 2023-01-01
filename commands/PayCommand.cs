using NAIBot.db;
using Telegram.Bot;

namespace NAIBot.commands;

public abstract class PayCommand : Command
{
    public abstract NovelUserAssets GetSelectedAsset();

    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (!uint.TryParse(cmdText, out var balance))
            return;


        var tgUser = Message.ReplyToMessage!.From;
        var toUser = await Db.GetUser(tgUser);

        var loginFrom = Message.From!.Username;
        var loginTo = tgUser.Username;

        if (!User.IsAllowExecute(balance, GetSelectedAsset()))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Insufficient balance of  {GetSelectedAsset().GetEmojiFor()}",
                cancellationToken: ct);
            return;
        }

        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: $"@{loginFrom} payed @{loginTo} {balance}{GetSelectedAsset().GetEmojiFor()}",
            cancellationToken: ct);

        await User.GrantCoinsAsync(GetSelectedAsset(), -balance);
        await toUser.GrantCoinsAsync(GetSelectedAsset(), balance);
    }
}



public class PayCrownCommand : PayCommand
{
    public override List<string> Aliases => new() { "/pay_crown" };
    public override NovelUserAssets GetSelectedAsset() => NovelUserAssets.CROWN;
}

public class PayCrystalCommand : PayCommand
{
    public override List<string> Aliases => new() { "/pay_crystal" };
    public override NovelUserAssets GetSelectedAsset() => NovelUserAssets.CRYSTAL;
}