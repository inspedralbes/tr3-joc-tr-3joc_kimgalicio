using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Game State")]
public class GameStateSO : ScriptableObject
{
    [Header("Configuració")]
    public float InitialTimer = 30f;
    public int MaxLives = 3;

    [Header("Estat de la Partida (Lectura en tiempo real)")]

    public float GameTimer;
    public int CurrentRound = 1;
    public GameObject CurrentBombOwner;
    public bool GameOver;

    [Header("Resultats")]
    public string WinnerName;
    public string LoserName;

    [NonSerialized] public Dictionary<string, int> EntityLives = new Dictionary<string, int>();
    [NonSerialized] public HashSet<string> Spectators = new HashSet<string>();

    public event Action OnChanged;

    public void ResetRun()
    {
        GameTimer = InitialTimer;
        CurrentRound = 1;
        CurrentBombOwner = null;
        GameOver = false;
        WinnerName = "";
        LoserName = "";
        EntityLives.Clear();
        Spectators.Clear();
        OnChanged?.Invoke();
    }

    public void SetGameOver(string winner, string loser)
    {
        GameOver = true;
        WinnerName = winner;
        LoserName = loser;
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

    public void InitializeEntity(string entityName)
    {

        if (EntityLives == null) EntityLives = new Dictionary<string, int>();

        if (!EntityLives.ContainsKey(entityName))
        {
            EntityLives[entityName] = MaxLives;
            OnChanged?.Invoke();
        }
    }

    public int GetLives(string entityName)
    {
        if (EntityLives == null || !EntityLives.ContainsKey(entityName)) return 0;
        return EntityLives[entityName];
    }

    public void SubtractLife(string entityName)
    {
        if (EntityLives != null && EntityLives.ContainsKey(entityName))
        {
            EntityLives[entityName] = Mathf.Max(0, EntityLives[entityName] - 1);
            OnChanged?.Invoke();
        }
    }
}
