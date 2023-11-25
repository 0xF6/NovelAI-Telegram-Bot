using nai;
using nai.db;
using nai.i18n;
using Telegram.Bot;

public class GrantBalanceCommand : Command
{
    public override List<string> Aliases
        => new() { "/grant" };
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (!float.TryParse(cmdText, out var balance))
            return;

        if (!User.IsAdmin && Config.MainAdministrator != User.Id) return;

        var tgUser = Message.ReplyToMessage!.From;
        var toUser = await Db.GetUser(tgUser!);

        await toUser.GrantCoinsAsync(NovelUserAssets.CRYSTAL, balance);
        
        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: string.Format(Locale.Get(Locales.CoinsGranted), tgUser.Username, balance),
            cancellationToken: ct);
    }
}