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
        Load();
        Save();
    }

    public void Save()
    {
        if(saveData == null)
        {
            DebugManager.Instance.ErrorLog(this, "セーブするデータが生成されておりません。セーブデータを確認ください。");
            return;
        }
        saveData.Save();
    }
    public void Load()
    {
        if (saveData == null)
        {
            DebugManager.Instance.ErrorLog(this, "セーブするデータが生成されておりません。セーブデータを確認ください。");
            return;
        }
        saveData.Load();
    }

}

[Serializable]
public class MainSaveData : SaveDataBase
{

    [SerializeField]
    [SaveData("SaveVersion",typeof(int) , -1)]
    public int mSaveVersion = 0;
    [SerializeField]
    [SaveData("MasterVolume",typeof(float), -1.0f)]
    public float mMasterVolume = 1.0f;
    [SerializeField]
    [SaveData("SEVolume",typeof(float), -1.0f)]
    public float mSEVolume = 1.0f;
    [SerializeField]
    [SaveData("BGMVolume",typeof(float), -1.0f)]
    public float mBGMVolume = 1.0f;

    [SerializeField]
    [SaveData("Player", typeof(PlayerSaveData))]
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
    [SaveData("UserName",typeof(string), "")]
    public string mUserName = "";
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
                if (saveAtt.type == typeof(int))
                {
                    int saveValue = PlayerPrefs.GetInt(name);
                    if (!saveAtt.isErrorValue(saveValue))
                    {
                        field.SetValue(this, saveValue);
                    }
                }
                else if (saveAtt.type == typeof(float))
                {
                    float saveValue = PlayerPrefs.GetFloat(name);
                    if (!saveAtt.isErrorValue(saveValue))
                    {
                        field.SetValue(this, saveValue);
                    }
                }
                else if (saveAtt.type == typeof(string))
                {
                    string saveValue = PlayerPrefs.GetString(name);
                    if (!saveAtt.isErrorValue(saveValue))
                    {
                        field.SetValue(this, saveValue);
                    }
                }
                else if(saveAtt.type.IsSubclassOf(typeof(SaveDataBase)))
                {
                    SaveDataBase saveValue = value as SaveDataBase;
                    if(saveValue == null)
                    {
                        saveValue = (SaveDataBase)Activator.CreateInstance(saveAtt.type);
                    }
                    saveValue.Load();
                    field.SetValue(this, saveValue);
                }
            }
        }   
    }

    public void Save(bool isFirst = true)
    {
        if (isFirst)
        {
            PlayerPrefs.DeleteAll();
        }

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
                    Base.Save(false);
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
    public Type type;
    public object errorValue;

    public SaveDataAttribute(string _key,Type _type, object _errorValue = null)
    {
        key = _key;
        type = _type;
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
