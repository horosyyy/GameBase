using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonGenerator : SingletonMonoBehaviour<SingletonGenerator>
{
    protected override bool dontDestroyOnLoad { get { return true; } }

    [SerializeField]
    List<GameObject> SingletonManager = null;

    protected override void Awake()
    {
        base.Awake();
        foreach (var singleton in SingletonManager)
        {
            Instantiate(singleton);
        }
    }
}
