using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BotController : Agent
{
    [Header("Configuració de Referències")]
    public GameStateSO GameState;
    public PlayerModeController2D Controller;
    public LayerMask ladderLayer;

    [Header("Ajustes de IA")]
    public float DecisionInterval = 0.1f;
    public float ladderCheckRadius = 0.3f;

    private float _nextDecisionTime;
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInput;
    private bool _isNearLadder;

    // --- INICIALIZACIÓ ---

    public override void Initialize()
    {
        // Si no está asignado en el inspector, lo busca en el mismo objeto
        if (Controller == null) Controller = GetComponent<PlayerModeController2D>();
        
        // Inicializamos la entidad en el GameState para tener vidas
        if (GameState != null) GameState.InitializeEntity(gameObject.name);
    }

    void Update()
    {
        if (GameState == null || GameState.GameOver || Controller == null) return;
        
        // Si sóc espectador (mort), no faig res i em quedo quiet
        if (GameState.Spectators.Contains(gameObject.name))
        {
            _horizontalInput = 0f;
            _verticalInput = 0f;
            _jumpInput = false;
            Controller.SetInput(0, 0, false);
            return;
        }

        // Sol·licitar decisió al "Cerebro" de ML-Agents segons l'interval
        if (Time.time >= _nextDecisionTime)
        {
            RequestDecision();
            _nextDecisionTime = Time.time + DecisionInterval;
        }

        // Enviem els inputs calculats per la IA al controlador físic
        Controller.SetInput(_horizontalInput, _verticalInput, _jumpInput);
        
        // Reset del salt per evitar que es quedi polsat en el següent frame físic
        _jumpInput = false;
    }

    public override void OnEpisodeBegin()
    {
        // Reset d'estat del bot al començar un nou episodi d'entrenament
        _horizontalInput = 0f;
        _verticalInput = 0f;
        _jumpInput = false;
    }

    // --- OBSERVACIONS (El que el Bot percep del món) ---

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Posició pròpia (X, Y) - 2 observacions
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y);

        // 2. Estat de la bomba: Qui la té? - 1 observació
        bool hasBomb = (GameState.CurrentBombOwner == gameObject);
        sensor.AddObservation(hasBomb ? 1f : 0f);

        // 3. Detecció d'escaleres: Estic a sobre d'una? - 1 observació
        _isNearLadder = Physics2D.OverlapCircle(transform.position, ladderCheckRadius, ladderLayer);
        sensor.AddObservation(_isNearLadder ? 1f : 0f);

        // 4. Distància al propietari actual de la bomba - 1 observació
        if (GameState.CurrentBombOwner != null)
        {
            float distToBomb = Vector3.Distance(transform.position, GameState.CurrentBombOwner.transform.position);
            sensor.AddObservation(distToBomb);
        }
        else
        {
            sensor.AddObservation(0f);
        }

        // 5. Distàncies i posicions relatives a tots els altres jugadors/bots vius
        // NOTA: Para entrenamiento estable, el número de observaciones debe ser FIJO.
        // Aquí buscamos los tags, pero lo ideal es que siempre observes al mismo número de oponentes.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] bots = GameObject.FindGameObjectsWithTag("Bot");
        
        List<GameObject> allTargets = new List<GameObject>(players);
        allTargets.AddRange(bots);
        
        foreach (var t in allTargets)
        {
            if (t == gameObject) continue;
            if (GameState.Spectators.Contains(t.name)) continue;

            // Enviem la posició relativa (direcció i distància) - 2 observacions per cada objectiu
            Vector2 relativePos = (Vector2)t.transform.position - (Vector2)transform.position;
            sensor.AddObservation(relativePos.x);
            sensor.AddObservation(relativePos.y);
        }
    }

    // --- ACCIONS (El que el Bot decideix fer) ---

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Acció Contínua 0: Moviment horitzontal (-1 a 1)
        _horizontalInput = actions.ContinuousActions[0];

        // Acció Discreta Branch 0: Salt (0: No, 1: Sí)
        _jumpInput = actions.DiscreteActions[0] == 1;

        // Acció Discreta Branch 1: Moviment Vertical / Escaleres (0: Res, 1: Amunt, 2: Avall)
        int verticalAction = actions.DiscreteActions[1];
        if (verticalAction == 1) _verticalInput = 1f;
        else if (verticalAction == 2) _verticalInput = -1f;
        else _verticalInput = 0f;

        // --- SISTEMA DE RECOMPENSES ---

        if (GameState.CurrentBombOwner == gameObject)
        {
            // FASE 1: Perseguir. Recompensa por estar vivo con la bomba (incita a pasarla rápido)
            // O una pequeña recompensa por acercarse a otros.
            AddReward(0.001f); 
        }
        else
        {
            // FASE 2: Fugir. Recompensa por sobrevivir sin la bomba.
            AddReward(0.002f);
        }
    }

    // --- MÈTODE HEURÍSTIC (Control manual para testeo) ---

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        // Moviment horitzontal
        continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
        
        // Salt
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        // Escaleres
        float v = Input.GetAxisRaw("Vertical");
        if (v > 0) discreteActionsOut[1] = 1;      // Amunt
        else if (v < 0) discreteActionsOut[1] = 2; // Avall
        else discreteActionsOut[1] = 0;           // Quiet
    }

    // --- FUNCIONS DE SUPORT I INTEGRACIÓ ---

    public void OnTaggedTarget()
    {
        // Se llama desde Bomb.cs cuando este Bot le pasa la bomba a otro con éxito
        AddReward(5.0f); 
        EndEpisode();    
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
        Gizmos.DrawWireSphere(transform.position, ladderCheckRadius);
    }
}