using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace nai.db;

using LiteDB;

public class LiteDbAdapter(string path) : IDbAdapter
{
    private readonly LiteDatabase db = new(path);

    public ICollectionReference Nai
    {
        get
        {
            BsonMapper.Global.Entity<LiteDbCollectionTable>()
                .Id(x => x.Id)
                .DbRef(x => x.Documents);
            BsonMapper.Global.Entity<LiteDbDocumentTable>()
                .Id(x => x.Id);

            return new LiteDbCollectionReference(db, db.GetCollection<LiteDbCollectionTable>("nai"));
        }
    }
}


public class LiteDbCollectionTable
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<LiteDbDocumentTable> Documents { get; set; }
}

public class LiteDbDocumentTable
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Body { get; set; }

}

public class LiteDbCollectionReference(LiteDatabase db, ILiteCollection<LiteDbCollectionTable> collection) : ICollectionReference
{
    public IDocumentReference Document(string name)
    {
        collection.EnsureIndex(x => x.Id);
        collection.EnsureIndex(x => x.Name);
        var table = GetTable();


        if (table.Documents.Any(x => x.Name.Equals(name)))
            return new LiteDbDocumentReference(db, table.Documents.First(x => x.Name.Equals(name)));

        var doc = new LiteDbDocumentTable();


        doc.Id = Guid.NewGuid();
        doc.Name = name;

        var col = db.GetCollection<LiteDbDocumentTable>();

        col.EnsureIndex(x => x.Name);
        col.EnsureIndex(x => x.Id);

        col.Insert(doc);

        table.Documents.Add(doc);
        collection.Update(table);

        return new LiteDbDocumentReference(db, doc);
    }


    private LiteDbCollectionTable GetTable()
    {
        if (!collection.Exists(x => x.Name == collection.Name))
        {
            var table = new LiteDbCollectionTable()
            {
                Name = collection.Name,
                Documents = new List<LiteDbDocumentTable>(),
                Id = Guid.NewGuid()
            };
            collection.Insert(table);

            return table;
        }
        return collection.Include(x => x.Documents).FindOne(x => x.Name == collection.Name);
    }

    public IAsyncEnumerable<IDocumentReference> ListDocumentsAsync()
    {
        var table = GetTable();
        var list = table.Documents.Select(x => new LiteDbDocumentReference(db, x));

        return list.ToAsyncEnumerable();
    }
}

public class LiteDbDocumentReference(LiteDatabase db, LiteDbDocumentTable docTable) : IDocumentReference
{
    public async Task<IDocumentSnapshot> GetSnapshotAsync(CancellationToken ct = default)
    {
        return new LiteDbDocumentSnapShot(docTable);
    }

    public ICollectionReference Collection(string name)
        => new LiteDbCollectionReference(db, db.GetCollection<LiteDbCollectionTable>(EncapsulateCollectionName(name)));

    private string EncapsulateCollectionName(string name)
    {
        if (name.Equals("$"))
            return "_self";
        return name;
    }

    public Task CreateAsync(object data)
    {
        docTable.Body = JsonConvert.SerializeObject(data);
        db.GetCollection<LiteDbDocumentTable>().Update(docTable);
        return Task.CompletedTask;
    }

    public Task SetAsync(object data, SetOptions options) 
        => CreateAsync(data);
}

public class LiteDbDocumentSnapShot(LiteDbDocumentTable table) : IDocumentSnapshot
{
    public T ConvertTo<T>() => JsonConvert.DeserializeObject<T>(table.Body)!;

    public T GetValue<T>(string name)
    {
        var jtoken = JToken.Parse(table.Body);

        return jtoken[name].ToObject<T>();
    }

    public bool Exists => !string.IsNullOrEmpty(table.Body);
}