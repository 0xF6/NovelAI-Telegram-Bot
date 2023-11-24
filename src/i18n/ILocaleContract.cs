namespace nai.i18n;

public interface ILocaleContract
{
    string Get(string key)
    {
        var props = GetType().GetProperties();

        foreach (var prop in props)
        {
            if (prop.Name == key)
                return prop.GetValue(this) as string ?? key;
        }

        return key;
    }
}