namespace nai.db;

using Google.Cloud.Firestore;

public class FireStoreAdapter(FireStoreConnectionString path) : IDbAdapter
{
    private FirestoreDb db { get; } = FirestoreDb.Create(path.ProjectId);
    public ICollectionReference Nai => new FireStoreCollectionReference(db.Collection(path.InitialCollection));
}

public readonly struct FireStoreConnectionString(string path)
{
    public string ProjectId => path.Split("://").First();
    public string InitialCollection => path.Split("://").Last();

    public static implicit operator FireStoreConnectionString(string s) => new(s);
}



public class FireStoreCollectionReference(CollectionReference collection) : ICollectionReference
{

    public IDocumentReference Document(string name) 
        => new FireStoreDocumentReference(collection, collection.Document(name));

    public IAsyncEnumerable<IDocumentReference> ListDocumentsAsync() 
        => collection.ListDocumentsAsync().Select(x => new FireStoreDocumentReference(collection, x));
}

public class FireStoreDocumentReference(CollectionReference collection, DocumentReference document) : IDocumentReference
{
    public async Task<IDocumentSnapshot> GetSnapshotAsync(CancellationToken ct = default)
    {
        var snap = await document.GetSnapshotAsync(ct);
        return new FireStoreDocumentSnapShot(snap);
    }

    public ICollectionReference Collection(string name)
        => new FireStoreCollectionReference(document.Collection(name));

    public Task CreateAsync(object data)
        => document.CreateAsync(data);

    public Task SetAsync(object data, SetOptions options) 
        => document.SetAsync(data, options);
}

public class FireStoreDocumentSnapShot(DocumentSnapshot snapshot) : IDocumentSnapshot
{
    public T ConvertTo<T>() => snapshot.ConvertTo<T>();

    public T GetValue<T>(string name) => snapshot.GetValue<T>(name);

    public bool Exists => snapshot.Exists;
}

public interface IDbAdapter
{
    ICollectionReference Nai { get; }
}


public interface ICollectionReference
{
    IDocumentReference Document(string name);

    IAsyncEnumerable<IDocumentReference> ListDocumentsAsync();
}

public interface IDocumentReference
{
    Task<IDocumentSnapshot> GetSnapshotAsync(CancellationToken ct = default);
    ICollectionReference Collection(string name);
    Task CreateAsync(object data);

    Task SetAsync(object data, SetOptions options);
}

public interface IDocumentSnapshot
{
    T ConvertTo<T>();

    T GetValue<T>(string name);


    bool Exists { get; }
}