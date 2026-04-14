using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BotController : Agent
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
            RequestDecision();
            _nextDecisionTime = Time.time + DecisionInterval;
        }

        Controller.SetInput(_horizontalInput, 0f, _jumpInput);
        _jumpInput = false;
    }

    public override void OnEpisodeBegin()
    {
        // Reset bot state if needed
        _horizontalInput = 0f;
        _jumpInput = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add observations
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y);
        
        // Bomb owner
        bool hasBomb = (GameState.CurrentBombOwner == gameObject);
        sensor.AddObservation(hasBomb ? 1f : 0f);
        
        // Distance to bomb owner
        if (GameState.CurrentBombOwner != null)
        {
            float distToBomb = Vector3.Distance(transform.position, GameState.CurrentBombOwner.transform.position);
            sensor.AddObservation(distToBomb);
        }
        else
        {
            sensor.AddObservation(0f);
        }
        
        // Distances to other players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");
        
        List<GameObject> allTargets = new List<GameObject>(players);
        allTargets.AddRange(bots);
        
        foreach (var t in allTargets)
        {
            if (t == gameObject) continue;
            if (GameState.Spectators.Contains(t.name)) continue;
            float dist = Vector3.Distance(transform.position, t.transform.position);
            sensor.AddObservation(dist);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        _horizontalInput = actions.ContinuousActions[0]; // -1 to 1
        _jumpInput = actions.DiscreteActions[0] == 1; // 0 or 1
        
        // Rewards
        if (GameState.CurrentBombOwner == gameObject)
        {
            // Reward for having bomb
            AddReward(0.01f);
        }
        else
        {
            // Penalty for not having bomb
            AddReward(-0.01f);
        }
        
        // If tagged someone, big reward
        // Assuming GameState has some way to track tags, but for now, placeholder
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        // Use the old logic for heuristic
        MakeDecision();
        continuousActionsOut[0] = _horizontalInput;
        discreteActionsOut[0] = _jumpInput ? 1 : 0;
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
