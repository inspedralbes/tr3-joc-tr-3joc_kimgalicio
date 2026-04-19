using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {

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
