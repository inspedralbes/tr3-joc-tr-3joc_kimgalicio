using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.InferenceEngine;
using Unity.MLAgents.Policies;

public class BotController : Agent
{
    [Header("Configuració de Referències")]
    public GameStateSO GameState;
    public PlayerModeController2D Controller;

    [Tooltip("Arrossega aquí el Transform del jugador (Dino verd)")]
    public Transform OpponentTarget;

    [Header("Ajustes de IA")]
    public float DecisionInterval = 0.1f;
    public LayerMask ladderLayer;
    public float ladderCheckRadius = 0.3f;

    [Header("Modelos IA (.onnx)")]
    public ModelAsset catcherModel;
    public ModelAsset evaderModel;
    private BehaviorParameters _behaviorParameters;
    private bool? _wasCatcher = null;

    private float _nextDecisionTime;
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInput;
    private bool _isNearLadder;
    private Rigidbody2D _rb;

    public override void Initialize()
    {
        _behaviorParameters = GetComponent<BehaviorParameters>();
        _rb = GetComponent<Rigidbody2D>();
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

        bool isCatcher = (GameState.CurrentBombOwner == gameObject);
        if (_wasCatcher == null || isCatcher != _wasCatcher)
        {
            _wasCatcher = isCatcher;
            if (_behaviorParameters != null)
            {
                if (isCatcher && catcherModel != null)
                {
                    _behaviorParameters.Model = catcherModel;
                }
                else if (!isCatcher && evaderModel != null)
                {
                    _behaviorParameters.Model = evaderModel;
                }
            }
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
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y);

        bool hasBomb = (GameState.CurrentBombOwner == gameObject);
        sensor.AddObservation(hasBomb ? 1f : 0f);

        Vector2 checkPosition = (Vector2)transform.position + (Vector2.up * 0.5f);
        _isNearLadder = Physics2D.OverlapCircle(checkPosition, ladderCheckRadius, ladderLayer);
        sensor.AddObservation(_isNearLadder ? 1f : 0f);

        if (OpponentTarget != null)
        {
            Vector2 relativePos = (Vector2)OpponentTarget.position - (Vector2)transform.position;
            sensor.AddObservation(relativePos.x);
            sensor.AddObservation(relativePos.y);
        }
        else
        {

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

        if (GameState.CurrentBombOwner == gameObject)
        {

            AddReward(-0.001f);
        }
        else
        {

            AddReward(0.001f);
        }

        if (OpponentTarget != null && Controller != null && Controller.isClimbing)
        {
            float yDiff = OpponentTarget.position.y - transform.position.y;
            float vSpeed = _rb != null ? _rb.linearVelocity.y : 0f;

            if (yDiff > 0.2f && verticalAction == 1 && vSpeed > 0.01f)
            {

                AddReward(0.01f * vSpeed);
            }
            else if (yDiff < -0.2f && verticalAction == 2 && vSpeed < -0.01f)
            {

                AddReward(0.01f * Mathf.Abs(vSpeed));
            }
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
        Gizmos.color = Color.yellow;
        Vector2 checkPos = (Vector2)transform.position + (Vector2.up * 0.5f);
        Gizmos.DrawWireSphere(checkPos, ladderCheckRadius);
    }
}
