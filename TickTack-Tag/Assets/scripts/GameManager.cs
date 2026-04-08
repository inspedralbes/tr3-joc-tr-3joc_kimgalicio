using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameStateSO GameState;
    public GameObject[] Entities; // Jugadors i Bots
    public Transform[] SpawnPoints;

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

        // Assignar bomba aleatòriament a algú viu
        AssignRandomBomb();
        
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
        
        // La bomba explota automàticament des de Bomb.cs quan el timer arriba a 0,
        // cridant a HandleDeath()
    }

    public void HandleDeath(GameObject deadEntity)
    {
        if (_isTransitioning || GameState.GameOver) return;

        Debug.Log($"{deadEntity.name} ha mort!");
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
        else if (Entities.Length == 3)
        {
            // Sistema d'espectador
            if (GameState.Spectators.Count == 0)
            {
                // Primer mort de la ronda -> Espectador
                SetSpectator(deadEntity);
                
                // Si la bomba era del mort, passar-la a un altre
                if (GameState.CurrentBombOwner == deadEntity)
                {
                    AssignRandomBomb();
                }
            }
            else
            {
                // Segon mort de la ronda -> Reset ronda
                StartCoroutine(RoundResetCoroutine());
            }
        }
    }

    private void EndGame(string loserName)
    {
        _isTransitioning = true;
        // El guanyador és el que té més vides o l'últim que queda (simplificat)
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
        
        // Desactivar visualment i físicament
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
        
        // Reset multiplicador de velocitat si existeix
        var p = entity.GetComponent<PlayerModeController2D>();
        if (p != null) p.ResetSpeedMultiplier();
    }

    private void AssignRandomBomb()
    {
        List<GameObject> candidates = new List<GameObject>();
        foreach (var entity in Entities)
        {
            if (!GameState.Spectators.Contains(entity.name))
            {
                candidates.Add(entity);
            }
        }

        if (candidates.Count > 0)
        {
            GameObject newOwner = candidates[Random.Range(0, candidates.Count)];
            if (GameBomb != null) GameBomb.TransferTo(newOwner);
        }
    }
}
            GameObject newOwner = candidates[Random.Range(0, candidates.Count)];
            FindObjectOfType<Bomb>().TransferTo(newOwner);
        }
    }
}
