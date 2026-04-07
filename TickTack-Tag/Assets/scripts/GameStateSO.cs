using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Game/Game State")]
public class GameStateSO : ScriptableObject
{
    [Header("Game Settings")]
    public float InitialTimer = 30f;

    [Header("Runtime State")]
    [NonSerialized] public float GameTimer;
    [NonSerialized] public GameObject CurrentBombOwner;
    [NonSerialized] public bool GameOver;
    [NonSerialized] public string WinnerName;

    public event Action OnChanged;

    public void ResetRun()
    {
        GameTimer = InitialTimer;
        CurrentBombOwner = null;
        GameOver = false;
        WinnerName = "";
        OnChanged?.Invoke();
    }

    public void SetGameOver(string winner)
    {
        GameOver = true;
        WinnerName = winner;
        OnChanged?.Invoke();
    }

    public void UpdateTimer(float deltaTime)
    {
        if (GameOver) return;
        GameTimer = Mathf.Max(0, GameTimer - deltaTime);
        OnChanged?.Invoke();
    }

    public void SetBombOwner(GameObject owner)
    {
        CurrentBombOwner = owner;
        OnChanged?.Invoke();
    }
}
