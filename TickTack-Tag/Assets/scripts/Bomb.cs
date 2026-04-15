using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class Bomb : MonoBehaviour
{
    public GameStateSO GameState;
    public float TransferCooldown = 1.0f;
    public float SpeedMultiplier = 1.15f;
    public Vector3 Offset = new Vector3(0, 1.5f, 0);

    [Header("Animation Settings")]
    public float criticalTimeThreshold = 3.0f; // Tiempo para cambiar a clip "Critical"

    // Referència visual de la bomba sobre el cap
    public GameObject BombIndicatorPrefab;
    private Dictionary<GameObject, GameObject> _indicators = new Dictionary<GameObject, GameObject>();
    private float _lastTransferTime;
    private GameManager _gameManager;
    
    private Animator _animator;
    private int _currentStage = 0; // 0: Idle, 1: Critical, 2: Explosion

    void Start()
    {
        _animator = GetComponent<Animator>();
        _gameManager = FindFirstObjectByType<GameManager>();
        
        if (GameState != null && GameState.CurrentBombOwner != null)
        {
            ApplyBombState(GameState.CurrentBombOwner);
        }
    }

    void Update()
    {
        if (GameState == null || GameState.GameOver) return;

        if (GameState.CurrentBombOwner != null)
        {
            // Seguir al propietari
            transform.position = GameState.CurrentBombOwner.transform.position + Offset;

            // --- Lógica de Animación ---
            UpdateAnimationStage();

            // Feedback visual: escala segons el temps
            float t = 1f - (GameState.GameTimer / GameState.InitialTimer);
            float scale = 1f + (t * 0.5f); 
            
            if (GameState.GameTimer < 3f)
            {
                scale += Mathf.Sin(Time.time * 25f) * 0.15f;
            }
            transform.localScale = new Vector3(scale, scale, 1f);
            
            if (GameState.GameTimer <= 0)
            {
                Explode();
            }
        }
    }

    private void UpdateAnimationStage()
    {
        int newStage = 0; // Idle por defecto

        if (GameState.GameTimer <= 0)
        {
            newStage = 2; // Explosion
        }
        else if (GameState.GameTimer <= criticalTimeThreshold)
        {
            newStage = 1; // Critical
        }

        if (newStage != _currentStage)
        {
            _currentStage = newStage;
            _animator.SetInteger("BombStage", _currentStage);
        }
    }

    private void Explode()
    {
        if (GameState.GameOver) return;
        
        GameObject loser = GameState.CurrentBombOwner;
        Debug.Log($"BOMBA EXPLOTA! {loser.name} perd una vida.");
        
        // El GameManager s'encarregarà de la resta (restar vida, respawn, etc.)
        if (_gameManager != null) _gameManager.HandleDeath(loser);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GameState == null || GameState.GameOver) return;
        if (GameState.Spectators.Contains(other.gameObject.name)) return;

        if (other.CompareTag("Player") || other.CompareTag("Bot"))
        {
            if (other.gameObject != GameState.CurrentBombOwner)
            {
                if (Time.time - _lastTransferTime < TransferCooldown) return;
                TransferTo(other.gameObject);
            }
        }
    }

    public void TransferTo(GameObject newOwner)
    {
        if (GameState.CurrentBombOwner != null)
        {
            RemoveBombState(GameState.CurrentBombOwner);
        }

        GameState.SetBombOwner(newOwner);
        ApplyBombState(newOwner);
        _lastTransferTime = Time.time;
        Debug.Log($"Bomba transferida a {newOwner.name}");
    }

    private void ApplyBombState(GameObject owner)
    {
        if (BombIndicatorPrefab != null)
        {
            GameObject indicator = Instantiate(BombIndicatorPrefab, owner.transform);
            indicator.transform.localPosition = Offset;
            _indicators[owner] = indicator;
        }

        var sr = owner.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.material.SetColor("_OutlineColor", Color.red);
        }

        var playerCtrl = owner.GetComponent<PlayerModeController2D>();
        if (playerCtrl != null) playerCtrl.ApplySpeedMultiplier(SpeedMultiplier);
        
        var botCtrl = owner.GetComponent<BotController>();
        if (botCtrl != null) botCtrl.ApplySpeedMultiplier(SpeedMultiplier);
    }

    private void RemoveBombState(GameObject owner)
    {
        if (_indicators.ContainsKey(owner))
        {
            Destroy(_indicators[owner]);
            _indicators.Remove(owner);
        }

        var sr = owner.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.material.SetColor("_OutlineColor", Color.black);
        }

        var playerCtrl = owner.GetComponent<PlayerModeController2D>();
        if (playerCtrl != null) playerCtrl.ResetSpeedMultiplier();

        var botCtrl = owner.GetComponent<BotController>();
        if (botCtrl != null) botCtrl.ResetSpeedMultiplier();
    }
}