using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeReferences();
        StartCoroutine(FindEntityNames());
    }

    private void InitializeReferences()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var root = uiDocument.rootVisualElement;
        
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

        if (lblRound == null) Debug.LogWarning("[HUD] No s'ha trobat 'lbl-round' al UXML.");
        if (countdownOverlay == null) Debug.LogWarning("[HUD] No s'ha trobat 'countdown-overlay' al UXML.");
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
        else 
        {
            // Intentem re-inicialitzar si s'ha perdut la referència
            lblRound = GetComponent<UIDocument>().rootVisualElement.Q<Label>("lbl-round");
        }
    }

    private void UpdateTimer()
    {
        if (lblTimer == null) 
        {
            lblTimer = GetComponent<UIDocument>().rootVisualElement.Q<Label>("lbl-timer");
            if (lblTimer == null) return;
        }

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

    public void ShowCountdown(System.Action onComplete)
    {
        // Ens assegurem de tenir les referències abans de començar
        if (countdownOverlay == null || lblCountdown == null) InitializeReferences();
        StartCoroutine(CountdownCoroutine(onComplete));
    }

    private IEnumerator CountdownCoroutine(System.Action onComplete)
    {
        if (countdownOverlay == null || lblCountdown == null)
        {
            Debug.LogError("[HUD] ERROR CRÍTIC: No s'ha trobat l'overlay o el label del compte enrere després de re-intentar.");
            onComplete?.Invoke();
            yield break;
        }

        countdownOverlay.style.display = DisplayStyle.Flex;
        countdownOverlay.visible = true;
        
        lblCountdown.text = "Comença la\nRonda " + gameStateSO.CurrentRound;
        lblCountdown.style.fontSize = 60;
        lblCountdown.style.color = new StyleColor(Color.white);
        yield return new WaitForSeconds(1.5f);

        for (int i = 3; i > 0; i--)
        {
            lblCountdown.text = i.ToString();
            lblCountdown.style.fontSize = 160;
            yield return new WaitForSeconds(0.2f);
            lblCountdown.style.fontSize = 120;
            yield return new WaitForSeconds(0.8f);
        }

        lblCountdown.text = "Comença!!";
        lblCountdown.style.color = new StyleColor(Color.green);
        yield return new WaitForSeconds(0.8f);

        countdownOverlay.style.display = DisplayStyle.None;
        countdownOverlay.visible = false;

        onComplete?.Invoke();
    }
}
