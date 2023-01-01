namespace NAIBot.db;

public static class NovelUserAssetsEx
{
    public static string GetEmojiFor(this NovelUserAssets asset) =>
        asset switch
        {
            NovelUserAssets.CROWN => "👑",
            NovelUserAssets.CRYSTAL => "💎",
            _ => "🌟"
        };
}