using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;


/// <summary>
/// セーブマネージャー
/// </summary>
public class SaveManager : SingletonMonoBehaviour<SaveManager>
{
    protected override bool dontDestroyOnLoad { get { return true; } }

    private string filePath;

    private SaveData save = null;

    public SaveData getSaveData { get { return save; } }

    protected override void Awake()
    {
        base.Awake();
        filePath = Application.persistentDataPath + "/" + "savedata.json";
        save = new SaveData();
        Load();

        DebugLogSystem.DebugLog(this, "generate SaveManager");
    }

    public void Save()
    {
        string json = Encode(JsonUtility.ToJson(save));
        StreamWriter streamWriter = new StreamWriter(filePath);
        streamWriter.Write(json);
        streamWriter.Flush();
        streamWriter.Close();
    }
    public void Load()
    {
        try
        {
            StreamReader streamReader = new StreamReader(filePath);
            string json = streamReader.ReadToEnd();
            streamReader.Close();
            var _save = JsonUtility.FromJson<SaveData>(Decode(json));
            if (_save != null)
                save = _save;
        }
        catch(Exception)
        {
            Save();
        }
    }

    private string Encode(string json)
    {
        return json;
    }

    private string Decode(string code)
    {
        return code;
    }

}

[Serializable]
public class SaveData
{
    /// <summary>
    /// セーブバージョン
    /// </summary>
    public const int version = 0;

    /// <summary>
    /// マスター音量
    /// </summary>
    [SerializeField]
    private float masterVolume = 1.0f;

    public float MasterVolume
    {
        get { return masterVolume; }
        set { masterVolume = Mathf.Clamp(value, 0.0f, 1.0f); }
    }

    /// <summary>
    /// SEの音量
    /// </summary>
    [SerializeField]
    private float seVolume = 1.0f;
    public float SeVolume
    {
        get { return seVolume; }
        set { seVolume = Mathf.Clamp(value, 0.0f, 1.0f); }
    }

    /// <summary>
    /// BGMの音量
    /// </summary>
    [SerializeField]
    private float bgmVolume = 1.0f;
    public float BgmVolume
    {
        get { return bgmVolume; }
        set { bgmVolume = Mathf.Clamp(value, 0.0f, 1.0f); }
    }

}
