using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class SaveManager : SingletonMonoBehaviour<SaveManager>
{
    protected override bool dontDestroyOnLoad { get { return true; } }
    private const string keyFilePath = "key.dat";

    private byte[] _key;
    private byte[] _iv;

    SaveData save = null;

    protected override void Awake()
    {
        base.Awake();
        LoadKey();
        if (_key == null || _key.Length == 0)
        {
            _key = new byte[32];
            _iv = new byte[16];
            using (var provider = new RNGCryptoServiceProvider())
            {
                provider.GetBytes(_key);
                provider.GetBytes(_iv);
            }
            SaveKey();
        }
    }

    private void LoadKey()
    {
        if (File.Exists(keyFilePath))
        {
            var data = File.ReadAllBytes(keyFilePath);
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                _key = br.ReadBytes(32);
                _iv = br.ReadBytes(16);
            }
        }
    }

    private void SaveKey()
    {
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms))
        {
            bw.Write(_key);
            bw.Write(_iv);
            File.WriteAllBytes(keyFilePath, ms.ToArray());
        }
    }

    public void Save<T>(T data, string filePath)
    {
        var json = JsonUtility.ToJson(data);
        var encryptedData = EncryptString(json);
        File.WriteAllBytes(filePath, encryptedData);
    }

    public T Load<T>(string filePath) where T : class, new()
    {
        if (!File.Exists(filePath))
        {
            return new T();
        }

        var data = File.ReadAllBytes(filePath);
        var json = DecryptString(data);
        return JsonUtility.FromJson<T>(json);
    }

    private byte[] EncryptString(string plainText)
    {
        byte[] encrypted;
        using (Aes aes = Aes.Create())
        {
            aes.Key = _key;
            aes.IV = _iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    encrypted = ms.ToArray();
                }
            }
        }
        return encrypted;
    }

    private string DecryptString(byte[] cipherText)
    {
        string plaintext = null;
        using (Aes aes = Aes.Create())
        {
            aes.Key = _key;
            aes.IV = _iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        plaintext = sr.ReadToEnd();
                    }
                }
            }
        }
        return plaintext;
    }
}

[Serializable]
public class SaveData
{
    public float MasterVolume = 1.0f;
    public float BGMVolume = 1.0f;
    public float SEVolume = 1.0f;
}