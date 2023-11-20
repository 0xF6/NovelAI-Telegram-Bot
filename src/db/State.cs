using Google.Cloud.Firestore;

namespace NAIBot.db;

[FirestoreData]
public class State
{
    public List<NovelUser> Users { get; set; } = new();
    [FirestoreProperty]
    public NovelAIConfig NovelAI { get; set; } = new();

    public List<long> AllowedChatList { get; set; } = new();
    public long MainAdministrator { get; set; }
}