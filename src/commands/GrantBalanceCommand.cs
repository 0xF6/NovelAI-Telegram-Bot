﻿namespace nai;

using i18n;
using Telegram.Bot;

public class GrantBalanceCommand : Command
{
    public override string Aliases => "grant";
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (!float.TryParse(cmdText, out var balance))
            return;

        if (!User.IsAdmin && Config.MainAdministrator != User.Id) return;

        var tgUser = Message.ReplyToMessage!.From;
        var toUser = await Db.GetUser(tgUser!);

        await toUser.GrantCoinsAsync(Db, balance);
        
        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: string.Format(Locale.Get(Locales.CoinsGranted), tgUser.Username, balance),
            cancellationToken: ct);
    }
}