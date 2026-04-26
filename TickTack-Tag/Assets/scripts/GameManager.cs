using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static event Action<int, int> OnLocalGameOver;
    public GameStateSO GameState;
    public GameObject[] Entities;
    public Transform[] SpawnPoints;

    [Header("Maps & Spawns")]
    [SerializeField] private GameObject Map_Bot;
    [SerializeField] private GameObject Map_Player;
    [SerializeField] private Transform[] BotSpawnPoints;
    [SerializeField] private Transform[] PlayerSpawnPoints;

    [Header("Co-op Camera Settings")]
    [SerializeField] private bool useCoopCamera = true;
    [SerializeField] private float cameraSmoothTime = 0.3f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 12f;
    [SerializeField] private float zoomLimiter = 40f;

    private Camera _mainCamera;
    private Vector3 _cameraVelocity;
    private bool _isTransitioning = false;
    public bool IsTransitioning => _isTransitioning;
    private bool _waitingForPlayers = false;
    public bool IsWaitingForPlayers => _waitingForPlayers;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _mainCamera = Camera.main;
        
        if (GetComponent<NetworkPlayerSync>() == null)
        {
            gameObject.AddComponent<NetworkPlayerSync>();
            Debug.Log("[GameManager] NetworkPlayerSync added automatically.");
        }
    }

    private void OnEnable()
    {
        NetworkManager.OnGameReady += HandleGameReady;
        NetworkManager.OnOpponentDisconnected += HandleOpponentDisconnected;

        // Si ja està a punt quan ens activem, processem-ho
        if (NetworkManager.Instance != null && NetworkManager.Instance.IsGameReady)
        {
            Debug.Log("[GameManager] La partida ja està a punt al activar el Manager.");
            HandleGameReady();
        }
    }

    private void OnDisable()
    {
        NetworkManager.OnGameReady -= HandleGameReady;
        NetworkManager.OnOpponentDisconnected -= HandleOpponentDisconnected;
    }

    private void HandleGameReady()
    {
        if (_waitingForPlayers)
        {
            Debug.Log("[GameManager] Partida a punt (Sincronitzat). Iniciant ronda...");
            _waitingForPlayers = false;
            if (HUDController.Instance != null) HUDController.Instance.SetWaitingForOpponent(false);
            StartCoroutine(RoundResetCoroutine(false));
        }
    }

    private bool _abandonmentOccurred = false;
    public bool AbandonmentOccurred => _abandonmentOccurred;

    private void HandleOpponentDisconnected()
    {
        Debug.LogWarning("[GameManager] L'oponent ha abandonat la partida.");
        if (_isTransitioning || GameState.GameOver) return;

        if (HUDController.Instance != null) 
        {
            HUDController.Instance.SetWaitingForOpponent(true);
        }

        // Si estàvem jugant, forcem que guanyi el jugador local
        if (!_waitingForPlayers)
        {
            _abandonmentOccurred = true;

            string myName = "";
            string loserName = "";

            if (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.UserId))
            {
                GameObject myEntity = GetEntityById(NetworkManager.Instance.UserId);
                if (myEntity != null) myName = myEntity.name;
            }

            foreach (var entity in Entities)
            {
                if (entity != null && entity.name != myName)
                {
                    loserName = entity.name;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(loserName))
            {
                EndGame(loserName);
            }
        }
    }
    void Start()
    {
        if (GameState == null) return;
        if (HUDController.Instance != null) HUDController.Instance.ShowHUD();
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

        // --- LÒGICA DE CLONAT DINÀMIC PER A MULTIJUGADOR ---
        if (GameState.SelectedMode == GameModeType.VsPlayer)
        {
            GameObject originalPlayer = null;
            GameObject botObject = null;
            GameObject remotePlayer = null;

            // Identificar jugador original y el bot
            foreach (var e in Entities)
            {
                if (e == null) continue;
                if (e.name.ToLower().Contains("bot") || e.CompareTag("Bot"))
                {
                    botObject = e;
                }
                else if (e.name.Contains("_Remote"))
                {
                    remotePlayer = e;
                }
                else
                {
                    originalPlayer = e;
                }
            }

            // Desactivar el bot en modo VsPlayer
            if (botObject != null)
            {
                botObject.SetActive(false);
            }

            // Crear el clon estrictamente si estamos solos
            if (originalPlayer != null && remotePlayer == null)
            {
                Debug.Log("[GameManager] Només s'ha trobat 1 Player. Creant clon per al Jugador 2...");
                GameObject player2 = Instantiate(originalPlayer, originalPlayer.transform.parent);
                player2.name = originalPlayer.name + "_Remote";

                // Forzar array tamaño 2 exacto: original y clon
                Entities = new GameObject[] { originalPlayer, player2 };
            }
            else if (originalPlayer != null && remotePlayer != null)
            {
                // Si ya se clonó antes, asegurarse de que el array sea solo de 2
                Entities = new GameObject[] { originalPlayer, remotePlayer };
            }
        }
        
        for (int i = 0; i < Entities.Length; i++)
        {
            var entity = Entities[i];
            if (entity == null) continue;

            if (GameState.SelectedMode == GameModeType.VsPlayer)
            {
                entity.SetActive(true);
                var controller = entity.GetComponent<PlayerModeController2D>();
                if (controller != null && NetworkManager.Instance != null)
                {
                    bool sócJugador1 = NetworkManager.Instance.IsPlayer1;
                    
                    // i=0 es el Jugador 1 (host), i=1 es el Jugador 2 (cliente)
                    bool isRemote = sócJugador1 ? (i != 0) : (i != 1);
                    controller.SetRemote(isRemote);
                    
                    Debug.Log($"[GameManager] {entity.name} (Índex {i}) configurada. Local: {!isRemote}");
                }
                GameState.InitializeEntity(entity.name);
            }
            else
            {
                // Mode Vs Bot original
                if (entity.name.ToLower().Contains("bot") || entity.CompareTag("Bot"))
                {
                    entity.SetActive(true);
                    GameState.InitializeEntity(entity.name);
                }
                else
                {
                    entity.SetActive(true);
                    var controller = entity.GetComponent<PlayerModeController2D>();
                    if (controller != null) controller.SetRemote(false);
                    GameState.InitializeEntity(entity.name);
                }
            }
        }

        if (GameState.SelectedMode == GameModeType.VsPlayer && 
            NetworkManager.Instance != null && 
            NetworkManager.Instance.CurrentGameData != null)
        {
            var data = NetworkManager.Instance.CurrentGameData;
            Debug.Log($"[GameManager] Mode Online. GameId: {data.id}, P1: {data.player1}, P2: {data.player2}. IsReady: {NetworkManager.Instance.IsGameReady}");
            
            if (NetworkManager.Instance.IsGameReady)
            {
                Debug.Log("[GameManager] La partida ja estava a punt. Iniciant immediatament.");
                _waitingForPlayers = false;
                if (HUDController.Instance != null) HUDController.Instance.SetWaitingForOpponent(false);
                StartCoroutine(RoundResetCoroutine(false));
            }
            else
            {
                _waitingForPlayers = true;
                if (HUDController.Instance != null) HUDController.Instance.SetWaitingForOpponent(true);
            }
            
            // Posem als jugadors als seus llocs inicials mentre esperen
            for (int i = 0; i < Entities.Length; i++)
            {
                if (Entities[i] != null && Entities[i].activeInHierarchy) ResetEntity(Entities[i], i);
            }
        }
        else
        {
            StartRound();
        }
    }

    void StartRound()
    {
        _isTransitioning = false;
        _waitingForPlayers = false;
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
        if (!_waitingForPlayers)
        {
            AssignStartingBomb();
        }
    }

    void Update()
    {
        if (GameState == null) return;
        if (GameState.GameOver && Input.GetKeyDown(KeyCode.R))
        {
            InitializeGame();
            return;
        }
        if (GameState.GameOver || _isTransitioning || _waitingForPlayers) return;
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

        // --- MULTIPLAYER SYNC ---
        if (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.GameId))
        {
            string loserId = GetUserIdOf(deadEntity);
            if (!string.IsNullOrEmpty(loserId))
            {
                NetworkManager.Instance.SendExplosion(GameState.GetLives(deadEntity.name), loserId);
            }
        }

        if (GameState.GetLives(deadEntity.name) <= 0)
        {
            EndGame(deadEntity.name);
            return;
        }
        StartCoroutine(RoundResetCoroutine(true));
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
            if (winnerEntity.name.ToLower().Contains("bot") || winnerEntity.CompareTag("Bot")) 
            {
                winnerId = 0;
            }
            else 
            {
                string idStr = GetUserIdOf(winnerEntity);
                if (!string.IsNullOrEmpty(idStr)) winnerId = int.Parse(idStr);
                else winnerId = 1;
            }
        }
        OnLocalGameOver?.Invoke(winnerId, maxLives);
        
        if (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.GameId)) 
        {
            bool canSendFinish = NetworkManager.Instance.IsPlayer1 || _abandonmentOccurred;
            if (canSendFinish)
            {
                NetworkManager.Instance.FinishGame(winnerId, maxLives);
            }
        }
    }

    private IEnumerator RoundResetCoroutine(bool incrementRound = true)
    {
        _isTransitioning = true;
        yield return new WaitForSeconds(1.0f);
        if (GameState != null && incrementRound) GameState.CurrentRound++;
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
            // En multijugador, usem l'ID de la partida com a llavor perquè tots els clients triïn el mateix
            if (GameState.SelectedMode == GameModeType.VsPlayer && NetworkManager.Instance != null && NetworkManager.Instance.CurrentGameData != null)
            {
                var data = NetworkManager.Instance.CurrentGameData;
                int seed = data.id + GameState.CurrentRound;
                System.Random rnd = new System.Random(seed);
                
                int chosenPlayerNum = rnd.Next(0, 2);
                string chosenUserId = (chosenPlayerNum == 0) ? data.player1.ToString() : data.player2.ToString();
                
                GameObject newOwner = GetEntityById(chosenUserId);
                if (newOwner != null)
                {
                    Bomb bomb = FindFirstObjectByType<Bomb>();
                    if (bomb != null) bomb.TransferTo(newOwner);
                    return; // Sortim perquè ja s'ha assignat
                }
            }

            int index = UnityEngine.Random.Range(0, candidates.Count);
            GameObject fallbackOwner = candidates[index];
            Bomb fallbackBomb = FindFirstObjectByType<Bomb>();
            if (fallbackBomb != null) fallbackBomb.TransferTo(fallbackOwner);
        }
    }
    public string GetUserIdOf(GameObject entity)
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.CurrentGameData == null) return "";

        for (int i = 0; i < Entities.Length; i++)
        {
            if (Entities[i] == entity)
            {
                if (i == 0) return NetworkManager.Instance.CurrentGameData.player1.ToString();
                if (i == 1 && NetworkManager.Instance.CurrentGameData.player2 != 0) 
                    return NetworkManager.Instance.CurrentGameData.player2.ToString();
            }
        }
        return "";
    }

    public GameObject GetEntityById(string userId)
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.CurrentGameData == null) return null;

        var data = NetworkManager.Instance.CurrentGameData;
        if (data == null) return null;

        if (userId == data.player1.ToString()) return Entities[0];
        if (data.player2 != 0 && userId == data.player2.ToString()) return Entities[1];

        return null;
    }
}
