using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    protected abstract bool dontDestroyOnLoad { get; }

    private static T instance;
    public static T Instance
    {
        get
        {
            if(!instance)
            {
                Type t = typeof(T);
                instance = (T)FindObjectOfType(t);
                if (!instance)
                {
                    Debug.LogError(t + " is nothing.");
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if(this != Instance)
        {
            Destroy(this);
            return;
        }
        if(dontDestroyOnLoad)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
