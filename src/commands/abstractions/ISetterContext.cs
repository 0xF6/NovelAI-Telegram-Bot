using NAIBot.db;
using Telegram.Bot;
using Telegram.Bot.Types;

public interface ISetterContext
{
    void Set(Message msg, NovelUser user, ITelegramBotClient botClient);
}