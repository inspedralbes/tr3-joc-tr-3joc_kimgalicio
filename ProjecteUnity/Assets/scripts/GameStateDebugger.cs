using UnityEngine;

public class GameStateDebugger : MonoBehaviour
{
    [SerializeField] private GameStateSO gameState;

    [Header("Runtime (read only)")]
    [SerializeField] private int score;
    [SerializeField] private int lives;
    [SerializeField] private bool gameOver;

    private void Update()
    {
        if (gameState == null) return;
        score = gameState.Score;
        lives = gameState.Lives;
        gameOver = gameState.GameOver;
    }
}