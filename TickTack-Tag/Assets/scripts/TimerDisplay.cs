using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    public GameStateSO GameState;
    public TextMeshProUGUI TimerText;

    [Header("Visual Config")]
    public Color NormalColor = Color.white;
    public Color WarningColor = Color.red;
    public Color VictoryColor = Color.green;
    public float WarningThreshold = 10f;

    void OnEnable()
    {
        // Ens suscribim a l'esdeveniment per actualitzar el text quan el GameState canviï
        if (GameState != null) GameState.OnChanged += RefreshUI;
    }

    void OnDisable()
    {
        // Molt important desubscriure's per evitar errors de memòria
        if (GameState != null) GameState.OnChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        if (GameState == null || TimerText == null) return;

        // ESTAT: FINAL DE JOC
        if (GameState.GameOver)
        {
            TimerText.color = VictoryColor;
            TimerText.text = $"GUANYADOR:\n{GameState.WinnerName}!\n\n<size=60%>(Prem 'R' per reiniciar)</size>";
            TimerText.transform.localScale = Vector3.one;
            return;
        }

        // ESTAT: JUGANT (Mostrar temps actual amb 1 decimal)
        TimerText.text = GameState.GameTimer.ToString("F1") + "s";

        // CANVI DE COLOR SEGONS EL TEMPS CRÍTIC
        if (GameState.GameTimer <= WarningThreshold)
        {
            TimerText.color = WarningColor;
        }
        else
        {
            TimerText.color = NormalColor;
            TimerText.transform.localScale = Vector3.one;
        }
    }

    void Update()
    {
        if (GameState == null || TimerText == null || GameState.GameOver) return;

        // EFECTE DE BATEG (PULSE)
        // El posem a l'Update perquè el moviment sigui suau (60fps) i no depengui només de l'OnChanged
        if (GameState.GameTimer <= WarningThreshold && GameState.GameTimer > 0)
        {
            // Fem que el bateg sigui més ràpid com menys temps quedi
            float speedMultiplier = Mathf.InverseLerp(WarningThreshold, 0, GameState.GameTimer) * 5f + 2f;
            float pulse = 1f + Mathf.PingPong(Time.time * speedMultiplier, 0.15f);
            TimerText.transform.localScale = new Vector3(pulse, pulse, 1f);
        }
    }
}