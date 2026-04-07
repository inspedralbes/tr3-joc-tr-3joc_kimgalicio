using UnityEngine;
using TMPro; // Assuming TextMeshPro is being used

public class TimerDisplay : MonoBehaviour
{
    public GameStateSO GameState;
    public TextMeshProUGUI TimerText;

    void Update()
    {
        if (GameState == null || TimerText == null) return;

        if (GameState.GameOver)
        {
            TimerText.text = GameState.WinnerName;
        }
        else
        {
            TimerText.text = GameState.GameTimer.ToString("F1");
        }
    }
}
