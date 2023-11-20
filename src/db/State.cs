using Google.Cloud.Firestore;

namespace nai.db;

[FirestoreData]
public class State
{
    public List<NovelUser> Users { get; set; } = new();

    public List<long> AllowedChatList { get; set; } = new();
}