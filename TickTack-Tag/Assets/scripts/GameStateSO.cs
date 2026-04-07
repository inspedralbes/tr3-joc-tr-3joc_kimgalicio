using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Game State")]
public class GameStateSO : ScriptableObject
{
    [Header("Configuració")]
    public float InitialTimer = 90f;
    public int MaxLives = 3;

    [Header("Estat de la Partida")]
    [NonSerialized] public float GameTimer;
    [NonSerialized] public GameObject CurrentBombOwner;
    [NonSerialized] public bool GameOver;
    [NonSerialized] public string WinnerName;
    [NonSerialized] public string LoserName;
    
    // Diccionari per rastrejar les vides de cada entitat per nom o ID
    [NonSerialized] public Dictionary<string, int> EntityLives = new Dictionary<string, int>();
    // Llista d'entitats que estan en mode espectador (vives però fora de la ronda actual)
    [NonSerialized] public HashSet<string> Spectators = new HashSet<string>();

    public event Action OnChanged;

    public void ResetRun()
    {
        GameTimer = InitialTimer;
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
        if (!EntityLives.ContainsKey(entityName))
        {
            EntityLives[entityName] = MaxLives;
        }
    }

    public int GetLives(string entityName)
    {
        return EntityLives.ContainsKey(entityName) ? EntityLives[entityName] : 0;
    }

    public void SubtractLife(string entityName)
    {
        if (EntityLives.ContainsKey(entityName))
        {
            EntityLives[entityName] = Mathf.Max(0, EntityLives[entityName] - 1);
            OnChanged?.Invoke();
        }
    }
}
