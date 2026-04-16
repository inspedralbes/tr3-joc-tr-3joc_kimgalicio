using UnityEngine;

public class TagCollision : MonoBehaviour
{
    public GameStateSO GameState;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameState == null || GameState.GameOver) return;

        // Comprobamos si chocamos contra otro jugador o bot
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bot"))
        {
            // IMPORTANTE: Solo disparamos el traspaso si YO soy el que tiene la bomba
            // (Así evitamos que el traspaso se active dos veces en el mismo choque)
            if (GameState.CurrentBombOwner == this.gameObject)
            {
                // Buscamos la bomba y ordenamos que se pase al que acabamos de tocar
                Bomb bomb = FindFirstObjectByType<Bomb>();
                if (bomb != null)
                {
                    bomb.TransferTo(collision.gameObject);
                }
            }
        }
    }
}