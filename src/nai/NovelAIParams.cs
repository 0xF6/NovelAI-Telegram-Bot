using Newtonsoft.Json;

public record NovelAIParams(
    bool qualityToggle = true,
    int scale = 11,
    string sampler = "k_euler_ancestral",
    int n_samples = 1,
    int ucPreset = 0
)
{
    public string uc =
        "lowres, bad anatomy, bad hands, text, error, missing fingers, extra digit, fewer digits, cropped, worst quality, low quality, normal quality, jpeg artifacts, signature, watermark, username, blurry, lowres, bad anatomy, bad hands, text, error, missing fingers, extra digit, fewer digits, cropped, worst quality, low quality, normal quality, jpeg artifacts, signature, watermark, username, blurry, bad feet, futa, futanari, yaoi,huge_breasts, large_breasts";

    public int stems = 28;
    public int width = 512;
    public int height = 768;
    public long seed = -1;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string image { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float? strength { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float? noise { get; set; }
}