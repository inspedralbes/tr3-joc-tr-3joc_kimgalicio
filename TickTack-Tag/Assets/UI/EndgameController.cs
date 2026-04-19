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

        // Busquem els elements. Si ja els teníem, no fem res.
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

        // Amaguem el panell per defecte (per si s'ha quedat encès a l'editor)
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

        // Ens assegurem que les referències estiguin a punt
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
                string nickname = (winnerId.ToString() == localUserId) ? "TU HAS GUANYAT!" : $"JUGADOR {winnerId} GUANYA!";
                _lblWinner.text = $"{nickname}\nRestaven {winnerHearts} cors.";
            }
        }

        if (_btnRestart != null)
        {
            // Restart només visible en vs_bot o si no hi ha GameId (local)
            bool isVsBotOrLocal = (winnerId == 0 || NetworkManager.Instance == null || string.IsNullOrEmpty(NetworkManager.Instance.GameId));
            _btnRestart.style.display = isVsBotOrLocal ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void OnMenuClicked() => SceneManager.LoadScene("MainMenu");
    private void OnRestartClicked() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
