using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static event Action<int, int> OnLocalGameOver;
    public GameStateSO GameState;
    public GameObject[] Entities;
    public Transform[] SpawnPoints;

    [Header("Bomba Config")]
    [SerializeField] private bool forceBotStart = true;

    private bool _isTransitioning = false;

    void Start()
    {
        if (GameState == null) return;
        InitializeGame();
    }

    void InitializeGame()
    {
        if (GameState != null) GameState.ResetRun();
        
        foreach (var entity in Entities)
        {
            if (entity != null) GameState.InitializeEntity(entity.name);
        }
        StartRound();
    }

    void StartRound()
    {
        _isTransitioning = false;
        GameState.GameTimer = GameState.InitialTimer;
        GameState.Spectators.Clear();

        for (int i = 0; i < Entities.Length; i++)
        {
            if (Entities[i] != null) ResetEntity(Entities[i], i);
        }

        StartCoroutine(DelayedInitialBombAssignment());

        Debug.Log("Nova Ronda Començada!");
    }

    private IEnumerator DelayedInitialBombAssignment()
    {

        yield return new WaitForEndOfFrame();
        AssignStartingBomb();
    }

    void Update()
    {
        if (GameState == null) return;

        if (GameState.GameOver && Input.GetKeyDown(KeyCode.R))
        {
            InitializeGame();
            return;
        }

        if (GameState.GameOver || _isTransitioning) return;

        GameState.UpdateTimer(Time.deltaTime);
    }

    public void HandleDeath(GameObject deadEntity)
    {
        if (_isTransitioning || GameState.GameOver) return;

        Debug.Log($"{deadEntity.name} ha mort!");

        var controller = deadEntity.GetComponent<PlayerModeController2D>();
        if (controller != null) controller.TriggerDamage();

        GameState.SubtractLife(deadEntity.name);

        if (GameState.GetLives(deadEntity.name) <= 0)
        {
            EndGame(deadEntity.name);
            return;
        }

        if (Entities.Length == 2)
        {
            StartCoroutine(RoundResetCoroutine());
        }
        else if (Entities.Length >= 3)
        {
            if (GameState.Spectators.Count < Entities.Length - 2)
            {
                SetSpectator(deadEntity);

                if (GameState.CurrentBombOwner == deadEntity)
                {
                    AssignStartingBomb();
                }
            }
            else
            {
                StartCoroutine(RoundResetCoroutine());
            }
        }
    }

    private void EndGame(string loserName)
    {
        _isTransitioning = true;
        string winnerName = "Ningú";
        int maxLives = -1;
        GameObject winnerEntity = null;

        foreach (var entity in Entities)
        {
            int lives = GameState.GetLives(entity.name);
            if (lives > maxLives && entity.name != loserName)
            {
                maxLives = lives;
                winnerName = entity.name;
                winnerEntity = entity;
            }
        }

        GameState.SetGameOver(winnerName, loserName);

        int winnerId = 0;
        int winnerHearts = maxLives;

        if (winnerEntity != null)
        {
            if (winnerEntity.name.ToLower().Contains("bot") || winnerEntity.CompareTag("Bot"))
            {
                winnerId = 0;
            }
            else
            {

                if (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.UserId))
                    winnerId = int.Parse(NetworkManager.Instance.UserId);
                else
                    winnerId = 1;
            }
        }

        Debug.Log($"[GameManager] Disparant OnLocalGameOver. Guanyador: {winnerId}, Cors: {winnerHearts}");
        OnLocalGameOver?.Invoke(winnerId, winnerHearts);

        if (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.GameId))
        {
            NetworkManager.Instance.FinishGame(winnerId, winnerHearts);
        }
    }

    private IEnumerator RoundResetCoroutine()
    {
        _isTransitioning = true;
        
        // Esperem una mica després de l'explosió
        yield return new WaitForSeconds(1.0f);

        if (HUDController.Instance != null)
        {
            bool countdownFinished = false;
            HUDController.Instance.ShowCountdown(() => {
                countdownFinished = true;
            });

            // Esperem que el compte enrere acabi
            yield return new WaitUntil(() => countdownFinished);
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
        }

        StartRound();
    }

    private void SetSpectator(GameObject entity)
    {
        GameState.Spectators.Add(entity.name);
        entity.GetComponent<SpriteRenderer>().enabled = false;
        entity.GetComponent<Collider2D>().enabled = false;
        var rb = entity.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
    }

    private void ResetEntity(GameObject entity, int index)
    {
        entity.GetComponent<SpriteRenderer>().enabled = true;
        entity.GetComponent<Collider2D>().enabled = true;
        var rb = entity.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }

        if (SpawnPoints != null && index < SpawnPoints.Length)
        {
            entity.transform.position = SpawnPoints[index].position;
        }

        var p = entity.GetComponent<PlayerModeController2D>();
        if (p != null) p.ResetSpeedMultiplier();

        var b = entity.GetComponent<BotController>();
        if (b != null) b.ResetSpeedMultiplier();
    }

    private void AssignStartingBomb()
    {
        List<GameObject> candidates = new List<GameObject>();
        GameObject botEntity = null;

        foreach (var entity in Entities)
        {
            if (entity == null) continue;

            if (!GameState.Spectators.Contains(entity.name))
            {
                candidates.Add(entity);
                if (entity.name.ToLower().Contains("bot") || entity.CompareTag("Bot"))
                {
                    botEntity = entity;
                }
            }
        }

        if (candidates.Count > 0)
        {
            GameObject newOwner = (forceBotStart && botEntity != null)
                ? botEntity
                : candidates[UnityEngine.Random.Range(0, candidates.Count)];

            Bomb bomb = FindFirstObjectByType<Bomb>();
            if (bomb != null && newOwner != null)
            {
                bomb.TransferTo(newOwner);
            }
        }
    }
}
