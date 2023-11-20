namespace NAIBot.locales.dicts;

public class RuLocaleContract : ILocaleContract
{
    [Locale(Locales.MessageStart)]
    public string StartMessage => @"Привет! 🔥
Я - бот для генерации изображений на основе движка NovelAI 👑
Бот платный, но доступен в чате @novelai_generation_chat
Если вы купили код авторизации, используйте комманду /authorize_chat
Комманды и их описание: https://t.me/novelai_generation_chat/1891";
    [Locale(Locales.SendedAuthRequest)]
    public string SentAuthRequest => @"Запросите код авторизации у @ivysola.";

    [Locale(Locales.AuthSuccess)]
    public string AuthSuccess => @"Чат авторизован!";

    [Locale(Locales.AuthFailed)]
    public string AuthFailed => @"Ошибка авторизации, неверный пасс.";
}