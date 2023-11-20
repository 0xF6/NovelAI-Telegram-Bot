using nai.i18n;

namespace nai.i18n.dicts;

public class EnLocaleContract : ILocaleContract
{
    [Locale(Locales.MessageStart)]
    public string StartMessage => @"Hi! 🔥
I am a bot for generating images based on the Novela engine 👑
The bot is paid, but is available in the chat @novelai_generation_chat
If you bought an authorization code, use the /authorize_chat command
Commands and their description: https://t.me/novelai_generation_chat/1891";

    [Locale(Locales.SendedAuthRequest)]
    public string SentAuthRequest => @"Authorization request has been sent, request code from @ivysola";

    [Locale(Locales.AuthSuccess)]
    public string AuthSuccess => @"Chat has been authorized!";

    [Locale(Locales.AuthFailed)]
    public string AuthFailed => @"Failed authorization chat, bad passcode.";
}