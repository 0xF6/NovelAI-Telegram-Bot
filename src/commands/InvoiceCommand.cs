using Telegram.Bot.Types.Payments;
using Telegram.Bot;

namespace nai.commands;

public class InvoiceCommand : Command
{
    public override List<string> Aliases => new()
    {
        "/invoice"
    };
    public override string QueueName => "payment";

    public override async ValueTask ExecuteAsync(string cmdText, CancellationToken ct)
    {
        var payToken = Environment.GetEnvironmentVariable("TELEGRAM_PAYMENT_PROVIDER_TOKEN");
        var payCurrency = Environment.GetEnvironmentVariable("TELEGRAM_PAYMENT_CURRENCY") ?? "USD";


        if (!ushort.TryParse(cmdText, out var balance))
            return;
        try
        {
            await BotClient.SendInvoiceAsync(CharId, "Purchase of currency", $"Purchase {balance * 5} 👑 {balance * 100} 💎", $"{balance}", payToken, payCurrency,
                new LabeledPrice[]
                {
                    new LabeledPrice($"{balance * 5} 👑 {balance * 100} 💎", (int)(balance) * 100),
                }, replyToMessageId: Message.MessageId, cancellationToken: ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await BotClient.SendTextMessageAsync(
                chatId: CharId,
                replyToMessageId: Message.MessageId,
                text: $"Failed to create order",
                cancellationToken: ct);
        }
    }
}