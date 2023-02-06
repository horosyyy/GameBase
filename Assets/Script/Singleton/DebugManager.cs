using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// デバッグ用マネージャー
/// </summary>
public class DebugManager : SingletonMonoBehaviour<DebugManager>
{
    protected override bool dontDestroyOnLoad { get { return true; } }

    #region ログ関係
    public void DebugLog(Object _object, object _message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        UnityEngine.Debug.Log(_message, _object);
#endif
    }
    public void ErrorLog(Object _object, object _message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        UnityEngine.Debug.LogError(_message, _object);
#endif
    }
    public void WarningLog(Object _object, object _message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        UnityEngine.Debug.LogWarning(_message, _object);
#endif
    }
    #endregion
}
