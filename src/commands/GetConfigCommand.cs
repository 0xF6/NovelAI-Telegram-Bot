namespace nai.commands;

using nai;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

public class GetConfigCommand : Command
{
    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        var api = new NovelAI(Config);


        var userData = await api.GetUserData(ct);
        var naiSettings = Config.GetNaiSettings();
        var output = new StringBuilder();


        var sub = userData.subscription;
        
        output.AppendLine($"subscription: <code>{(sub.active ? "active" : "not active")}</code>, expired: <code>{DateTimeOffset.FromUnixTimeSeconds(sub.expiresAt)}</code>");
        output.AppendLine(
            $"accountBalance: <code>{sub.trainingStepsLeft.fixedTrainingStepsLeft + sub.trainingStepsLeft.purchasedTrainingSteps}</code> <tg-emoji emoji-id=\"5424785011880509198\">\ud83d\udc8e</tg-emoji>");

        output.AppendLine($"hasDebug: <code>{Config.DebugRequests}</code>");
        output.AppendLine($"mainAdministrator: <a href=\"tg://user?id={Config.MainAdministrator}\">user</a>");
        output.AppendLine($"defaultLocale: <code>{Config.DefaultLocale}</code>");
        output.AppendLine($"model status:");
        foreach (var (key, isActive) in naiSettings.ModelActive)
        {
            //var modelSettings = naiSettings.For(key);
            output.AppendLine($"    <code>{key}: {(isActive ? "active" : "not active")}</code>");
        }

        await BotClient.SendTextMessageAsync(CharId, output.ToString(), replyToMessageId: Message.MessageId, parseMode: ParseMode.Html, cancellationToken: ct).ConfigureAwait(false);
    }

    public override string QueueName => EngineQueue.ImageGeneration; // for limitation
    public override string Aliases => "config";
}