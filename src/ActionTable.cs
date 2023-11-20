using Newtonsoft.Json;
using System;

namespace NAIBot;

public class ActionTable
{
    public ActionTable()
    {
        if (!Directory.Exists("./actions"))
            Directory.CreateDirectory("./actions");
    }
    public Guid Create(KeyboardImageGeneratorData data)
    {
        var guid = Guid.NewGuid();
        data.id = guid;
        File.WriteAllText($"./actions/{guid:N}.table", JsonConvert.SerializeObject(data, Formatting.Indented));
        return guid;
    }

    public void Remote(Guid id)
    {
        if (!File.Exists($"./actions/{id:N}.table"))
            return;
        File.Delete($"./actions/{id:N}.table");
    }

    public KeyboardImageGeneratorData? Load(Guid id)
    {
        if (!File.Exists($"./actions/{id:N}.table"))
            return null;
        return JsonConvert.DeserializeObject<KeyboardImageGeneratorData>(File.ReadAllText($"./actions/{id:N}.table"));
    }
}