using UnityEngine;
using System.Text;

public class GameStateDebugger : MonoBehaviour
{
    [SerializeField] private GameStateSO gameState;

    [Header("Runtime (read only)")]
    [SerializeField, TextArea(3, 10)] private string entitiesStatus;
    [SerializeField] private bool gameOver;
    [SerializeField] private string currentBombOwner;

    private void Update()
    {
        if (gameState == null) return;

        StringBuilder sb = new StringBuilder();
        foreach (var kvp in gameState.EntityLives)
        {
            sb.AppendLine($"{kvp.Key}: {kvp.Value} lives" + (gameState.Spectators.Contains(kvp.Key) ? " (Spectator)" : ""));
        }
        entitiesStatus = sb.ToString();

        gameOver = gameState.GameOver;
        currentBombOwner = gameState.CurrentBombOwner != null ? gameState.CurrentBombOwner.name : "None";
    }
}
