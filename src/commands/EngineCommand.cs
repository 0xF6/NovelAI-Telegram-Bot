namespace nai.commands;

using i18n;
using nai;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;


public class EngineCommand : Command
{
    public override string Aliases => "engine";
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(cmdText))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: string.Format(Locale.Get(Locales.ActiveEngine), User.GetSelectedEngine(Config.GetNaiSettings()).key, string.Join(", ", NovelAIEngine.All.Select(x => $"`{x.key}`"))),
                cancellationToken: ct, parseMode: ParseMode.Markdown).ConfigureAwait(false);
            return;
        }

        if (!NovelAIEngine.HasKey(cmdText))
        {
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: string.Format(Locale.Get(Locales.EngineNotFound), cmdText.Trim()),
                cancellationToken: ct).ConfigureAwait(false);
            return;

        }

        var engine = NovelAIEngine.ByKey(cmdText);
        await User.SetActiveEngine(Db, engine);

        await BotClient.SendTextMessageAsync(
            chatId: CharId,
            replyToMessageId: Message.MessageId,
            text: string.Format(Locale.Get(Locales.EngineHasSwitched), engine.key),
            cancellationToken: ct).ConfigureAwait(false);
    }
}