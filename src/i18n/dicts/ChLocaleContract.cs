using nai.i18n;

namespace nai.i18n.dicts;

public class ChLocaleContract : ILocaleContract
{
    [Locale(Locales.MessageStart)]
    public string StartMessage => @"嗨！ 🔥
我是一个基于NovelAI引擎生成图像的机器人！ 👑
机器人是付费的，但可以在聊天@novelai_generation_chat中使用
如果您购买了授权码，请使用/authorize_chat命令
命令及其描述:https://t.me/novelai_generation_chat/1891";

    [Locale(Locales.SendedAuthRequest)]
    public string SentAuthRequest => @"授权请求已发送，请求代码来自 @ivysola";

    [Locale(Locales.AuthSuccess)]
    public string AuthSuccess => @"聊天已被授权!";

    [Locale(Locales.AuthFailed)]
    public string AuthFailed => @"";
}