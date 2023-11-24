using Flurl.Http;
using Ionic.Zip;
using nai.db;
using Telegram.Bot;
using Telegram.Bot.Types;

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

    public static NovelAIEngine ByKey(string key) =>
        All.First(x => x.key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
}


public class NovelAI
{
    public FlurlClient flurl;

    public NovelAI()
    {
        flurl = new FlurlClient("https://api.novelai.net/ai/generate-image");

        flurl.WithHeader("Authorization",
            $"Bearer {Config.NovelAIToken}");
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