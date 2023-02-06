using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StateData
{
    public int Value;
    public string Name;

    public Action FirstAction;
    public Action LoopAction;
    public Action EndAction;
}

public class GameStateManagerBase<T> : SingletonMonoBehaviour<T> where T : MonoBehaviour
{
    protected override bool dontDestroyOnLoad { get { return true; } }

    private int StateNum = 0;

    [SerializeField]
    private List<StateData> mStateList = new List<StateData>();

    [SerializeField]
    private StateData mNowState = null;

    protected StateData mPreState = null;

    private bool IsNowChange = false;
    public void Update()
    {
        if(mNowState != null && !IsNowChange)
        {
            if (mNowState.LoopAction != null)
            {
                mNowState.LoopAction();
            }
        }
        else if(IsNowChange)
        {
            IsNowChange = false;
        }
    }

    public void AddState(string _name, Action _firstAction, Action _loopAction, Action _endAction)
    {
        StateData state = new StateData()
        {
            Value = StateNum,
            Name = _name,

            FirstAction = _firstAction,
            LoopAction = _loopAction,
            EndAction = _endAction,
        };
        StateNum++;

        mStateList.Add(state);
    }

    public void ChangeState(string _name)
    {
        if (mNowState != null && mNowState.EndAction != null)
        {
            mNowState.EndAction();
        }
        var state = SearchState(_name);
        if(state == null)
        {
            Debug.LogError(_name + "は存在しないステートです。");
            return;
        }
        mPreState = mNowState;
        mNowState = state;
        IsNowChange = true;
        if (mNowState != null && mNowState.FirstAction != null)
        {
            mNowState.FirstAction();
        }
    }

    private StateData SearchState(string _name)
    {
        foreach (var state in mStateList)
        {
            if (state.Name == _name)
            {
                return state;
            }
        }
        return null;
    }
}
