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
        // En una partida multijugador real de este proyecto:
        // - Tú eres un objeto (Player1 o Player2)
        // - El otro jugador es el otro objeto en la lista Entities del GameManager.
        
        if (_gameManager == null || _gameManager.Entities == null) return;

        foreach (var entity in _gameManager.Entities)
        {
            if (entity == null) continue;

            // Buscamos al jugador remoto. 
            // Si el objeto NO es controlado por el jugador local (useAiInput o similar), lo movemos.
            var controller = entity.GetComponent<PlayerModeController2D>();
            if (controller != null && (controller.useAiInput || entity.name.Contains("Player")))
            {
                // Estrategia simple: si el nombre del objeto NO es el nuestro, lo movemos.
                // En este proyecto, los IDs se suelen asignar secuencialmente o por nombre.
                // Por ahora, moveremos cualquier entidad que no sea la local y coincida con el patrón.
                if (NetworkManager.Instance != null && userId != NetworkManager.Instance.UserId)
                {
                    // Aplicamos interpolación simple o posición directa
                    entity.transform.position = Vector3.Lerp(entity.transform.position, new Vector3(position.x, position.y, 0), Time.deltaTime * 15f);
                }
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
