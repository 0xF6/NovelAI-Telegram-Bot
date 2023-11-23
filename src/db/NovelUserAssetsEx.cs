namespace nai.db;

public static class NovelUserAssetsEx
{
    public static string GetEmojiFor(this NovelUserAssets asset) =>
        asset switch
        {
            NovelUserAssets.CRYSTAL => "💎",
            _ => "🌟"
        };
}