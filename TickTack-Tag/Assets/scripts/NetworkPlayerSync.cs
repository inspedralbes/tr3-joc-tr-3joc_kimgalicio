using UnityEngine;
using System.Collections.Generic;

public class NetworkPlayerSync : MonoBehaviour
{
    private GameManager _gameManager;

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
    }

    private Vector3[] _targetPositions = new Vector3[2];
    private bool[] _hasTarget = new bool[2];

    private void OnEnable()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.IsBotGame)
        {
            this.enabled = false;
            return;
        }

        NetworkManager.OnMoveReceived += HandleMoveReceived;
        NetworkManager.OnBombTransferReceived += HandleBombTransferReceived;
        NetworkManager.OnExplosionReceived += HandleExplosionReceived;
    }

    private void OnDisable()
    {
        NetworkManager.OnMoveReceived -= HandleMoveReceived;
        NetworkManager.OnBombTransferReceived -= HandleBombTransferReceived;
        NetworkManager.OnExplosionReceived -= HandleExplosionReceived;
    }

    private void Update()
    {
        // Aplicar suavitat de moviment a les entitats remotes
        if (_gameManager == null || _gameManager.Entities == null) return;

        for (int i = 0; i < _gameManager.Entities.Length; i++)
        {
            if (_hasTarget[i])
            {
                GameObject entity = _gameManager.Entities[i];
                if (entity != null)
                {
                    entity.transform.position = Vector3.Lerp(entity.transform.position, _targetPositions[i], Time.deltaTime * 15f);
                    
                    // Si ja estem molt a prop, marquem com a completat (opcional)
                    if (Vector3.Distance(entity.transform.position, _targetPositions[i]) < 0.001f)
                    {
                        entity.transform.position = _targetPositions[i];
                        _hasTarget[i] = false;
                    }
                }
            }
        }
    }

    private void HandleMoveReceived(string userId, Vector2 position)
    {
        if (_gameManager == null || NetworkManager.Instance == null) return;

        int targetIndex = GetTargetIndex(userId);
        if (targetIndex != -1 && targetIndex < _gameManager.Entities.Length)
        {
            // Només actualitzem si NO som nosaltres
            if (userId != NetworkManager.Instance.UserId)
            {
                _targetPositions[targetIndex] = new Vector3(position.x, position.y, 0);
                _hasTarget[targetIndex] = true;
            }
        }
        else
        {
            Debug.LogWarning($"[Sync] Moviment rebut per a UserId {userId} però no s'ha trobat cap entitat corresponent. Està el Jugador 2 sincronitzat?");
        }
    }

    private void HandleBombTransferReceived(string newOwnerId)
    {
        Debug.Log($"[Sync] Transferència de bomba rebuda per xarxa per UserId: {newOwnerId}");
        
        if (_gameManager == null) return;
        GameObject newOwner = _gameManager.GetEntityById(newOwnerId);
        
        Bomb bomb = FindFirstObjectByType<Bomb>();
        if (bomb != null && newOwner != null)
        {
            bomb.TransferTo(newOwner);
        }
    }

    private void HandleExplosionReceived(int livesLeft, string loserId)
    {
        Debug.Log($"[Sync] Explosió rebuda per xarxa. Jugador {loserId} perd vida. Li queden {livesLeft}");

        if (_gameManager == null) return;
        GameObject loser = _gameManager.GetEntityById(loserId);

        if (loser != null && _gameManager.GameState != null)
        {
            // Sincronitzem la quantitat exacta de vides en lloc de restar-ne una altra
            _gameManager.GameState.SetLives(loser.name, livesLeft);
        }
    }

    private int GetTargetIndex(string userId)
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.CurrentGameData == null) return -1;
        var data = NetworkManager.Instance.CurrentGameData;
        if (userId == data.player1.ToString()) return 0;
        if (data.player2 != 0 && userId == data.player2.ToString()) return 1;
        
        if (data.player2 == 0 && userId != data.player1.ToString())
        {
            int parsedId;
            if (int.TryParse(userId, out parsedId))
            {
                data.player2 = parsedId;
                Debug.Log($"[Sync] Fallback: Assigned userId {userId} as player2.");
                return 1;
            }
        }
        
        return -1;
    }
}
