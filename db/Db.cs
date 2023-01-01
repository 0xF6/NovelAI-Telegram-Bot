using Google.Cloud.Firestore;
using Telegram.Bot.Types;

namespace NAIBot.db;

public class Db
{
    private static State? _state;
    private static bool isLoaded = false;
    public static State State => _state;
    private static FireStoreAdapter _adapter;

    public static async ValueTask<State> LoadState()
    {
        if (isLoaded) return _state;
        _adapter = new FireStoreAdapter();
        _state = new State();

        _state.NovelAI = (await _adapter.Nai.Document("novelai").GetSnapshotAsync()).ConvertTo<NovelAIConfig>();
        var users = await _adapter.Nai.Document("users").Collection("$").ListDocumentsAsync().ToListAsync();
        var snapshots = await Task.WhenAll(users.Select(x => x.GetSnapshotAsync()));
        _state.Users = snapshots.Select(x => x.ConvertTo<NovelUser>()).ToList();

        var chatsSnapshot = await _adapter.Nai.Document("chats").GetSnapshotAsync();
        if (chatsSnapshot.Exists)
            _state.AllowedChatList = chatsSnapshot.GetValue<List<long>>("data");
        else
            await _adapter.Nai.Document("chats").CreateAsync(new { data = new List<long>() });
        var adminSnapshot = await _adapter.Nai.Document("owner").GetSnapshotAsync();
        _state.MainAdministrator = adminSnapshot.GetValue<long>("id");
        isLoaded = true;
        return _state;
    }

    public static async ValueTask<NovelUser> GetUser(User user)
        => State.Users.FirstOrDefault(x => x.Id == user.Id) ?? await AddUser(user);

    public static async ValueTask RefreshUser(User user)
    {
        var u = await GetUser(user);
        u.TgLogin = user.Username;
        await SaveUser(u);
    }

    public static bool ChatIsAllowed(long chat)
        => _state.AllowedChatList.Any(x => x == chat);

    public static async ValueTask AddAllowedChat(long chat)
    {
        _state.AllowedChatList.Add(chat);
        await _adapter.Nai.Document("chats").SetAsync(new { data = _state.AllowedChatList }, SetOptions.Overwrite);
    }

    private static async ValueTask<NovelUser> AddUser(User u)
    {
        var user = new NovelUser(u.Id)
        {
            TgLogin = u.Username
        };
        State.Users.Add(user);
        await SaveUser(user);
        return user;
    }


    public static void SetNovelAIEngine(string model)
    {
        State.NovelAI.Model = model;
    }

    public static long CalculatePrice(NovelAIParams novelAIParams)
        => 6; // TODO

    public static string GetNovelAIToken()
        => State.NovelAI.Token;

    public static async ValueTask SaveUser(NovelUser novelUser)
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