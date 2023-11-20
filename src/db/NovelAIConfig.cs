using Google.Cloud.Firestore;

namespace nai.db;

[FirestoreData]
public class NovelAIConfig
{
    [FirestoreProperty]
    public string Token { get; set; }
    [FirestoreProperty]
    public string Model { get; set; } = "safe-diffusion";
}