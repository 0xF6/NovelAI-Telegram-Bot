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

    public static bool ModeOfChannel => bool.Parse(GetValue());

    public static string NovelAI_Username => GetValue();
    public static long MainAdministrator => long.Parse(GetValue());

    public static string NovelAIToken => GetValue();

    public static string PythonDllPath => GetValue();
    public static string CrystallFormula => Root.GetSection("Nai").GetSection("CrystallFormula").Value;
    public static string NovelAiEngine => Root.GetSection("Nai").GetSection("SelectedModel").Value;


    public static bool CommandIsActive(Command command) 
        => bool.Parse(Root.GetSection("Commands").GetSection(command.GetType().Name).Value!);

    public static bool IsDbActive(Db.DbKind kind)
        => bool.Parse(Root.GetSection("Database").GetSection(kind.ToString()).GetSection("IsActive").Value!);


    public static NaiSettings GetNaiSettings()
    {
        var settings = new NaiSettings();
        Root.GetSection("nai").Bind(settings, x => x.ErrorOnUnknownConfiguration = true);
        return settings;
    }

    public static string GetDbPath(Db.DbKind kind)
        => Root.GetSection("Database").GetSection(kind.ToString()).GetSection("ConnectionString").Value!;
}

public class NaiSettings
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string SelectedModel { get; set; }
    public NegateTags DefaultNegateTags { get; set; }
    public string SelectedSampler { get; set; }
    public bool SMEA { get; set; }
    public bool DYN { get; set; }
    public int Guidance { get; set; }
    public int ucPreset { get; set; }
    public bool qualityToggle { get; set; }
    public bool dynamic_thresholding { get; set; }
    public int controlnet_strength { get; set; }
    public int defaultStep { get; set; }
    public string CrystallFormula { get; set; }
    public string EnhanceFormula { get; set; }
    public string VariationFormula { get; set; }
    public string SeedFormula { get; set; }
    public int VariationSize { get; set; }

    public string SfwNegateTags { get; set; }

    public NovelAIEngine Engine => NovelAIEngine.ByKey(SelectedModel);
}

public class NegateTags
{
    public string Actual { get; set; }
    public string Legacy { get; set; }
}