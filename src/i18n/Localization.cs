namespace nai.i18n;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class Localization
{
    private readonly string _code;

    public Localization(string code) => _code = code;

    public string Get(string key)
        => FetchContract(_code).Get(key);
    public string Get(string key, params object[] args)
        => string.Format(FetchContract(_code).Get(key), args);


    private readonly Dictionary<string, LocaleContract> _locales = new();

    public void Load()
    {
        var dir = new DirectoryInfo("./locales");

        var deserializer = new DeserializerBuilder()
            //.WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
            .Build();

        foreach (var file in dir.EnumerateFiles("*.yaml"))
        {
            var all = File.ReadAllText(file.FullName);
            var contract = deserializer.Deserialize<LocaleContract>(all);
            _locales.Add(file.Name.Replace(file.Extension, ""), contract);
            Console.WriteLine($"Loaded '{file.Name}'");
        }
    }


    public ILocaleContract FetchContract(string key)
    {
        if (_locales.TryGetValue(key, out var contract))
            return contract;
        return _locales["en-US"];
    }
}