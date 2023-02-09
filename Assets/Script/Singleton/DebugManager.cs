using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �f�o�b�O�p�}�l�[�W���[
/// </summary>
public static class DebugLogSystem
{ 
    #region ���O�֌W
    public static void DebugLog(Object _object, object _message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        UnityEngine.Debug.Log(_message, _object);
#endif
    }
    public static void ErrorLog(Object _object, object _message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        UnityEngine.Debug.LogError(_message, _object);
#endif
    }
    public static void WarningLog(Object _object, object _message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        UnityEngine.Debug.LogWarning(_message, _object);
#endif
    }
    #endregion
}
