using Google.Cloud.Firestore;

namespace NAIBot.db;

[FirestoreData]
public class NovelAIConfig
{
    [FirestoreProperty]
    public string Token { get; set; }
    [FirestoreProperty]
    public string Model { get; set; } = "safe-diffusion";
}