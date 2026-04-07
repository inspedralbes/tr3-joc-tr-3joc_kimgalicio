using UnityEngine;
using System.Collections.Generic;

public class BotController : MonoBehaviour
{
    public GameStateSO GameState;
    public PlayerModeController2D Controller;
    public float DecisionInterval = 0.2f;

    private float _nextDecisionTime;
    private float _horizontalInput;
    private bool _jumpInput;

    void Start()
    {
        if (GameState != null) GameState.InitializeEntity(gameObject.name);
    }

    void Update()
    {
        if (GameState == null || GameState.GameOver || Controller == null) return;
        
        // Si sóc espectador, no faig res
        if (GameState.Spectators.Contains(gameObject.name))
        {
            _horizontalInput = 0f;
            _jumpInput = false;
            Controller.SetInput(0, 0, false);
            return;
        }

        if (Time.time >= _nextDecisionTime)
        {
            MakeDecision();
            _nextDecisionTime = Time.time + DecisionInterval;
        }

        Controller.SetInput(_horizontalInput, 0f, _jumpInput);
        _jumpInput = false;
    }

    private void MakeDecision()
    {
        if (GameState.CurrentBombOwner == null) return;

        bool hasBomb = (GameState.CurrentBombOwner == gameObject);

        if (hasBomb)
        {
            ChaseClosestTarget();
        }
        else
        {
            FleeFromBombOwner(GameState.CurrentBombOwner);
        }
    }

    private void ChaseClosestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");
        
        List<GameObject> allTargets = new List<GameObject>(players);
        allTargets.AddRange(bots);

        GameObject closest = null;
        float minDistance = float.MaxValue;

        foreach (var t in allTargets)
        {
            if (t == gameObject) continue;
            // Ignorar espectadors
            if (GameState.Spectators.Contains(t.name)) continue;

            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = t;
            }
        }

        if (closest != null)
        {
            _horizontalInput = (closest.transform.position.x > transform.position.x) ? 1f : -1f;
            if (Mathf.Abs(closest.transform.position.y - transform.position.y) > 1.5f) _jumpInput = true;
        }
    }

    private void FleeFromBombOwner(GameObject owner)
    {
        float dist = Vector3.Distance(transform.position, owner.transform.position);
        
        if (dist < 12.0f)
        {
            _horizontalInput = (owner.transform.position.x > transform.position.x) ? -1f : 1f;
            if (Random.value < 0.05f) _jumpInput = true;
        }
        else
        {
            _horizontalInput = 0f;
        }
    }

    public void ApplySpeedMultiplier(float multiplier)
    {
        if (Controller != null) Controller.ApplySpeedMultiplier(multiplier);
    }

    public void ResetSpeedMultiplier()
    {
        if (Controller != null) Controller.ResetSpeedMultiplier();
    }
}
