using System;
using UnityEngine;


/// <summary>
/// セーブマネージャー
/// </summary>
public class SaveManager : SingletonMonoBehaviour<SaveManager>
{
    protected override bool dontDestroyOnLoad { get { return true; } }

    private GameSaveDataBase saveData;

    public void Save()
    {
    }
}

[Serializable]
public class GameSaveDataBase : object
{
    public void Init()
    {
        mMasterVolume = 1.0f;
        mSEVolume = 1.0f;
        mBGMVolume = 1.0f;
    }
    [SerializeField]
    private const int mSaveVersion = 1;
    [SerializeField]
    private float mMasterVolume;
    [SerializeField]
    private float mSEVolume;
    [SerializeField]
    private float mBGMVolume;

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
