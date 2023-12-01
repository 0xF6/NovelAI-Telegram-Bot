namespace nai.db;

using System.Numerics;
using Google.Cloud.Firestore;
using nai;
using Telegram.Bot.Types;


public class Db(Config config)
{
    private State? _state;
    private bool isLoaded = false;
    public State State => _state;
    private IDbAdapter _adapter;

    public enum DbKind
    {
        Firestore,
        LiteDb
    }

    public async ValueTask<State> LoadState()
    {
        if (isLoaded) return _state;

        if (config.IsDbActive(DbKind.Firestore))
            _adapter = new FireStoreAdapter(config.GetDbPath(DbKind.Firestore), config);
        else
            _adapter = new LiteDbAdapter(config.GetDbPath(DbKind.LiteDb));


        _state = new State();

        var users = await _adapter.Nai.Document("users").Collection("$").ListDocumentsAsync().ToListAsync();
        var snapshots = await Task.WhenAll(users.Select(x => x.GetSnapshotAsync()));
        _state.Users = snapshots.Select(x => x.ConvertTo<NovelUser>()).ToList();

        var chatsSnapshot = await _adapter.Nai.Document("chats").GetSnapshotAsync();
        if (chatsSnapshot.Exists)
            _state.AllowedChatList = chatsSnapshot.GetValue<List<long>>("data");
        else
            await _adapter.Nai.Document("chats").CreateAsync(new { data = new List<long>() });

        isLoaded = true;
        return _state;
    }

    public async ValueTask<NovelUser> GetUser(User user)
        => State.Users.FirstOrDefault(x => x.Id == user.Id) ?? await AddUser(user);

    public async ValueTask RefreshUser(User user)
    {
        var u = await GetUser(user);
        u.TgLogin = user.Username;
        await SaveUser(u);
    }

    public bool ChatIsAllowed(long chat)
        => _state.AllowedChatList.Any(x => x == chat);

    public async ValueTask AddAllowedChat(long chat)
    {
        _state.AllowedChatList.Add(chat);
        await _adapter.Nai.Document("chats").SetAsync(new { data = _state.AllowedChatList }, SetOptions.Overwrite);
    }

    private async ValueTask<NovelUser> AddUser(User u)
    {
        var user = new NovelUser(u.Id)
        {
            TgLogin = u.Username
        };
        State.Users.Add(user);
        await SaveUser(user);
        return user;
    }


    public long CalculatePrice(NovelAIEngine engine, NovelAIParams @params)
    {
        var formula = new CrystallFormula(config.CrystallFormula);
        return formula.GetPrice(engine, @params.steps, new Vector2(@params.width, @params.height));
    }
    
    public async ValueTask SaveUser(NovelUser novelUser)
    {
        if (novelUser.TgLogin is null)
            return;
        var snapshot = await _adapter.Nai
            .Document("users")
            .Collection("$")
            .Document(novelUser.TgLogin)
            .GetSnapshotAsync();

        if (snapshot.Exists)
        {
            await _adapter.Nai.Document("users").Collection("$").Document(novelUser.TgLogin).SetAsync(novelUser, SetOptions.Overwrite);
        }
        else
        {
            await _adapter.Nai.Document("users").Collection("$").Document(novelUser.TgLogin).CreateAsync(novelUser);
        }
    }
}