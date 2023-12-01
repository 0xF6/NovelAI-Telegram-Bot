namespace nai;

using Flurl.Http;
using Ionic.Zip;
using System.IO;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
        catch (Exception e) 
        {
            throw new Exception($"Error when finding '{key}' in All", e);
        }
    }

    public static bool HasKey(string key) =>
        All.Any(x => x.key.Equals(key.Trim(), StringComparison.InvariantCultureIgnoreCase));
}


public class NovelAI
{
    private readonly Config _config;
    public FlurlClient flurl;

    public NovelAI(Config config)
    {
        _config = config;
        var settings = config.GetNaiSettings();
        flurl = new FlurlClient(settings.BaseDomainUrl);

        flurl.WithHeader("Authorization",
            $"Bearer {settings.AuthToken}");
    }


    public IFlurlRequest Request(string url)
        => flurl.Request(url);


    public Task<NovelAiUserData> GetUserData(CancellationToken ct = default) 
        => flurl.Request("/user/data").GetJsonAsync<NovelAiUserData>(ct);


    public async Task<List<MemoryStream>> GenerateRequest(ITelegramBotClient botClient, long chatId, Message message, NovelAIinput input, CancellationToken ct = default)
    {
        var result = await this
            .Request("/ai/generate-image")
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

        if (_config.DebugRequests)
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

            await reader.CopyToAsync(stream, ct);

            stream.Position = 0;


            list.Add(stream);
        }
        return list;
    }
}

public class Information
{
    public bool emailVerified { get; set; }
    public bool emailVerificationLetterSent { get; set; }
    public bool trialActivated { get; set; }
    public int trialActionsLeft { get; set; }
    public int trialImagesLeft { get; set; }
    public int accountCreatedAt { get; set; }
}

public class Keystore
{
    public string keystore { get; set; }
    public int changeIndex { get; set; }
}

public class Perks
{
    public int maxPriorityActions { get; set; }
    public int startPriority { get; set; }
    public int moduleTrainingSteps { get; set; }
    public bool unlimitedMaxPriority { get; set; }
    public bool voiceGeneration { get; set; }
    public bool imageGeneration { get; set; }
    public bool unlimitedImageGeneration { get; set; }
    public List<UnlimitedImageGenerationLimit> unlimitedImageGenerationLimits { get; set; }
    public int contextTokens { get; set; }
}

public class Priority
{
    public int maxPriorityActions { get; set; }
    public int nextRefillAt { get; set; }
    public int taskPriority { get; set; }
}

public class NovelAiUserData
{
    public Priority priority { get; set; }
    public Subscription subscription { get; set; }
    public Keystore keystore { get; set; }
    public string settings { get; set; }
    public Information information { get; set; }
}

public class Subscription
{
    public int tier { get; set; }
    public bool active { get; set; }
    public int expiresAt { get; set; }
    public Perks perks { get; set; }
    public object paymentProcessorData { get; set; }
    public TrainingStepsLeft trainingStepsLeft { get; set; }
    public int accountType { get; set; }
}

public class TrainingStepsLeft
{
    public int fixedTrainingStepsLeft { get; set; }
    public int purchasedTrainingSteps { get; set; }
}

public class UnlimitedImageGenerationLimit
{
    public int resolution { get; set; }
    public int maxPrompts { get; set; }
}