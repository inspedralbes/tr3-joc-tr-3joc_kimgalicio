using UnityEngine;

public class TagCollision : MonoBehaviour
{
    public GameStateSO GameState;

    private GameManager _gameManager;

    private void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameState == null || GameState.GameOver) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bot"))
        {

            if (GameState.CurrentBombOwner == this.gameObject)
            {

                Bomb bomb = FindFirstObjectByType<Bomb>();
                if (bomb != null)
                {
                    bool success = bomb.TransferTo(collision.gameObject);

                    // --- MULTIPLAYER SYNC ---
                    if (success && NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.GameId))
                    {
                        string targetUserId = _gameManager != null ? _gameManager.GetUserIdOf(collision.gameObject) : "";
                        if (!string.IsNullOrEmpty(targetUserId))
                        {
                            NetworkManager.Instance.SendBombTransfer(targetUserId);
                        }
                    }
                }
            }
        }
    }
}
