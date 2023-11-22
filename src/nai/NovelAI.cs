using Flurl.Http;
using Ionic.Zip;
using nai.db;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace nai.nai;


public record NovelAIEngine(string key, long minPrice, long maxPrice)
{
    public static NovelAIEngine SafeDiffusion = new("safe-diffusion", 2, 8);
    public static NovelAIEngine NaiDiffusion = new("nai-diffusion", 2, 8);
    public static NovelAIEngine NaiFurry = new("nai-diffusion-furry", 2, 8);
    public static NovelAIEngine NaiV2 = new("nai-diffusion-2", 2, 24);
    public static NovelAIEngine NaiV3 = new("nai-diffusion-3", 4, 33);

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


    public async Task<MemoryStream?> GenerateRequest(ITelegramBotClient botClient, long chatId, Message message, NovelAIinput input, CancellationToken ct = default)
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
            return null;
        }


        using var zip = ZipFile.Read(await result.GetStreamAsync());


        var element = zip.FirstOrDefault();


        if (element is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                replyToMessageId: message.MessageId,
                text: $"Unhandled error\n{result.StatusCode}, response from novel ai is empty, seed: {input.parameters.seed}",
                cancellationToken: ct);
            return null;
        }

        await using var reader = element.OpenReader();
        var stream = new MemoryStream();

        await reader.CopyToAsync(stream, ct);

        stream.Position = 0;

        Console.WriteLine($"Received {element.FileName}");

        return stream;
    }
}