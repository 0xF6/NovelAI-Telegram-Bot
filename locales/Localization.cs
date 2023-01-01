using NAIBot.locales.dicts;

namespace NAIBot.locales;

public class Localization
{
    private readonly string _code;

    public static Dictionary<string, ILocaleContract> _locales = new()
    {
        { "ru", new RuLocaleContract() },
        { "en", new EnLocaleContract() },
        { "ch", new ChLocaleContract() },
        { "fallback", new EnLocaleContract() }
    };

    public Localization(string code) => _code = code;

    public string Get(string key)
        => FetchContract(_code).Get(key);
    public string Get(string key, params object[] args)
        => string.Format(FetchContract(_code).Get(key), args);


    public static ILocaleContract FetchContract(string key)
    {
        if (_locales.ContainsKey(key))
            return _locales[key];
        return _locales["fallback"];
    }
}