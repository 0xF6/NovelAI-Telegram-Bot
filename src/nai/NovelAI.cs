using Flurl.Http;
using Ionic.Zip;
using nai.db;
using System.IO;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace nai.nai;


public record NovelAIEngine(string key, long minPrice, long maxPrice, bool isActual)
{
    public static NovelAIEngine SafeDiffusion = new("safe-diffusion", 2, 8, false);
    public static NovelAIEngine NaiDiffusion = new("nai-diffusion", 2, 8, false);
    public static NovelAIEngine NaiFurry = new("nai-diffusion-furry", 2, 8, false);
    public static NovelAIEngine NaiV2 = new("nai-diffusion-2", 2, 24, false);
    public static NovelAIEngine NaiV3 = new("nai-diffusion-3", 4, 33, true);

    public static List<NovelAIEngine> All = new List<NovelAIEngine>()
        { SafeDiffusion, NaiDiffusion, NaiFurry, NaiV2, NaiV3 };

    public static NovelAIEngine ByKey(string key)
    {
        key = key.Trim();
        try
        {
            return All.First(x => x.key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }
        catch
        {
            Console.WriteLine($"Error when finding '{key}' in All");
            throw;
        }
    }

    public static bool HasKey(string key) =>
        All.Any(x => x.key.Equals(key.Trim(), StringComparison.InvariantCultureIgnoreCase));
}


public class NovelAI
{
    public FlurlClient flurl;

    public NovelAI()
    {
        var settings = Config.GetNaiSettings();
        flurl = new FlurlClient(settings.GenerationUrl);

        flurl.WithHeader("Authorization",
            $"Bearer {settings.AuthToken}");
    }


    public IFlurlRequest Request()
        => flurl.Request();


    public async Task<List<MemoryStream>> GenerateRequest(ITelegramBotClient botClient, long chatId, Message message, NovelAIinput input, CancellationToken ct = default)
    {
        var result = await this
            .Request()
            .AllowAnyHttpStatus()
            .PostJsonAsync(input, cancellationToken: ct);

        if (result.StatusCode is not (200 or 201))
        {
            var strerr = await result.GetStringAsync();
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                replyToMessageId: message.MessageId,
                text: $"Unhandled error\n{result.StatusCode}, {strerr}, seed: {input.parameters.seed}",
                cancellationToken: ct);
            return new List<MemoryStream>();
        }

        if (Config.DebugRequests)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                replyToMessageId: message.MessageId,
                text: $"[Request]\nStatusCode: {result.StatusCode}, seed: {input.parameters.seed}, action: {input.action}, from <a href=\"tg://user?id={message.From!.Id}\">{message.From.FirstName}</a>\n" +
                      $"<pre><code class=\"json\">" +
                      $"{JsonConvert.SerializeObject(input.parameters, Formatting.Indented)}\n" +
                      $"</code></pre>",
                cancellationToken: ct, parseMode: ParseMode.Html);
        }

        using var zip = ZipFile.Read(await result.GetStreamAsync());

        var list = new List<MemoryStream>();

        foreach (var e in zip)
        {
            await using var reader = e.OpenReader();
            var stream = new MemoryStream();
            Console.WriteLine($"Received {e.FileName}");

            await reader.CopyToAsync(stream, ct);

            stream.Position = 0;


            list.Add(stream);
        }
        return list;
    }
}