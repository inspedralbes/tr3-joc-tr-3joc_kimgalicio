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
            TimerText.text = $"GUANYADOR:\n{GameState.WinnerName}!\n\n<size=60%>(Prem 'R' per reiniciar)</size>";
            TimerText.transform.localScale = Vector3.one;
            return;
        }

        TimerText.text = GameState.GameTimer.ToString("F1") + "s";

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

        if (GameState.GameTimer <= WarningThreshold && GameState.GameTimer > 0)
        {

            float speedMultiplier = Mathf.InverseLerp(WarningThreshold, 0, GameState.GameTimer) * 5f + 2f;
            float pulse = 1f + Mathf.PingPong(Time.time * speedMultiplier, 0.15f);
            TimerText.transform.localScale = new Vector3(pulse, pulse, 1f);
        }
    }
}
