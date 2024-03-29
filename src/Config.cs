﻿using System.Text;
using nai.commands;
namespace nai;

using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
using db;

public class Config(IConfiguration Root)
{
    private string GetValue([CallerMemberName] string? key = null)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        var sec = Root.GetSection(key);

        if (sec.Exists() && !string.IsNullOrWhiteSpace(sec.Value))
            return sec.Value!;

        throw new KeyNotFoundException($"Key '{key}' not found in config.json or env variables.");
    }


    public string TelegramBotToken => GetValue();
    public bool DebugRequests => bool.Parse(GetValue());

    public bool ModeOfChannel => bool.Parse(GetValue());

    public long MainAdministrator => long.Parse(GetValue());

    public string CrystallFormula => Root.GetSection("Nai").GetSection("CrystallFormula").Value;
    public string NovelAiEngine => Root.GetSection("Nai").GetSection("SelectedModel").Value;
    public string DefaultLocale => GetValue();

    public bool CommandIsActive(Command command)
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

    public bool IsDbActive(Db.DbKind kind)
        => bool.Parse(Root.GetSection("Database").GetSection(kind.ToString()).GetSection("IsActive").Value!);

    public string GetDbCredentials(Db.DbKind kind)
    {
        if (kind == Db.DbKind.LiteDb)
            throw new NotSupportedException();
        var dbSection = Root.GetSection("Database").GetSection(kind.ToString());

        var path = dbSection.GetSection("CredentialPath").Value;
        var base64 = dbSection.GetSection("CredentialBase64").Value;


        if (!string.IsNullOrEmpty(path))
            return File.ReadAllText(path);
        if (!string.IsNullOrEmpty(base64))
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        throw new Exception($"No defined correct credential, CredentialPath and CredentialBase64 is null");
    }

    public  NaiSettings GetNaiSettings()
    {
        var settings = new NaiSettings();
        Root.GetSection("nai").Bind(settings, x => x.ErrorOnUnknownConfiguration = true);
        return settings;
    }

    public InvoiceConfig GetInvoiceConfig()
    {
        var config = new InvoiceConfig();
        Root.GetSection("invoice").Bind(config, x => x.ErrorOnUnknownConfiguration = true);
        return config;
    }

    public string GetDbPath(Db.DbKind kind)
        => Root.GetSection("Database").GetSection(kind.ToString()).GetSection("ConnectionString").Value!;
}

public class NaiSettings
{
    public string GenerationUrl { get; set; }
    public string BaseDomainUrl { get; set; }
    public string AuthToken { get; set; }
    public string CrystallFormula { get; set; }
    public string EnhanceFormula { get; set; }
    public string VariationFormula { get; set; }
    public string SeedFormula { get; set; }
    public int VariationSize { get; set; }
    public string DefaultModel { get; set; }
    public Dictionary<string, ModelSettings> PerModel { get; set; }

    public Dictionary<string, bool> ModelActive { get; set; }

    public ModelSettings For(NovelAIEngine engine) => PerModel[engine.key];
    public ModelSettings For(string engine) => PerModel[engine];

    public bool IsActiveEngine(NovelAIEngine engine)
    {
        if (ModelActive.TryGetValue(engine.key, out var active))
            return active;
        return false;
    }
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