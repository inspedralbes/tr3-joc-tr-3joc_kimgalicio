using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    [SerializeField] private GameStateSO gameState;
    [SerializeField] private int damage = 1;
    [SerializeField] private float cooldown = 0.8f;

    private float nextTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < nextTime) return;
        nextTime = Time.time + cooldown;
        gameState.TakeDamage(damage);
    }
}
