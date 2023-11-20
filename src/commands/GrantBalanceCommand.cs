using nai.db;
using Telegram.Bot;

public class GrantBalanceCommand : Command
{
    public override List<string> Aliases
        => new() { "/grant@novelai_sanrioslut_bot", "/grant" };
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (!float.TryParse(cmdText, out var balance))
            return;

        if (!User.IsAdmin)
            return;

        var tgUser = Message.ReplyToMessage!.From;
        var toUser = await Db.GetUser(tgUser!);

        await toUser.GrantCoinsAsync(NovelUserAssets.CRYSTAL, balance);
        
        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: $"Пользователю @{tgUser.Username} выдано {balance}💎!",
            cancellationToken: ct);
    }
}