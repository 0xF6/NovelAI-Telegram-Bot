using nai;
using nai.nai;
using Newtonsoft.Json;

public record NovelAIParams
{
    private NovelAIParams(){}

    public int width { get; set; } = 512;
    public int height { get; set; } = 768;
    public int scale { get; set; } = 5;
    public string sampler { get; set; }
    public int steps { get; set; } = 28;
    public long seed { get; set; } = -1;
    public int n_samples { get; set; } = 1;
    public int ucPreset { get; set; } = 0;
    public bool qualityToggle { get; set; } = true;
    public bool sm { get; set; } = false;
    public bool sm_dyn { get; set; } = false;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? dynamic_thresholding { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? controlnet_strength { get; set; }
    public bool legacy { get; set; } = false;
    public bool add_original_image { get; set; }
    public string negative_prompt { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string noise_schedule { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float? cfg_rescale { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float? uncond_scale;



    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string image { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float? strength { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float? noise { get; set; }


    public static NovelAIParams Create(NaiSettings settings, NovelAIEngine engine, long seed)
    {
        var model = settings.For(engine);
        var @params = new NovelAIParams();
        ModelSettings.Fill(model, settings, @params);
        @params.seed = seed;
        return @params;
    }
}