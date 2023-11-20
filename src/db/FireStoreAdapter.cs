namespace NAIBot.db;

using Google.Cloud.Firestore;

public class FireStoreAdapter : IFireStoreAdapter
{
    private FirestoreDb db { get; }
    public FireStoreAdapter() => db = FirestoreDb.Create(Environment.GetEnvironmentVariable("FIRESTORE_PROJECT_ID"));
    public CollectionReference Nai => db.Collection("nai");
}

public interface IFireStoreAdapter
{
    CollectionReference Nai { get; }
}