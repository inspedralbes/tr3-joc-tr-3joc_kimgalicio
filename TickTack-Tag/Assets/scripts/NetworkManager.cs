using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [Header("Configuració de Xarxa")]
    [SerializeField] private string httpUrl = "http://localhost:3000";
    [SerializeField] private string wsUrl = "ws://localhost:3000";

    public string UserId { get; private set; }
    public string GameId { get; private set; }
    public string PlayerNickname { get; private set; }

    private WebSocket _websocket;

    public static event Action<string, Vector2> OnMoveReceived;
    public static event Action<string> OnBombTransferReceived;
    public static event Action<int, string> OnExplosionReceived;
    public static event Action<WsGameOverMessage> OnGameOverReceived;
    public static event Action OnConnected;

    private void Awake()
    {

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
                UserId = response.userId.ToString();
                PlayerNickname = nickname;
                Debug.Log($"[NetworkManager] Login correcte. UserId: {UserId}, Nickname: {PlayerNickname}");
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError($"[NetworkManager] Error en el Login: {request.error}");
                callback?.Invoke(false);
            }
        }
    }

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
            Debug.Log($"[NetworkManager] Enviant Join JSON: {json}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                JoinResponse response = JsonUtility.FromJson<JoinResponse>(request.downloadHandler.text);
                GameId = response.partida.id.ToString();
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

    public void FinishGame(int winnerId, int winnerHearts, Action<bool> callback = null)
    {
        StartCoroutine(PostFinish(winnerId, winnerHearts, callback));
    }

    private IEnumerator PostFinish(int winnerId, int winnerHearts, Action<bool> callback)
    {
        FinishRequest data = new FinishRequest {
            gameId = int.Parse(GameId),
            winnerId = winnerId,
            winnerHearts = winnerHearts
        };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest($"{httpUrl}/api/games/finish", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[NetworkManager] Partida finalitzada correctament al backend.");
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogError($"[NetworkManager] Error en finalitzar partida: {request.error}");
                callback?.Invoke(false);
            }
        }
    }

    #endregion

    #region Part WebSocket (NativeWebSocket)

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

        await _websocket.Connect();
    }

    private void HandleWsMessage(string json)
    {

        WsBaseMessage baseMsg = JsonUtility.FromJson<WsBaseMessage>(json);

        switch (baseMsg.action)
        {
            case "move":
                WsMoveMessage moveMsg = JsonUtility.FromJson<WsMoveMessage>(json);
                if (moveMsg.userId != UserId) {
                    OnMoveReceived?.Invoke(moveMsg.userId, new Vector2(moveMsg.position.x, moveMsg.position.y));
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

            case "game_over":
                WsGameOverMessage gameOverMsg = JsonUtility.FromJson<WsGameOverMessage>(json);
                OnGameOverReceived?.Invoke(gameOverMsg);
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
    public int userId;
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
    public string missatge;
    public PartidaDTO partida;
}

[Serializable]
public class PartidaDTO
{
    public int id;
    public string mode;
    public int player1;
    public int? player2;
    public string status;
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
public class WsGameOverMessage : WsBaseMessage
{
    public int winnerId;
    public int winnerHearts;
    public int loserId;
}

[Serializable]
public class FinishRequest
{
    public int gameId;
    public int winnerId;
    public int winnerHearts;
}

[Serializable]
public class Vector2DTO
{
    public float x;
    public float y;
}

#endregion
