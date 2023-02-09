using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MainGameState
{
    Title,
    InGame,
}

public class MainGameStateManager : GameStateManagerBase<MainGameStateManager>
{
    protected override void Awake()
    {
        base.Awake();

        DebugLogSystem.DebugLog(this, "generate MainGameStateManager");
    }

    public void AddState(MainGameState _state, Action _enterAction, Action _loopAction, Action _endAction)
    {
        base.AddState(_state.ToString(), _enterAction, _loopAction, _endAction);
    }

    public void ChangeState(MainGameState _state)
    {
        base.ChangeState(_state.ToString());
    }
}
