using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [SerializeField] private GameStateSO gameStateSO;

    private Label lblTimer;
    private Label lblLivesP1;
    private Label lblLivesP2;
    private Label lblRound;
    private VisualElement countdownOverlay;
    private Label lblCountdown;

    private string playerEntityName = "";
    private string botEntityName = "";
    private UIDocument _uiDocument;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeReferences();
        StartCoroutine(FindEntityNames());

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            HideHUD();
        }
        else
        {
            ShowHUD();
        }
    }

    private void InitializeReferences()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;

        var root = _uiDocument.rootVisualElement;
        
        lblTimer = root.Q<Label>("lbl-timer");
        lblLivesP1 = root.Q<Label>("lbl-lives-p1");
        lblLivesP2 = root.Q<Label>("lbl-lives-p2");
        lblRound = root.Q<Label>("lbl-round");
        countdownOverlay = root.Q<VisualElement>("countdown-overlay");
        lblCountdown = root.Q<Label>("lbl-countdown");

        if (countdownOverlay != null) 
        {
            countdownOverlay.style.display = DisplayStyle.None;
            countdownOverlay.visible = false;
        }
    }

    private IEnumerator FindEntityNames()
    {
        yield return new WaitForSeconds(0.5f);
        if (gameStateSO == null || gameStateSO.EntityLives == null) yield break;

        foreach (var key in gameStateSO.EntityLives.Keys)
        {
            if (key.ToLower().Contains("bot")) botEntityName = key;
            else playerEntityName = key;
        }
    }

    private void Update()
    {
        if (gameStateSO == null) return;

        UpdateTimer();
        UpdateLivesDisplay();
        
        if (lblRound != null) 
        {
            lblRound.text = "RONDA " + gameStateSO.CurrentRound;
        }
    }

    private void UpdateTimer()
    {
        if (lblTimer == null) return;
        int timeLeft = Mathf.CeilToInt(gameStateSO.GameTimer);
        lblTimer.text = timeLeft.ToString();
        lblTimer.style.color = (timeLeft <= 5) ? new StyleColor(Color.red) : new StyleColor(Color.yellow);
    }

    private void UpdateLivesDisplay()
    {
        if (playerEntityName != "") UpdateLifeLabel(playerEntityName, lblLivesP1);
        if (botEntityName != "") UpdateLifeLabel(botEntityName, lblLivesP2);
    }

    private void UpdateLifeLabel(string entityName, Label label)
    {
        if (label == null || gameStateSO == null) return;
        label.text = gameStateSO.GetLives(entityName).ToString();
    }

    public void SetWaitingForOpponent(bool isWaiting)
    {
        if (countdownOverlay == null || lblCountdown == null) InitializeReferences();
        
        if (isWaiting)
        {
            countdownOverlay.style.display = DisplayStyle.Flex;
            countdownOverlay.visible = true;
            lblCountdown.text = "Esperant oponent...";
            lblCountdown.style.fontSize = 40;
            lblCountdown.style.color = new StyleColor(Color.white);
        }
        else
        {
            countdownOverlay.style.display = DisplayStyle.None;
            countdownOverlay.visible = false;
        }
    }

    public void ShowCountdown(System.Action onComplete)
    {
        if (countdownOverlay == null || lblCountdown == null) InitializeReferences();
        StartCoroutine(CountdownCoroutine(onComplete));
    }

    private IEnumerator CountdownCoroutine(System.Action onComplete)
    {
        if (countdownOverlay == null || lblCountdown == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        countdownOverlay.style.display = DisplayStyle.Flex;
        countdownOverlay.visible = true;
        
        // Utilitzem una mida de font conservadora i white-space: nowrap (al UXML) per evitar salts de línia
        lblCountdown.text = "Comença la Ronda " + gameStateSO.CurrentRound;
        lblCountdown.style.fontSize = 45; 
        lblCountdown.style.color = new StyleColor(Color.white);
        yield return new WaitForSeconds(1.5f);

        for (int i = 3; i > 0; i--)
        {
            lblCountdown.text = i.ToString();
            lblCountdown.style.fontSize = 120;
            yield return new WaitForSeconds(0.2f);
            lblCountdown.style.fontSize = 100;
            yield return new WaitForSeconds(0.8f);
        }

        lblCountdown.text = "¡¡Comença!!";
        lblCountdown.style.fontSize = 70;
        lblCountdown.style.color = new StyleColor(Color.green);
        yield return new WaitForSeconds(0.8f);

        countdownOverlay.style.display = DisplayStyle.None;
        countdownOverlay.visible = false;

        onComplete?.Invoke();
    }

    public void ShowHUD()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument != null) _uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void HideHUD()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument != null) _uiDocument.rootVisualElement.style.display = DisplayStyle.None;
    }
}
