using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameStateSO _gameState;
    [SerializeField] private UIDocument _uiDocument;
    private TextField _nicknameInput;
    private Button _btnVsBot;
    private Button _btnVsPlayer;

    private void OnEnable()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

        if (_uiDocument == null)
        {
            Debug.LogError("[MenuController] No s'ha trobat cap component UIDocument assignat o al GameObject!");
            return;
        }

        VisualElement root = _uiDocument.rootVisualElement;

        _btnVsBot = root.Q<Button>("btn-vs-bot");
        _btnVsPlayer = root.Q<Button>("btn-vs-player");
        _nicknameInput = root.Q<TextField>("nickname-input");

        if (_btnVsBot != null)
        {
            _btnVsBot.RegisterCallback<ClickEvent>(evt => StartConnectionFlow("vs_bot"));
        }
        else
        {
            Debug.LogWarning("[MenuController] No s'ha trobat el botó 'btn-vs-bot'");
        }

        if (_btnVsPlayer != null)
        {
            _btnVsPlayer.RegisterCallback<ClickEvent>(evt => StartConnectionFlow("vs_player"));
        }
        else
        {
            Debug.LogWarning("[MenuController] No s'ha trobat el botó 'btn-vs-player'");
        }

        if (_nicknameInput == null)
        {
            Debug.LogWarning("[MenuController] No s'ha trobat el TextField 'nickname-input'");
        }
    }

    private void StartConnectionFlow(string modeStr)
    {
        GameModeType mode = (modeStr == "vs_player") ? GameModeType.VsPlayer : GameModeType.VsBot;
        if (_gameState != null) _gameState.SelectedMode = mode;
        SetButtonsInteractable(false);

        string nickname = (_nicknameInput != null && !string.IsNullOrEmpty(_nicknameInput.value))
            ? _nicknameInput.value
            : "JugadorDesconegut";

        Debug.Log($"[MenuController] FASE 1: Intentant Login amb nickname: {nickname}");

        NetworkManager.Instance.LoginUser(nickname, (loginSuccess) =>
        {
            if (loginSuccess)
            {
                Debug.Log($"[MenuController] FASE 2: Login correcte. Unint-se a partida mode: {modeStr}");

                NetworkManager.Instance.JoinGame(mode, (joinSuccess) =>
                {
                    if (joinSuccess)
                    {
                        Debug.Log("[MenuController] FASE 3: Partida trobada. Connectant per WebSocket...");
                        NetworkManager.Instance.ConnectToGame();

                        Debug.Log("[MenuController] FASE 4: Connexió iniciada. Carregant escena de joc...");
                        SceneManager.LoadScene("TickTack-Tag");

                    }
                    else
                    {
                        Debug.LogError("[MenuController] ERROR: No s'ha pogut unir a la partida.");
                        SetButtonsInteractable(true);
                    }
                });
            }
            else
            {
                Debug.LogError("[MenuController] ERROR: El Login ha fallat.");
                SetButtonsInteractable(true);
            }
        });
    }

    private void SetButtonsInteractable(bool state)
    {
        if (_btnVsBot != null) _btnVsBot.SetEnabled(state);
        if (_btnVsPlayer != null) _btnVsPlayer.SetEnabled(state);
    }
}
