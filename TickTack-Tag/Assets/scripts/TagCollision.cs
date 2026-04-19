using UnityEngine;

public class TagCollision : MonoBehaviour
{
    public GameStateSO GameState;

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
                    bomb.TransferTo(collision.gameObject);
                }
            }
        }
    }
}
