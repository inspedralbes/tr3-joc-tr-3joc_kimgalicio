using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Game/Game State")]
public class GameStateSO : ScriptableObject
{
    public int MaxLives = 5;

    [NonSerialized] public int Score;
    [NonSerialized] public int Lives;
    [NonSerialized] public bool GameOver;

    public event Action OnChanged;

    public void ResetRun()
    {
        Score = 0;
        Lives = MaxLives;
        GameOver = false;
        OnChanged?.Invoke();
    }

    public void AddScore(int amount)
    {
        if (GameOver) return;
        Score += amount;
        OnChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (GameOver) return;
        Lives = Mathf.Max(0, Lives - amount);
        if (Lives == 0) GameOver = true;
        OnChanged?.Invoke();
    }
}
