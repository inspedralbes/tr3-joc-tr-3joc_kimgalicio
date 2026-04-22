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

    [Header("Maps & Spawns")]
    [SerializeField] private GameObject Map_Bot;
    [SerializeField] private GameObject Map_Player;
    [SerializeField] private Transform[] BotSpawnPoints;
    [SerializeField] private Transform[] PlayerSpawnPoints;

    [Header("Bomba Config")]
    [SerializeField] private bool forceBotStart = true;

    [Header("Co-op Camera Settings")]
    [SerializeField] private bool useCoopCamera = true;
    [SerializeField] private float cameraSmoothTime = 0.3f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 12f;
    [SerializeField] private float zoomLimiter = 40f;

    private Camera _mainCamera;
    private Vector3 _cameraVelocity;
    private bool _isTransitioning = false;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    void Start()
    {
        if (GameState == null) return;
        InitializeGame();
    }

    void InitializeGame()
    {
        if (GameState != null)
        {
            if (GameState.SelectedMode == GameModeType.VsPlayer)
            {
                GameState.InitialTimer = 90f;
            }
            else
            {
                GameState.InitialTimer = 30f;
            }

            GameState.ResetRun();
            
            if (GameState.SelectedMode == GameModeType.VsBot)
            {
                if (Map_Bot != null) Map_Bot.SetActive(true);
                if (Map_Player != null) Map_Player.SetActive(false);
                SpawnPoints = BotSpawnPoints;
                maxZoom = 12f;
            }
            else
            {
                if (Map_Bot != null) Map_Bot.SetActive(false);
                if (Map_Player != null) Map_Player.SetActive(true);
                SpawnPoints = PlayerSpawnPoints;
                maxZoom = 18f;
            }
        }
        
        foreach (var entity in Entities)
        {
            if (entity != null)
            {
                if (GameState.SelectedMode == GameModeType.VsPlayer && (entity.name.ToLower().Contains("bot") || entity.CompareTag("Bot")))
                {
                    entity.SetActive(false);
                }
                else
                {
                    entity.SetActive(true);
                    GameState.InitializeEntity(entity.name);
                }
            }
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
            if (Entities[i] != null && Entities[i].activeInHierarchy) ResetEntity(Entities[i], i);
        }

        StartCoroutine(DelayedInitialBombAssignment());
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

    private void LateUpdate()
    {
        if (!useCoopCamera || _mainCamera == null || Entities == null || Entities.Length == 0) return;

        List<GameObject> targets = new List<GameObject>();
        foreach (var entity in Entities)
        {
            if (entity != null && entity.activeInHierarchy)
            {
                if (GameState != null && !GameState.Spectators.Contains(entity.name))
                {
                    targets.Add(entity);
                }
            }
        }

        if (targets.Count == 0) return;

        Bounds bounds = new Bounds(targets[0].transform.position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].transform.position);
        }

        Vector3 centerPoint = bounds.center;
        Vector3 newPosition = new Vector3(centerPoint.x, centerPoint.y, -10f);
        _mainCamera.transform.position = Vector3.SmoothDamp(_mainCamera.transform.position, newPosition, ref _cameraVelocity, cameraSmoothTime);

        float greatestDistance = bounds.size.x > bounds.size.y ? bounds.size.x : bounds.size.y;
        float newZoom = Mathf.Lerp(minZoom, maxZoom, greatestDistance / zoomLimiter);
        _mainCamera.orthographicSize = Mathf.Lerp(_mainCamera.orthographicSize, newZoom, Time.deltaTime * 2f);
    }

    public void HandleDeath(GameObject deadEntity)
    {
        if (_isTransitioning || GameState.GameOver) return;
        var controller = deadEntity.GetComponent<PlayerModeController2D>();
        if (controller != null) controller.TriggerDamage();
        GameState.SubtractLife(deadEntity.name);
        if (GameState.GetLives(deadEntity.name) <= 0)
        {
            EndGame(deadEntity.name);
            return;
        }
        StartCoroutine(RoundResetCoroutine());
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
        if (winnerEntity != null)
        {
            if (winnerEntity.name.ToLower().Contains("bot") || winnerEntity.CompareTag("Bot")) winnerId = 0;
            else winnerId = (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.UserId)) ? int.Parse(NetworkManager.Instance.UserId) : 1;
        }
        OnLocalGameOver?.Invoke(winnerId, maxLives);
        if (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.GameId)) NetworkManager.Instance.FinishGame(winnerId, maxLives);
    }

    private IEnumerator RoundResetCoroutine()
    {
        _isTransitioning = true;
        yield return new WaitForSeconds(1.0f);
        if (GameState != null) GameState.CurrentRound++;
        if (HUDController.Instance != null)
        {
            bool countdownFinished = false;
            HUDController.Instance.ShowCountdown(() => { countdownFinished = true; });
            yield return new WaitUntil(() => countdownFinished);
        }
        else yield return new WaitForSeconds(2.0f);
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
        if (SpawnPoints != null && index < SpawnPoints.Length) entity.transform.position = SpawnPoints[index].position;
        var p = entity.GetComponent<PlayerModeController2D>();
        if (p != null) p.ResetSpeedMultiplier();
    }

    private void AssignStartingBomb()
    {
        List<GameObject> candidates = new List<GameObject>();
        foreach (var entity in Entities)
        {
            if (entity != null && entity.activeInHierarchy && !GameState.Spectators.Contains(entity.name)) candidates.Add(entity);
        }
        if (candidates.Count > 0)
        {
            GameObject newOwner = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            Bomb bomb = FindFirstObjectByType<Bomb>();
            if (bomb != null && newOwner != null) bomb.TransferTo(newOwner);
        }
    }
}
