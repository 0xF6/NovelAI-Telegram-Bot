using Google.Cloud.Firestore;
using nai.nai;

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
    [FirestoreProperty]
    public string? SelectedEngine { get; set; }


    public async ValueTask GrantCoinsAsync(NovelUserAssets asset, float coins)
    {
        if (asset == NovelUserAssets.CRYSTAL)
            CrystalCoin += coins;
        await Db.SaveUser(this);
    }

    public async ValueTask SetActiveEngine(NovelAIEngine engine)
    {
        SelectedEngine = engine.key;
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

    public NovelAIEngine GetSelectedEngine(NaiSettings settings)
    {
        if (SelectedEngine == null)
            return NovelAIEngine.ByKey(settings.DefaultModel);
        return NovelAIEngine.ByKey(SelectedEngine);
    }
}