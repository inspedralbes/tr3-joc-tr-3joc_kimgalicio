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
    
    [Tooltip("Arrossega aquí el Transform del jugador (Dino verd)")]
    public Transform OpponentTarget; 

    // --- AÑADE ESTAS DOS LÍNEAS ---
    [Header("Spawns d'Entrenament")]
    public Transform Spawn1;
    public Transform Spawn2;

    [Header("Ajustes de IA")]
    public float DecisionInterval = 0.1f;
    public LayerMask ladderLayer;           // <-- Vuelve a poner esto
    public float ladderCheckRadius = 0.3f;  // <-- Vuelve a poner esto

    private float _nextDecisionTime;
    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpInput;
    private bool _isNearLadder;
    private Rigidbody2D _rb;

    // --- INICIALITZACIÓ ---

    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (Controller == null) Controller = GetComponent<PlayerModeController2D>();
        if (GameState != null) 
        {
            // AÑADIMOS ESTA LÍNEA para quitar el "Game Over" perpetuo
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

    // Es crida automàticament cada cop que falla o encerta per reiniciar ràpid
    public override void OnEpisodeBegin()
    {
        // 1. Resetejar inputs
        _horizontalInput = 0f;
        _verticalInput = 0f;
        _jumpInput = false;

        // 2. Colocar en los Spawns seguros
        if (Spawn1 != null && Spawn2 != null)
        {
            // Tiramos una moneda (50% de probabilidad) para intercambiarlos de lado
            // Así la IA no memoriza que siempre sale en el mismo sitio
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

        // 3. FASE 1 ENTRENAMENT: Forçar que el Bot tingui la bomba inicialment
        if (GameState != null)
        {
            // TRUCO: Le quitamos la bomba a todo el mundo antes de dársela al bot.
            // Así la bomba no cree que se ha hecho un "pase" y no rompe el bucle.
            GameState.SetBombOwner(null); 
            
            Bomb bomb = FindFirstObjectByType<Bomb>();
            if (bomb != null)
            {
                bomb.TransferTo(this.gameObject);
                // Restauramos el temporizador para que la bomba vuelva a su tamaño normal
                GameState.GameTimer = GameState.InitialTimer; 
            }
        }
    }

    // --- OBSERVACIONS (El que veu el cervell de la IA) ---

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Posició pròpia (X, Y) [Total: 2]
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y);

        // 2. Estat de la bomba: La tinc jo? (1 = Sí, 0 = No) [Total: 1]
        bool hasBomb = (GameState.CurrentBombOwner == gameObject);
        sensor.AddObservation(hasBomb ? 1f : 0f);

        // 3. Detecció d'escaleres [Total: 1]
        // Añadimos un offset vertical para que el radar se centre en el torso/cabeza y no en los pies
        Vector2 checkPosition = (Vector2)transform.position + (Vector2.up * 0.5f);
        _isNearLadder = Physics2D.OverlapCircle(checkPosition, ladderCheckRadius, ladderLayer);
        sensor.AddObservation(_isNearLadder ? 1f : 0f);

        // 4. Posició relativa del jugador respecte al Bot [Total: 2]
        if (OpponentTarget != null)
        {
            Vector2 relativePos = (Vector2)OpponentTarget.position - (Vector2)transform.position;
            sensor.AddObservation(relativePos.x);
            sensor.AddObservation(relativePos.y);
        }
        else 
        {
            // Enviem 0 si no hi ha objectiu, l'important és no canviar la quantitat d'observacions
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
        
        // TOTAL D'OBSERVACIONS ENVIADES: 6
    }

    // --- ACCIONS (El que decideix fer la IA) ---
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

        // --- SISTEMA DE RECOMPENSES (FASE 1: PERSEGUIR) ---
        if (GameState.CurrentBombOwner == gameObject)
        {
            // Càstig lleuger constant. Obliga al bot a afanyar-se a passar la bomba!
            AddReward(-0.001f); 
        }
        else
        {
            // Si no la té (significa que l'ha passat bé), petit premi per mantenir-se viu.
            AddReward(0.001f);
        }

        // --- NUEVO: SISTEMA DE RECOMPENSAS DINÁMICAS (DIRECCIÓN + VELOCIDAD) ---
        if (OpponentTarget != null && Controller != null && Controller.isClimbing)
        {
            float yDiff = OpponentTarget.position.y - transform.position.y;
            float vSpeed = _rb != null ? _rb.linearVelocity.y : 0f;

            // Premiamos si el bot se mueve en la dirección correcta respecto al objetivo
            // Rebajamos el umbral de altura a 0.2f y el de velocidad a 0.01f para mantener el estímulo constante
            if (yDiff > 0.2f && verticalAction == 1 && vSpeed > 0.01f)
            {
                // Objetivo está arriba, el bot sube -> Recompensa proporcional a la velocidad
                AddReward(0.01f * vSpeed); 
            }
            else if (yDiff < -0.2f && verticalAction == 2 && vSpeed < -0.01f)
            {
                // Objetivo está abajo, el bot baja -> Recompensa proporcional a la velocidad de descenso
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

    // --- ES CRIDA DES DE BOMB.CS QUAN XOCA AMB ÈXIT ---

    public void OnTaggedTarget()
    {
        // Gran premi final per aconseguir l'objectiu de la Fase 1!
        AddReward(5.0f); 
        
        // Reseteja el mapa a l'instant per a la següent ronda d'entrenament
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
        Vector2 checkPos = (Vector2)transform.position + (Vector2.up * 0.5f);
        Gizmos.DrawWireSphere(checkPos, ladderCheckRadius);
    }
}