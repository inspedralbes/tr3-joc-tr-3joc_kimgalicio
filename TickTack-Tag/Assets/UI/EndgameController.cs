using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Gestiona la pantalla de resultats final utilitzant UI Toolkit.
/// S'activa quan rep l'esdeveniment game_over del NetworkManager.
/// </summary>
public class EndgameController : MonoBehaviour
{
    [Header("UI Toolkit References")]
    [SerializeField] private UIDocument uiDocument;
    
    private VisualElement _root;
    private Label _lblWinner;
    private Button _btnMenu;
    private Button _btnRestart;
    private VisualElement _endgamePanel;

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        
        _root = uiDocument.rootVisualElement;

        // Busquem els elements pel nom definit al UXML
        _lblWinner = _root.Q<Label>("lbl-winner");
        _btnMenu = _root.Q<Button>("btn-menu");
        _btnRestart = _root.Q<Button>("btn-restart");
        _endgamePanel = _root.Q<VisualElement>("endgame-panel");

        // Inicialment amaguem el panell
        if (_endgamePanel != null) _endgamePanel.style.display = DisplayStyle.None;

        // Subscripció als botons
        if (_btnMenu != null) _btnMenu.clicked += OnMenuClicked;
        if (_btnRestart != null) _btnRestart.clicked += OnRestartClicked;

        // Subscripció a l'esdeveniment de xarxa
        NetworkManager.OnGameOverReceived += HandleGameOver;
    }

    private void OnDisable()
    {
        // Desubscripció per evitar memory leaks
        if (_btnMenu != null) _btnMenu.clicked -= OnMenuClicked;
        if (_btnRestart != null) _btnRestart.clicked -= OnRestartClicked;
        
        NetworkManager.OnGameOverReceived -= HandleGameOver;
    }

    private void HandleGameOver(WsGameOverMessage data)
    {
        Debug.Log($"[Endgame] Joc acabat! Guanyador ID: {data.winnerId} amb {data.winnerHearts} cors.");
        
        if (_endgamePanel == null) return;

        // Mostrem el panell
        _endgamePanel.style.display = DisplayStyle.Flex;

        // Actualitzem el text del guanyador
        if (_lblWinner != null)
        {
            if (data.winnerId == 0)
            {
                _lblWinner.text = $"EL BOT HA GUANYAT!\nRestaven {data.winnerHearts} cors.";
            }
            else
            {
                // Si som nosaltres o un altre jugador
                string nickname = (data.winnerId.ToString() == NetworkManager.Instance.UserId) ? "TU HAS GUANYAT!" : $"EL JUGADOR {data.winnerId} HA GUANYAT!";
                _lblWinner.text = $"{nickname}\nRestaven {data.winnerHearts} cors.";
            }
        }

        // El botó "Tornar a jugar" només és visible si estàvem en mode vs_bot o si volem permetre-ho
        // Segons requeriment: "Només visible si el mode era vs_bot"
        if (_btnRestart != null)
        {
            // Podem saber el mode via una variable global o consultant el NetworkManager si ho tingués guardat
            // Aquí assumirem que el mostrem si el winnerId és 0 (el bot ha participat) o si el mode era bot.
            // Per ara el deixarem visible per defecte si winnerId és 0 o el guanyador som nosaltres en un entorn single player.
            bool isVsBot = (data.winnerId == 0 || (NetworkManager.Instance != null && string.IsNullOrEmpty(NetworkManager.Instance.GameId)));
             _btnRestart.style.display = isVsBot ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void OnMenuClicked()
    {
        Debug.Log("[Endgame] Tornant al Menú...");
        // Carrega l'escena del menú (ajusta el nom si és diferent)
        SceneManager.LoadScene("MainMenu"); 
    }

    private void OnRestartClicked()
    {
        Debug.Log("[Endgame] Reiniciant partida...");
        // Simplement recarreguem l'escena actual per reiniciar tota la lògica
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
