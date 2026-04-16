using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameStateSO GameState;
    public GameObject[] Entities; // Jugadors i Bots
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
        GameState.ResetRun();
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

        // Respawn i reactivar a tothom
        for (int i = 0; i < Entities.Length; i++)
        {
            if (Entities[i] != null) ResetEntity(Entities[i], i);
        }

        // --- CAMBIO CLAVE: Esperamos un frame para asegurar que la Bomba está lista ---
        StartCoroutine(DelayedInitialBombAssignment());
        
        Debug.Log("Nova Ronda Començada!");
    }

    private IEnumerator DelayedInitialBombAssignment()
    {
        // Esperamos al final del frame para que todos los Start() se hayan ejecutado
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
        foreach (var entity in Entities)
        {
            int lives = GameState.GetLives(entity.name);
            if (lives > maxLives && entity.name != loserName)
            {
                maxLives = lives;
                winnerName = entity.name;
            }
        }

        GameState.SetGameOver(winnerName, loserName);
    }

    private IEnumerator RoundResetCoroutine()
    {
        _isTransitioning = true;
        yield return new WaitForSeconds(1.5f); // Un poco más de tiempo para ver la explosión
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
                : candidates[Random.Range(0, candidates.Count)];

            Bomb bomb = FindFirstObjectByType<Bomb>();
            if (bomb != null && newOwner != null)
            {
                bomb.TransferTo(newOwner);
            }
        }
    }
}