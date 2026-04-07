using UnityEngine;

public class BotController : MonoBehaviour
{
    public GameStateSO GameState;
    public PlayerModeController2D Controller;
    public float DecisionInterval = 0.2f;

    private float _nextDecisionTime;
    private float _horizontalInput;
    private bool _jumpInput;

    void Update()
    {
        if (GameState == null || GameState.GameOver || Controller == null) return;

        if (Time.time >= _nextDecisionTime)
        {
            MakeDecision();
            _nextDecisionTime = Time.time + DecisionInterval;
        }

        // Apply input to the controller
        Controller.SetInput(_horizontalInput, 0f, _jumpInput);
        _jumpInput = false; // Reset jump input after sending
    }

    private void MakeDecision()
    {
        GameObject bombOwner = GameState.CurrentBombOwner;
        if (bombOwner == null) return;

        bool hasBomb = (bombOwner == gameObject);

        if (hasBomb)
        {
            ChaseClosestPlayer();
        }
        else
        {
            FleeFromBombOwner(bombOwner);
        }
    }

    private void ChaseClosestPlayer()
    {
        // Find closest entity that is NOT me
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var p in players)
        {
            if (p == gameObject) continue;
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = p;
            }
        }

        if (closest != null)
        {
            _horizontalInput = (closest.transform.position.x > transform.position.x) ? 1f : -1f;
            
            // Basic jump if we are close to the target or there's a height difference
            if (Mathf.Abs(closest.transform.position.y - transform.position.y) > 1.0f)
            {
                _jumpInput = true;
            }
        }
    }

    private void FleeFromBombOwner(GameObject owner)
    {
        float dist = Vector3.Distance(transform.position, owner.transform.position);
        
        // Only flee if the owner is reasonably close
        if (dist < 10.0f)
        {
            _horizontalInput = (owner.transform.position.x > transform.position.x) ? -1f : 1f;
            
            // Jump randomly to evade
            if (Random.value < 0.1f) _jumpInput = true;
        }
        else
        {
            _horizontalInput = Mathf.Lerp(_horizontalInput, 0f, Time.deltaTime);
        }
    }
}
