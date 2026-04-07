using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameStateSO GameState;
    public GameObject[] Entities; // Player and Bots

    void Start()
    {
        if (GameState == null) return;

        GameState.ResetRun();

        if (Entities != null && Entities.Length > 0)
        {
            // Randomly assign bomb to one entity
            int randomIndex = Random.Range(0, Entities.Length);
            GameState.SetBombOwner(Entities[randomIndex]);
            Debug.Log($"Game Started! Bomb owner: {Entities[randomIndex].name}");
        }
    }

    void Update()
    {
        if (GameState == null || GameState.GameOver) return;

        GameState.UpdateTimer(Time.deltaTime);

        if (GameState.GameTimer <= 0)
        {
            string loser = GameState.CurrentBombOwner != null ? GameState.CurrentBombOwner.name : "No one";
            Debug.Log($"BOOM! {loser} had the bomb and lost.");
            
            // For simplicity, let's say the winner is anyone who doesn't have the bomb
            // But we'll just set the game over state for now.
            GameState.SetGameOver("Game Over"); 
        }
    }
}
