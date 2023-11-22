namespace nai.nai;
using Newtonsoft.Json;
using Sodium;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class Keystore
{
    private Dictionary<string, byte[]> data;
    private Dictionary<string, byte[]> keystore;
    private byte[] nonce;
    private int version;
    private bool decrypted;
    private bool compressed;

    public Keystore(Dictionary<string, byte[]> keystore)
    {
        data = keystore;
        this.keystore = null;
        nonce = Array.Empty<byte>();
        version = 0;
        decrypted = false;
        compressed = false;
    }

    public byte[] this[string key]
    {
        get
        {
            if (!decrypted)
                throw new Exception("Cannot get key from an encrypted keystore");

            return keystore[key];
        }
        set
        {
            if (!decrypted)
                throw new Exception("Cannot set key in an encrypted keystore");

            keystore[key] = value;
        }
    }

    public bool ContainsKey(string key)
    {
        if (!decrypted)
            throw new Exception("Cannot find key in an encrypted keystore");

        return keystore.ContainsKey(key);
    }

    public void Remove(string key)
    {
        if (!decrypted)
            throw new Exception("Cannot delete key from an encrypted keystore");

        keystore.Remove(key);
    }

    public int Count
    {
        get
        {
            if (!decrypted)
                throw new Exception("Cannot get length of an encrypted keystore");

            return keystore.Count;
        }
    }

    public override string ToString()
    {
        if (!decrypted)
            throw new Exception("Cannot show an encrypted keystore");

        return JsonConvert.SerializeObject(keystore);
    }

    public string Create()
    {
        if (!decrypted)
            throw new Exception("Cannot set key in an encrypted keystore");

        string meta = keystore.Keys.FirstOrDefault();
        while (keystore.ContainsKey(meta))
        {
            meta = Guid.NewGuid().ToString();
        }

        keystore[meta] = SecretBox.GenerateNonce();

        return meta;
    }

    public void Decrypt(byte[] key)
    {
        var keystoreData = new Dictionary<string, byte[]>(data);

        if (keystoreData.ContainsKey("keystore") && keystoreData["keystore"] == null)
        {
            nonce = SecretBox.GenerateNonce();
            version = 2;
            data = new Dictionary<string, byte[]>
            {
                ["keystore"] = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
                {
                    version,
                    nonce = nonce.ToList(),
                    sdata = "",
                }))
            };

            keystore = new Dictionary<string, byte[]>();
            compressed = false;
            decrypted = true;

            return;
        }

        var encryptedKeystore = Convert.FromBase64String(Encoding.UTF8.GetString(keystoreData["keystore"]));
        var decryptedData = Array.Empty<byte>();
        var isCompressed = false;

        try
        {
            ;
            decryptedData = SecretAeadAes.Decrypt(encryptedKeystore, nonce, key);
            isCompressed = true;
        }
        catch (CryptographicException)
        {
            try
            {
                decryptedData = SecretAeadAes.Decrypt(encryptedKeystore, nonce, key);
            }
            catch (CryptographicException)
            {
                throw new Exception("Invalid keystore format");
            }
        }

        var jsonData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<byte>>>>(Encoding.UTF8.GetString(decryptedData));

        keystore = jsonData["keys"].ToDictionary(entry => entry.Key, entry => entry.Value.ToArray());

        compressed = isCompressed;
        decrypted = true;
    }

    public void Encrypt(byte[] key)
    {
        if (!decrypted)
            return;

        var keystoreBytes = keystore.ToDictionary(entry => entry.Key, entry => entry.Value.ToList());

        if (keystoreBytes.Count == 0)
        {
            Dictionary<string, object> keystoreData = new Dictionary<string, object>
            {
                ["version"] = version,
                ["nonce"] = nonce.ToList(),
                ["sdata"] = "",
            };

            data["keystore"] = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(keystoreData));
        }
        else
        {
            Dictionary<string, List<byte>> keys = new Dictionary<string, List<byte>>();
            foreach (var entry in keystoreBytes)
            {
                keys[entry.Key] = entry.Value.ToList();
            }

            Dictionary<string, Dictionary<string, List<byte>>> jsonData = new Dictionary<string, Dictionary<string, List<byte>>>
            {
                ["keys"] = keys
            };

            var json = JsonConvert.SerializeObject(jsonData);
            byte[] encryptedData = SecretAeadAes.Encrypt(Encoding.UTF8.GetBytes(json), nonce, key);
            data["keystore"] = encryptedData.Skip(nonce.Length).ToArray();
        }
    }
}