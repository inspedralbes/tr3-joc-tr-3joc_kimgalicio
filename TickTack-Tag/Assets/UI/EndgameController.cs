using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class EndgameController : MonoBehaviour
{
    [Header("UI Toolkit References")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement _root;
    private Label _lblWinner;
    private Button _btnMenu;
    private Button _btnRestart;
    private VisualElement _endgamePanel;
    private bool _isGameOverHandled;

    private void OnEnable()
    {
        Debug.Log("[Endgame] Enabled");
        _isGameOverHandled = false;
        NetworkManager.OnGameOverReceived += HandleGameOver;
        GameManager.OnLocalGameOver += HandleLocalGameOver;
        Initialize();
    }

    private void OnDisable()
    {
        if (_btnMenu != null) _btnMenu.clicked -= OnMenuClicked;
        if (_btnRestart != null) _btnRestart.clicked -= OnRestartClicked;
        NetworkManager.OnGameOverReceived -= HandleGameOver;
        GameManager.OnLocalGameOver -= HandleLocalGameOver;
    }

    private void Initialize()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogWarning("[Endgame] UIDocument null!");
            return;
        }

        _root = uiDocument.rootVisualElement;
        if (_root == null)
        {
            Debug.LogWarning("[Endgame] rootVisualElement null!");
            return;
        }

        if (_endgamePanel == null) _endgamePanel = _root.Q<VisualElement>("endgame-panel");
        if (_lblWinner == null) _lblWinner = _root.Q<Label>("lbl-winner");

        if (_btnMenu == null)
        {
            _btnMenu = _root.Q<Button>("btn-menu");
            if (_btnMenu != null) _btnMenu.clicked += OnMenuClicked;
        }

        if (_btnRestart == null)
        {
            _btnRestart = _root.Q<Button>("btn-restart");
            if (_btnRestart != null) _btnRestart.clicked += OnRestartClicked;
        }

        if (_endgamePanel != null)
            _endgamePanel.style.display = DisplayStyle.None;
    }

    private void HandleGameOver(WsGameOverMessage data)
    {
        Debug.Log($"[Endgame] Rebut OnGameOverReceived de xarxa. Winner: {data.winnerId}");
        ShowEndgamePanel(data.winnerId, data.winnerHearts);
    }

    private void HandleLocalGameOver(int winnerId, int hearts)
    {
        Debug.Log($"[Endgame] Rebut OnLocalGameOver local. Winner: {winnerId}");
        ShowEndgamePanel(winnerId, hearts);
    }

    private void ShowEndgamePanel(int winnerId, int winnerHearts)
    {
        if (_isGameOverHandled) return;
        _isGameOverHandled = true;

        Initialize();

        Debug.Log($"[Endgame] Mostrant panell (Origen: {(winnerId == 0 ? "Local/Bot" : "Xarxa")}) Guanyador: {winnerId}");

        if (_endgamePanel == null)
        {
            Debug.LogError("[Endgame] No s'ha trobat l'element 'endgame-panel' al UXML!");
            return;
        }

        _endgamePanel.style.display = DisplayStyle.Flex;

        if (_lblWinner != null)
        {
            if (winnerId == 0)
            {
                _lblWinner.text = $"EL BOT HA GUANYAT!\nRestaven {winnerHearts} cors.";
            }
            else
            {
                string localUserId = (NetworkManager.Instance != null) ? NetworkManager.Instance.UserId : "";
                string nickname = "";

                if (winnerId.ToString() == localUserId)
                {
                    nickname = (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.PlayerNickname))
                        ? NetworkManager.Instance.PlayerNickname
                        : "TU HAS GUANYAT!";
                }
                else
                {
                    nickname = $"JUGADOR {winnerId}";
                }

                _lblWinner.text = $"{nickname.ToUpper()} HA GUANYAT!\nRestaven {winnerHearts} cors.";
            }
        }

        if (_btnRestart != null)
        {

            bool isVsBotOrLocal = (winnerId == 0 || NetworkManager.Instance == null || string.IsNullOrEmpty(NetworkManager.Instance.GameId));
            _btnRestart.style.display = isVsBotOrLocal ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void OnMenuClicked() => SceneManager.LoadScene("Menu");
    private void OnRestartClicked() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
