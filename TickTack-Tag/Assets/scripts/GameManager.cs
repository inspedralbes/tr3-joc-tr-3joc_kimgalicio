using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameStateSO GameState;
    public GameObject[] Entities; // Jugadors i Bots
    public Transform[] SpawnPoints;

    [Header("Bomba Config")]
    [SerializeField] private bool forceBotStart = true; // Si es true, el Bot empieza siempre la partida con la bomba

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

        // Assignar bomba (Forçat al Bot o aleatori segons config)
        AssignStartingBomb();
        
        Debug.Log("Nova Ronda Començada!");
    }

    void Update()
    {
        if (GameState == null) return;

        // Reiniciar joc amb la tecla R si ha acabat
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
        
        // Activar animación de daño si es un jugador
        var controller = deadEntity.GetComponent<PlayerModeController2D>();
        if (controller != null) controller.TriggerDamage();

        GameState.SubtractLife(deadEntity.name);

        // Comprovar si algú s'ha quedat a 0 vides (Fi del joc total)
        if (GameState.GetLives(deadEntity.name) <= 0)
        {
            EndGame(deadEntity.name);
            return;
        }

        // Lògica de rondes segons nombre de jugadors
        if (Entities.Length == 2)
        {
            StartCoroutine(RoundResetCoroutine());
        }
        else if (Entities.Length >= 3)
        {
            // Sistema d'espectador
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
        Debug.Log($"JOC ACABAT! Guanyador: {winnerName}, Perdedor: {loserName}");
    }

    private IEnumerator RoundResetCoroutine()
    {
        _isTransitioning = true;
        yield return new WaitForSeconds(1f);
        StartRound();
    }

    private void SetSpectator(GameObject entity)
    {
        GameState.Spectators.Add(entity.name);
        entity.GetComponent<SpriteRenderer>().enabled = false;
        entity.GetComponent<Collider2D>().enabled = false;
        var rb = entity.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
        Debug.Log($"{entity.name} ara és espectador.");
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
                
                // Buscamos al Bot por nombre o por Tag
                if (entity.name.ToLower().Contains("bot") || entity.CompareTag("Bot"))
                {
                    botEntity = entity;
                }
            }
        }

        if (candidates.Count > 0)
        {
            GameObject newOwner = null;

            // Si forzamos al Bot y existe, se la damos a él. Si no, aleatorio.
            if (forceBotStart && botEntity != null)
            {
                newOwner = botEntity;
            }
            else
            {
                newOwner = candidates[Random.Range(0, candidates.Count)];
            }

            Bomb bomb = FindFirstObjectByType<Bomb>();
            if (bomb != null && newOwner != null)
            {
                bomb.TransferTo(newOwner);
                Debug.Log($"Bomba asignada a: {newOwner.name}");
            }
        }
    }
}