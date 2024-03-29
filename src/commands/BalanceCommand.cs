﻿using nai.i18n;
using Telegram.Bot;

public class BalanceCommand : Command
{
    public override string Aliases => "balance";
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: string.Format(Locale.Get(Locales.YourBalance), User.CrystalCoin),
            cancellationToken: ct);
    }
}