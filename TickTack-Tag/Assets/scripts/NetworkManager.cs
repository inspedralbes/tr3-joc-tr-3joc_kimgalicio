using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;

/// <summary>
/// Gestiona la comunicació amb el backend (HTTP i WebSockets).
/// Implementat com a Singleton que sobreviu entre escenes.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [Header("Configuració de Xarxa")]
    [SerializeField] private string httpUrl = "http://localhost:3000";
    [SerializeField] private string wsUrl = "ws://localhost:3000";

    // Dades de sessió
    public string UserId { get; private set; }
    public string GameId { get; private set; }

    private WebSocket _websocket;

    // Esdeveniments per a altres scripts
    public static event Action<string, Vector2> OnMoveReceived;
    public static event Action<string> OnBombTransferReceived;
    public static event Action<int, string> OnExplosionReceived;
    public static event Action OnConnected;

    private void Awake()
    {
        // Patró Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Part HTTP (UnityWebRequest)

    /// <summary>
    /// Fa el login de l'usuari amb un nickname.
    /// </summary>
    public void LoginUser(string nickname, Action<bool> callback = null)
    {
        StartCoroutine(PostLogin(nickname, callback));
    }

    private IEnumerator PostLogin(string nickname, Action<bool> callback)
    {
        LoginRequest data = new LoginRequest { nickname = nickname };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest($"{httpUrl}/api/users/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                UserId = response.userId;
                Debug.Log($"[NetworkManager] Login correcte. UserId: {UserId}");
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError($"[NetworkManager] Error en el Login: {request.error}");
                callback?.Invoke(false);
            }
        }
    }

    /// <summary>
    /// S'uneix a una partida segons el mode ('vs_bot' o 'vs_player').
    /// </summary>
    public void JoinGame(string mode, Action<bool> callback = null)
    {
        StartCoroutine(PostJoin(mode, callback));
    }

    private IEnumerator PostJoin(string mode, Action<bool> callback)
    {
        JoinRequest data = new JoinRequest { userId = UserId, mode = mode };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest($"{httpUrl}/api/games/join", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                JoinResponse response = JsonUtility.FromJson<JoinResponse>(request.downloadHandler.text);
                GameId = response.gameId;
                Debug.Log($"[NetworkManager] Unió a la partida correcta. GameId: {GameId}");
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError($"[NetworkManager] Error en unir-se a la partida: {request.error}");
                callback?.Invoke(false);
            }
        }
    }

    #endregion

    #region Part WebSocket (NativeWebSocket)

    /// <summary>
    /// Connecta al servidor de WebSockets.
    /// </summary>
    public async void ConnectToGame()
    {
        _websocket = new WebSocket(wsUrl);

        _websocket.OnOpen += () =>
        {
            Debug.Log("[NetworkManager] Connexió WebSocket oberta.");
            SendJoinAction();
            OnConnected?.Invoke();
        };

        _websocket.OnError += (e) =>
        {
            Debug.LogError($"[NetworkManager] Error WebSocket: {e}");
        };

        _websocket.OnClose += (e) =>
        {
            Debug.Log("[NetworkManager] Connexió WebSocket tancada.");
        };

        _websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            HandleWsMessage(message);
        };

        // Iniciem la connexió
        await _websocket.Connect();
    }

    private void HandleWsMessage(string json)
    {
        // Primer llegim l'acció per saber com parsejar la resta
        WsBaseMessage baseMsg = JsonUtility.FromJson<WsBaseMessage>(json);

        switch (baseMsg.action)
        {
            case "move":
                WsMoveMessage moveMsg = JsonUtility.FromJson<WsMoveMessage>(json);
                // Només avisem si el moviment NO és el nostre
                if (moveMsg.userId != UserId) {
                    OnMoveReceived?.Invoke(moveMsg.userId, ...);
                }
                break;

            case "bomb_transfer":
                WsBombTransferMessage bombMsg = JsonUtility.FromJson<WsBombTransferMessage>(json);
                OnBombTransferReceived?.Invoke(bombMsg.newOwnerId);
                break;

            case "explosion":
                WsExplosionMessage explosionMsg = JsonUtility.FromJson<WsExplosionMessage>(json);
                OnExplosionReceived?.Invoke(explosionMsg.livesLeft, explosionMsg.loserId);
                break;

            default:
                Debug.LogWarning($"[NetworkManager] Acció desconeguda rebuda: {baseMsg.action}");
                break;
        }
    }

    private async void SendJoinAction()
    {
        WsJoinAction join = new WsJoinAction
        {
            action = "join",
            gameId = GameId,
            userId = UserId
        };
        await _websocket.SendText(JsonUtility.ToJson(join));
    }

    public async void SendMove(Vector2 position)
    {
        if (_websocket.State != WebSocketState.Open) return;

        WsMoveMessage msg = new WsMoveMessage
        {
            action = "move",
            userId = UserId,
            position = new Vector2DTO { x = position.x, y = position.y }
        };
        await _websocket.SendText(JsonUtility.ToJson(msg));
    }

    public async void SendBombTransfer(string newOwnerId)
    {
        if (_websocket.State != WebSocketState.Open) return;

        WsBombTransferMessage msg = new WsBombTransferMessage
        {
            action = "bomb_transfer",
            newOwnerId = newOwnerId
        };
        await _websocket.SendText(JsonUtility.ToJson(msg));
    }

    public async void SendExplosion(int livesLeft, string loserId)
    {
        if (_websocket.State != WebSocketState.Open) return;

        WsExplosionMessage msg = new WsExplosionMessage
        {
            action = "explosion",
            livesLeft = livesLeft,
            loserId = loserId
        };
        await _websocket.SendText(JsonUtility.ToJson(msg));
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _websocket?.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        if (_websocket != null)
        {
            await _websocket.Close();
        }
    }

    #endregion
}

#region Estructures de Dades (DTOs)

[Serializable]
public class LoginRequest
{
    public string nickname;
}

[Serializable]
public class LoginResponse
{
    public string userId;
}

[Serializable]
public class JoinRequest
{
    public string userId;
    public string mode;
}

[Serializable]
public class JoinResponse
{
    public string gameId;
}

[Serializable]
public class WsBaseMessage
{
    public string action;
}

[Serializable]
public class WsJoinAction : WsBaseMessage
{
    public string gameId;
    public string userId;
}

[Serializable]
public class WsMoveMessage : WsBaseMessage
{
    public string userId;
    public Vector2DTO position;
}

[Serializable]
public class WsBombTransferMessage : WsBaseMessage
{
    public string newOwnerId;
}

[Serializable]
public class WsExplosionMessage : WsBaseMessage
{
    public int livesLeft;
    public string loserId;
}

[Serializable]
public class Vector2DTO
{
    public float x;
    public float y;
}

#endregion
