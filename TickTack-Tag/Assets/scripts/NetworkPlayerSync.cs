using UnityEngine;
using System.Collections.Generic;

public class NetworkPlayerSync : MonoBehaviour
{
    private GameManager _gameManager;

    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
    }

    private void OnEnable()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.IsBotGame)
        {
            this.enabled = false;
            return;
        }

        NetworkManager.OnMoveReceived += HandleMoveReceived;
        NetworkManager.OnBombTransferReceived += HandleBombTransferReceived;
    }

    private void OnDisable()
    {
        NetworkManager.OnMoveReceived -= HandleMoveReceived;
        NetworkManager.OnBombTransferReceived -= HandleBombTransferReceived;
    }

    private void HandleMoveReceived(string userId, Vector2 position)
    {
        if (_gameManager == null || _gameManager.Entities == null || NetworkManager.Instance == null) return;

        // Identifiquem quina entitat hem de moure basant-nos en el ID
        int targetIndex = -1;
        var data = NetworkManager.Instance.CurrentGameData;
        if (data == null) return;

        if (userId == data.player1.ToString()) targetIndex = 0;
        else if (data.player2.HasValue && userId == data.player2.Value.ToString()) targetIndex = 1;

        if (targetIndex != -1 && targetIndex < _gameManager.Entities.Length)
        {
            GameObject entity = _gameManager.Entities[targetIndex];
            if (entity != null && userId != NetworkManager.Instance.UserId)
            {
                // Moure l'entitat remota
                entity.transform.position = Vector3.Lerp(entity.transform.position, new Vector3(position.x, position.y, 0), Time.deltaTime * 15f);
            }
        }
    }

    private void HandleBombTransferReceived(string newOwnerName)
    {
        Debug.Log($"[Sync] Transferència de bomba rebuda per xarxa: {newOwnerName}");
        
        Bomb bomb = FindFirstObjectByType<Bomb>();
        if (bomb == null) return;

        foreach (var entity in _gameManager.Entities)
        {
            if (entity != null && entity.name == newOwnerName)
            {
                bomb.TransferTo(entity);
                break;
            }
        }
    }
}
