namespace nai.i18n;

using YamlDotNet.Serialization;
using Microsoft.Extensions.Logging;

public class Localization(Config config, ILogger<Localization> logger)
{
    private bool isLoaded => _locales.Any();

    public string Get(string key)
        => FetchContract(config.DefaultLocale).Get(key);
    public string Get(string key, params object[] args)
        => string.Format(FetchContract(config.DefaultLocale).Get(key), args);


    private readonly Dictionary<string, LocaleContract> _locales = new();

    private void Load()
    {
        if (isLoaded) return;

        var dir = new DirectoryInfo("./locales");

        var deserializer = new DeserializerBuilder()
            .Build();

        foreach (var file in dir.EnumerateFiles("*.yaml"))
        {
            var all = File.ReadAllText(file.FullName);
            var contract = deserializer.Deserialize<LocaleContract>(all);
            _locales.Add(file.Name.Replace(file.Extension, ""), contract);
            logger.LogInformation("Loaded {localeName}", file.Name);
        }
    }


    public ILocaleContract FetchContract(string key)
    {
        if (!isLoaded)
            Load();

        if (_locales.TryGetValue(key, out var contract))
            return contract;
        return _locales["en-US"];
    }
}