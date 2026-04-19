using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class Bomb : MonoBehaviour
{
    public GameStateSO GameState;
    public float TransferCooldown = 1.0f;
    
    [Tooltip("Multiplicador de velocitat (ex: 1.2 = 20% més ràpid)")]
    public float SpeedMultiplier = 1.2f;
    
    public Vector3 Offset = new Vector3(0, 1.5f, 0);

    [Header("Animation Settings")]
    public float criticalTimeThreshold = 3.0f; 

    public GameObject BombIndicatorPrefab;
    private Dictionary<GameObject, GameObject> _indicators = new Dictionary<GameObject, GameObject>();
    private float _lastTransferTime;
    private GameManager _gameManager;
    
    private Animator _animator;
    private int _currentStage = 0;
    private bool _hasExploded = false; // Pestillo anti-spam d'explosió

    void Start()
    {
        _animator = GetComponent<Animator>();
        _gameManager = FindFirstObjectByType<GameManager>();
        
        // Si ja hi ha un amo a l'inici, ens col·loquem a sobre
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
            // La bomba segueix al propietari
            transform.position = GameState.CurrentBombOwner.transform.position + Offset;

            UpdateAnimationStage();

            // Feedback visual d'escala
            float t = 1f - (GameState.GameTimer / GameState.InitialTimer);
            float scale = 1f + (t * 0.5f); 
            
            if (GameState.GameTimer < 3f)
            {
                scale += Mathf.Sin(Time.time * 25f) * 0.15f;
            }
            transform.localScale = new Vector3(scale, scale, 1f);
            
            if (GameState.GameTimer <= 0 && !_hasExploded)
            {
                _hasExploded = true; // Tanquem el pestillo: només explota UNA vegada
                Explode();
            }
        }
    }

    private void UpdateAnimationStage()
    {
        int newStage = 0; 
        if (GameState.GameTimer <= 0) newStage = 2; 
        else if (GameState.GameTimer <= criticalTimeThreshold) newStage = 1; 

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
        
        if (_gameManager != null) _gameManager.HandleDeath(loser);
    }

    // Aquest mètode es crida des de 'TagCollision.cs' en xocar els cossos
    public void TransferTo(GameObject newOwner)
    {
        if (GameState.CurrentBombOwner != null && Time.time - _lastTransferTime < TransferCooldown) return;

        if (GameState.CurrentBombOwner != null)
        {
            RemoveBombState(GameState.CurrentBombOwner);
        }

        GameState.SetBombOwner(newOwner);
        ApplyBombState(newOwner);
        _lastTransferTime = Time.time;
        _hasExploded = false; // Resetem el pestillo perquè pugui tornar a explotar
        Debug.Log($"Bomba transferida a {newOwner.name}");
    }

    private void ApplyBombState(GameObject owner)
    {
        // --- CÓDIGO NUEVO (Para limpiar indicadores viejos) ---
        // 1. Limpiamos si ya tenía uno
        if (_indicators.ContainsKey(owner))
        {
            Destroy(_indicators[owner]);
            _indicators.Remove(owner);
        }
        
        // 1.5. Efecte visual (Indicador sobre el cap)
        if (BombIndicatorPrefab != null)
        {
            GameObject indicator = Instantiate(BombIndicatorPrefab, owner.transform);
            indicator.transform.localPosition = Offset;
            _indicators[owner] = indicator;
        }

        // 2. Canvi de color (Outline vermell)
        var sr = owner.GetComponent<SpriteRenderer>();
        if (sr != null) sr.material.SetColor("_OutlineColor", Color.red);

        // 3. AUGMENT DE VELOCITAT (Molt important per al gameplay)
        var playerCtrl = owner.GetComponent<PlayerModeController2D>();
        if (playerCtrl != null) playerCtrl.ApplySpeedMultiplier(SpeedMultiplier);
        
        var botCtrl = owner.GetComponent<BotController>();
        if (botCtrl != null) botCtrl.ApplySpeedMultiplier(SpeedMultiplier);
    }

    private void RemoveBombState(GameObject owner)
    {
        // 1. Treure indicador
        if (_indicators.ContainsKey(owner))
        {
            Destroy(_indicators[owner]);
            _indicators.Remove(owner);
        }

        // 2. Restaurar color
        var sr = owner.GetComponent<SpriteRenderer>();
        if (sr != null) sr.material.SetColor("_OutlineColor", Color.black);

        // 3. RESTAURAR VELOCITAT NORMAL
        var playerCtrl = owner.GetComponent<PlayerModeController2D>();
        if (playerCtrl != null) playerCtrl.ResetSpeedMultiplier();

        var botCtrl = owner.GetComponent<BotController>();
        if (botCtrl != null) botCtrl.ResetSpeedMultiplier();
    }
}