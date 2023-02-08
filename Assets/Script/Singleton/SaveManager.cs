using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;


/// <summary>
/// セーブマネージャー
/// </summary>
public class SaveManager : SingletonMonoBehaviour<SaveManager>
{
    protected override bool dontDestroyOnLoad { get { return true; } }

    private MainSaveData saveData = null;

    protected void Start()
    {
        saveData = new MainSaveData();
        saveData.Load();
        saveData.Save();
    }

}

[Serializable]
public class MainSaveData : SaveDataBase
{

    [SerializeField]
    [SaveData("SaveVersion", 0 , -1)]
    public int mSaveVersion;
    [SerializeField]
    [SaveData("MasterVolume", 1.0f, -1.0f)]
    public float mMasterVolume;
    [SerializeField]
    [SaveData("SEVolume", 1.0f, -1.0f)]
    public float mSEVolume;
    [SerializeField]
    [SaveData("BGMVolume", 1.0f, -1.0f)]
    public float mBGMVolume;

    [SerializeField]
    [SaveData("Player")]
    public PlayerSaveData mPlayer;

    public void setMasterVolume(float _volume)
    {
        mMasterVolume = Mathf.Clamp(_volume,0.0f, 1.0f);
    }
    public void setSEVolume(float _volume)
    {
        mSEVolume = Mathf.Clamp(_volume, 0.0f, 1.0f);
    }
    public void setBGMVolume(float _volume)
    {
        mBGMVolume = Mathf.Clamp(_volume, 0.0f, 1.0f);
    }
}
[Serializable]
public class PlayerSaveData : SaveDataBase
{
    [SerializeField]
    [SaveData("UserName", "", null)]
    private string mUserName;
}

[Serializable]
public class SaveDataBase : object
{
    public void Load()
    {
        // 自分のクラスのフィールドを探索する
        var fieldList = this.GetType().GetFields();
        DebugManager.Instance.DebugLog(null, this.GetType().ToString());
        foreach (System.Reflection.FieldInfo field in fieldList)
        {
            // セーブ対象のフィールドだった場合セーブデータからロードする
            var att = System.Attribute.GetCustomAttribute(field, typeof(SaveDataAttribute));
            var value = field.GetValue(this);
            if (att != null && att is SaveDataAttribute saveAtt)
            {
                string name = this.GetType().Name + "_" + saveAtt.key;
                // 
                if (value is int i)
                {
                    int saveValue = PlayerPrefs.GetInt(name);
                    if (saveAtt.isErrorValue(saveValue))
                    {
                        field.SetValue(this, saveAtt.initValue);
                    }
                    else
                    {
                        field.SetValue(this, saveValue);
                    }
                }
                else if (value is float f)
                {
                    float saveValue = PlayerPrefs.GetFloat(name);
                    if (saveAtt.isErrorValue(saveValue))
                    {
                        field.SetValue(this, saveAtt.initValue);
                    }
                    else
                    {
                        field.SetValue(this, saveValue);
                    }
                }
                else if (value is string str)
                {
                    string saveValue = PlayerPrefs.GetString(name);
                    if (saveAtt.isErrorValue(saveValue))
                    {
                        field.SetValue(this, saveAtt.initValue);
                    }
                    else
                    {
                        field.SetValue(this, saveValue);
                    }
                }else if(value is SaveDataBase Base)
                {
                    Base.Load();
                }
            }
        }   
    }

    public void Save()
    {
        PlayerPrefs.DeleteAll();

        // 自分のクラスのフィールドを探索する
        var fieldList = this.GetType().GetFields();
        foreach (System.Reflection.FieldInfo field in fieldList)
        {
            // セーブ対象のフィールドだった場合セーブする。
            var att = System.Attribute.GetCustomAttribute(field, typeof(SaveDataAttribute));
            var value = field.GetValue(this);
            if (att != null && att is SaveDataAttribute saveAtt)
            {
                string name = this.GetType().Name +"_"+ saveAtt.key;
                // 
                if (value is int i)
                {
                    PlayerPrefs.SetInt(name, i);
                }
                else if (value is float f)
                {
                    PlayerPrefs.SetFloat(name, f);
                }
                else if (value is string str)
                {
                    PlayerPrefs.SetString(name, str);
                }
                else if (value is SaveDataBase Base)
                {
                    Base.Save();
                }
            }
        }
    }
}
[Serializable]
class PlayerData
{
    [SerializeField]
    public int mhp;
}

public class SaveDataAttribute : System.Attribute
{
    public string key;
    public object initValue;
    public object errorValue;

    public SaveDataAttribute(string _key, object _initValue = null, object _errorValue = null)
    {
        key = _key;
        initValue = _initValue;
        errorValue = _errorValue;
    }

    public bool isErrorValue(object _value) { return _value != errorValue; }
}
public class SaveDataSetterAttribute : System.Attribute
{
    public string key;

    public SaveDataSetterAttribute(string _key)
    {
        key = _key;
    }
}
public class SaveDataGetterAttribute : System.Attribute
{
    public string key;

    public SaveDataGetterAttribute(string _key)
    {
        key = _key;
    }
}
