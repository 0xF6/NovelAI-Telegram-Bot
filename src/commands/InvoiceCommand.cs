namespace nai.commands;

using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Payments;
using Telegram.Bot;

public class InvoiceCommand : Command
{
    public override string Aliases => "invoice";
    public override string QueueName => EngineQueue.Payment;

    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        var cfg = Config.GetInvoiceConfig();


        var payToken = cfg.PaymentProviderToken;
        var payCurrency = cfg.Currency;
        var convertRate = cfg.ConvertRate;

        if (string.IsNullOrEmpty(payToken))
            return;
        if (string.IsNullOrEmpty(payCurrency))
            return;
        if (string.IsNullOrEmpty(convertRate))
            return;

        var from = decimal.Parse(convertRate.Split("->").First().Trim());
        var to = decimal.Parse(convertRate.Split("->").Last().Trim());


        if (!ushort.TryParse(cmdText, out var input))
            return;

        var crystals = (input * from) * to;

        try
        {
            await BotClient.SendInvoiceAsync(CharId, "Purchase of currency", $"Purchase {crystals} 💎", $"{crystals}", payToken, payCurrency,
                new LabeledPrice[]
                {
                    new($"{crystals} 💎", input),
                }, replyToMessageId: Message.MessageId, cancellationToken: ct);
        }
        catch (Exception e)
        {
            Logger.LogCritical(e, "Failed to create order");
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Failed to create order",
                cancellationToken: ct);
        }
    }
}

public class InvoiceConfig
{
    public string PaymentProviderToken { get; set; }
    public string Currency { get; set; }
    public string ConvertRate { get; set; }
}