using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprovar si el que ha caigut és un jugador o bot
        if (other.CompareTag("Player") || other.CompareTag("Bot"))
        {
            GameManager manager = FindFirstObjectByType<GameManager>();
            if (manager != null)
            {
                Debug.Log($"{other.gameObject.name} ha caigut al buit!");
                manager.HandleDeath(other.gameObject);
            }
        }
    }
}
