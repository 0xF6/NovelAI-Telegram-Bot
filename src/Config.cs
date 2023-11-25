using nai.commands;
using nai.nai;

namespace nai;

using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using db;

public static class Config
{
    public static void Init()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(new DirectoryInfo("./").FullName)
            .AddYamlFile("config.yml")
            .AddEnvironmentVariables();
        Root = builder.Build();


        var db = Root.GetSection("Database");

        var asd = db.GetSection("Firestore");
    }

    public static IConfigurationRoot Root;


    private static string GetValue([CallerMemberName] string? key = null)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        var sec = Root.GetSection(key);

        if (sec.Exists() && !string.IsNullOrWhiteSpace(sec.Value))
            return sec.Value!;

        throw new KeyNotFoundException($"Key '{key}' not found in config.json or env variables.");
    }


    public static string TelegramBotToken => GetValue();
    public static bool DebugRequests => bool.Parse(GetValue());

    public static bool ModeOfChannel => bool.Parse(GetValue());

    public static long MainAdministrator => long.Parse(GetValue());

    public static string CrystallFormula => Root.GetSection("Nai").GetSection("CrystallFormula").Value;
    public static string NovelAiEngine => Root.GetSection("Nai").GetSection("SelectedModel").Value;
    public static string DefaultLocale => GetValue();

    public static bool CommandIsActive(Command command)
    {
        try
        {
            return bool.Parse(Root.GetSection("Commands").GetSection(command.GetType().Name).Value!);
        }
        catch 
        {
            return false;
        }
    }

    public static bool IsDbActive(Db.DbKind kind)
        => bool.Parse(Root.GetSection("Database").GetSection(kind.ToString()).GetSection("IsActive").Value!);


    public static NaiSettings GetNaiSettings()
    {
        var settings = new NaiSettings();
        Root.GetSection("nai").Bind(settings, x => x.ErrorOnUnknownConfiguration = true);
        return settings;
    }

    public static InvoiceConfig GetInvoiceConfig()
    {
        var config = new InvoiceConfig();
        Root.GetSection("invoice").Bind(config, x => x.ErrorOnUnknownConfiguration = true);
        return config;
    }

    public static string GetDbPath(Db.DbKind kind)
        => Root.GetSection("Database").GetSection(kind.ToString()).GetSection("ConnectionString").Value!;
}

public class NaiSettings
{
    public string GenerationUrl { get; set; }
    public string AuthToken { get; set; }
    public string CrystallFormula { get; set; }
    public string EnhanceFormula { get; set; }
    public string VariationFormula { get; set; }
    public string SeedFormula { get; set; }
    public int VariationSize { get; set; }
    public string DefaultModel { get; set; }
    public Dictionary<string, ModelSettings> PerModel { get; set; }

    public ModelSettings For(NovelAIEngine engine) => PerModel[engine.key];
    public ModelSettings For(string engine) => PerModel[engine];
}

public class ModelSettings
{
    public string inherit { get; set; }
    public int? width { get; set; }
    public int? height { get; set; }
    public int? guidance { get; set; }
    public string sampler { get; set; }
    public int? DefaultStep { get; set; }
    public int? seed { get; set; }
    public int? n_samples { get; set; }
    public int? ucPreset { get; set; }
    public bool? qualityToggle { get; set; }
    public bool? SMEA { get; set; }
    public bool? DYN { get; set; }
    public bool? dynamic_thresholding { get; set; }
    public int? controlnet_strength { get; set; }
    public bool? legacy { get; set; }
    public bool? add_original_image { get; set; }
    public string negative_prompt { get; set; }
    public float? uncond_scale { get; set; }
    public float? cfg_rescale { get; set; }
    public string noise_schedule { get; set; }

    public static void Fill(ModelSettings modelSettings, NaiSettings settings, NovelAIParams @params)
    {
        if (!string.IsNullOrEmpty(modelSettings.inherit)) 
            Fill(settings.For(modelSettings.inherit), settings, @params);

        modelSettings.Fill(@params);
    }

    private void Fill(NovelAIParams @params)
    {
        if (width is not null) @params.width = width.Value;
        if (height is not null) @params.height = height.Value;
        if (guidance is not null) @params.scale = guidance.Value;
        if (!string.IsNullOrEmpty(sampler)) @params.sampler = sampler;
        if (DefaultStep is not null) @params.steps = DefaultStep.Value;
        @params.seed = -1;
        @params.n_samples = 1;
        if (ucPreset is not null) @params.ucPreset = ucPreset.Value;
        if (qualityToggle is not null) @params.qualityToggle = qualityToggle.Value;
        if (SMEA is not null) @params.sm = SMEA.Value;
        if (DYN is not null) @params.sm_dyn = DYN.Value;
        if (dynamic_thresholding is not null) @params.dynamic_thresholding = dynamic_thresholding.Value;
        if (controlnet_strength is not null) @params.controlnet_strength = controlnet_strength.Value;
        if (legacy is not null) @params.legacy = legacy.Value;
        if (add_original_image is not null) @params.add_original_image = add_original_image.Value;
        if (!string.IsNullOrEmpty(negative_prompt)) @params.negative_prompt = negative_prompt;
        if (uncond_scale is not null) @params.uncond_scale = uncond_scale.Value;
        if (cfg_rescale is not null) @params.cfg_rescale = cfg_rescale.Value;
        if (!string.IsNullOrEmpty(noise_schedule)) @params.noise_schedule = noise_schedule;
    }
}