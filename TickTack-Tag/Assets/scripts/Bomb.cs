using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameStateSO GameState;
    public float TransferCooldown = 1.0f;
    public Vector3 Offset = new Vector3(0, 1.5f, 0);

    private float _lastTransferTime;

    void Update()
    {
        if (GameState == null || GameState.GameOver) return;

        if (GameState.CurrentBombOwner != null)
        {
            // Follow the owner
            transform.position = GameState.CurrentBombOwner.transform.position + Offset;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameState == null || GameState.GameOver) return;

        // Check if the entity that touched us is NOT the current owner
        if (other.gameObject != GameState.CurrentBombOwner)
        {
            // Cooldown check
            if (Time.time - _lastTransferTime < TransferCooldown) return;

            // Transfer bomb
            Debug.Log($"Bomb transferred to {other.gameObject.name}");
            GameState.SetBombOwner(other.gameObject);
            _lastTransferTime = Time.time;
        }
    }
}
