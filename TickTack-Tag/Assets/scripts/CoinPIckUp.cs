using UnityEngine;

public class CoinPIckUp : MonoBehaviour
{
    [SerializeField] private GameStateSO gameState;
    [SerializeField] private int points = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        //log coin is picked up 

        Debug.Log("Coin picked up!  1");
        if (!other.CompareTag("Player")) return;

        Debug.Log("Coin picked up!   2");

        gameState.AddScore(points);
        gameObject.SetActive(false);
    }
}
