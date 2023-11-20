namespace nai.i18n;

public interface ILocaleContract
{
    string Get(string key)
    {
        var props = GetType().GetProperties();

        foreach (var prop in props)
        {
            var attrs = prop.GetCustomAttributes(true);
            foreach (var attr in attrs)
            {
                if (attr is not LocaleAttribute authAttr) continue;
                if (authAttr.Key == key)
                    return prop.GetValue(this) as string ?? key;
            }
        }

        return key;
    }
}