using UnityEngine;
using TMPro; // Assuming TextMeshPro is being used

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
        if (GameState != null) GameState.OnChanged += RefreshUI;
    }

    void OnDisable()
    {
        if (GameState != null) GameState.OnChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        if (GameState == null || TimerText == null) return;

        if (GameState.GameOver)
        {
            TimerText.color = VictoryColor;
            TimerText.text = $"GUANYADOR:\n{GameState.WinnerName}!\n\n(Prem 'R' per reiniciar)";
            return;
        }

        // Mostrar temps actual
        TimerText.text = GameState.GameTimer.ToString("F1");

        // Canviar color si el temps és baix
        if (GameState.GameTimer <= WarningThreshold)
        {
            TimerText.color = WarningColor;
            // Efecte de bateg (simplificat amb escala)
            float pulse = 1f + Mathf.PingPong(Time.time * 2f, 0.2f);
            TimerText.transform.localScale = new Vector3(pulse, pulse, 1f);
        }
        else
        {
            TimerText.color = NormalColor;
            TimerText.transform.localScale = Vector3.one;
        }
    }

    // Encara necessitem Update només per l'efecte de bateg si volem que sigui fluid
    // però el text es podria actualitzar només amb l'esdeveniment.
    // Tanmateix, per ara el bateg el posem aquí per comoditat.
    void Update()
    {
        if (GameState != null && !GameState.GameOver && GameState.GameTimer <= WarningThreshold)
        {
            float pulse = 1f + Mathf.PingPong(Time.time * 2f, 0.2f);
            TimerText.transform.localScale = new Vector3(pulse, pulse, 1f);
        }
    }
}
