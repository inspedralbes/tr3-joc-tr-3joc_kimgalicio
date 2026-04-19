using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BotEvader : Agent
{
    [Header("Configuració de Referències")]
    public GameStateSO GameState;
    public PlayerModeController2D Controller;
    public LayerMask ladderLayer;

    [Tooltip("Arrossega aquí el Transform del jugador (Dino verd)")]
    public Transform OpponentTarget;

    [Header("Spawns d'Entrenament")]
    public Transform Spawn1;
    public Transform Spawn2;

    [Header("Ajustes de IA")]
    public float DecisionInterval = 0.1f;
    public float ladderCheckRadius = 0.3f;
    public float distanceThreshold = 5.0f;

    private float _nextDecisionTime;
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInput;
    private bool _isNearLadder;

    public override void Initialize()
    {
        if (Controller == null) Controller = GetComponent<PlayerModeController2D>();
        if (GameState != null)
        {
            GameState.ResetRun();
            GameState.InitializeEntity(gameObject.name);
        }
    }

    void Update()
    {
        if (GameState == null || GameState.GameOver || Controller == null) return;

        if (GameState.Spectators.Contains(gameObject.name))
        {
            _horizontalInput = 0f;
            _verticalInput = 0f;
            _jumpInput = false;
            Controller.SetInput(0, 0, false);
            return;
        }

        if (Time.time >= _nextDecisionTime)
        {
            RequestDecision();
            _nextDecisionTime = Time.time + DecisionInterval;
        }

        Controller.SetInput(_horizontalInput, _verticalInput, _jumpInput);
        _jumpInput = false;
    }

    public override void OnEpisodeBegin()
    {

        _horizontalInput = 0f;
        _verticalInput = 0f;
        _jumpInput = false;

        if (Spawn1 != null && Spawn2 != null)
        {
            if (Random.value > 0.5f)
            {
                transform.position = Spawn1.position;
                if (OpponentTarget != null) OpponentTarget.position = Spawn2.position;
            }
            else
            {
                transform.position = Spawn2.position;
                if (OpponentTarget != null) OpponentTarget.position = Spawn1.position;
            }
        }

        if (GameState != null && OpponentTarget != null)
        {
            GameState.SetBombOwner(null);

            Bomb bomb = FindFirstObjectByType<Bomb>();
            if (bomb != null)
            {

                bomb.TransferTo(OpponentTarget.gameObject);
                GameState.GameTimer = GameState.InitialTimer;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y);

        Vector2 checkPosition = (Vector2)transform.position + (Vector2.up * 0.5f);
        _isNearLadder = Physics2D.OverlapCircle(checkPosition, ladderCheckRadius, ladderLayer);
        sensor.AddObservation(_isNearLadder ? 1f : 0f);

        if (OpponentTarget != null)
        {
            Vector2 relativePos = (Vector2)OpponentTarget.position - (Vector2)transform.position;
            sensor.AddObservation(relativePos.x);
            sensor.AddObservation(relativePos.y);

            sensor.AddObservation(relativePos.magnitude);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        _horizontalInput = actions.ContinuousActions[0];
        _jumpInput = actions.DiscreteActions[0] == 1;

        int verticalAction = actions.DiscreteActions[1];
        if (verticalAction == 1) _verticalInput = 1f;
        else if (verticalAction == 2) _verticalInput = -1f;
        else _verticalInput = 0f;

        AddReward(0.001f);

        if (OpponentTarget != null)
        {
            float dist = Vector2.Distance(transform.position, OpponentTarget.position);
            if (dist > distanceThreshold)
            {

                AddReward(0.001f);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("¡Atrapado! Aplicando castigo y reseteando episodio.");
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;

        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        float v = Input.GetAxisRaw("Vertical");
        if (v > 0) discreteActionsOut[1] = 1;
        else if (v < 0) discreteActionsOut[1] = 2;
        else discreteActionsOut[1] = 0;
    }

    public void ApplySpeedMultiplier(float multiplier)
    {
        if (Controller != null) Controller.ApplySpeedMultiplier(multiplier);
    }

    public void ResetSpeedMultiplier()
    {
        if (Controller != null) Controller.ResetSpeedMultiplier();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 checkPos = (Vector2)transform.position + (Vector2.up * 0.5f);
        Gizmos.DrawWireSphere(checkPos, ladderCheckRadius);

        if (OpponentTarget != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(transform.position, distanceThreshold);
        }
    }
}
