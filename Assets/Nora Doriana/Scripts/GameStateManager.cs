using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    #region Events
    public static event Action<GameState> OnGameStateChanged;
    #endregion

    #region Fields
    public static GameState CurrentState { get; private set; }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        ChangeState(GameState.Loading);
    }
    #endregion

    #region Public Methods
    public static void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
        {
            return;
        }

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
    #endregion
}