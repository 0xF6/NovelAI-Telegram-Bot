using Google.Cloud.Firestore;

namespace nai.db;

[FirestoreData]
public class NovelUser
{
    public NovelUser(long id) => Id = id;
    public NovelUser() { }
    [FirestoreProperty]
    public long Id { get; set; }
    [FirestoreProperty]
    public float CrystalCoin { get; set; }
    [FirestoreProperty]
    public float CrownCoin { get; set; }
    [FirestoreProperty]
    public bool IsAdmin { get; set; }
    [FirestoreProperty]
    public string? TgLogin { get; set; }


    public async ValueTask GrantCoinsAsync(NovelUserAssets asset, float coins)
    {
        if (asset == NovelUserAssets.CRYSTAL)
            CrystalCoin += coins;
        await Db.SaveUser(this);
    }

    public bool IsAllowExecute(float priceCrystal)
        => CrystalCoin - priceCrystal >= 0;

    public bool IsAllowExecuteByCrown(float price)
        => CrownCoin - price >= 0;

    public bool IsAllowExecute(float price, NovelUserAssets asset) =>
        asset switch
        {
            NovelUserAssets.CRYSTAL => IsAllowExecute(price),
            _ => false
        };
}