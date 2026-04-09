using UnityEngine;
using System.Collections.Generic;

public class Bomb : MonoBehaviour
{
    public GameStateSO GameState;
    public float TransferCooldown = 1.0f;
    public float SpeedMultiplier = 1.15f;
    public Vector3 Offset = new Vector3(0, 1.5f, 0);

    // Referència visual de la bomba sobre el cap
    public GameObject BombIndicatorPrefab;
    private Dictionary<GameObject, GameObject> _indicators = new Dictionary<GameObject, GameObject>();
    private float _lastTransferTime;
    private GameManager _gameManager;

    void Start()
    {
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

            // Feedback visual: escala segons el temps
            // Creixem un 50% extra quan estem a punt d'explotar
            float t = 1f - (GameState.GameTimer / GameState.InitialTimer);
            float scale = 1f + (t * 0.5f); 
            
            // Si queden menys de 3 segons, vibració/bateg ràpid
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
        
        // Ignorar espectadors
        if (GameState.Spectators.Contains(other.gameObject.name)) return;

        // Si l'entitat que ens toca NO és el propietari actual
        if (other.CompareTag("Player") || other.CompareTag("Bot")) // Assumint tags
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
        // 1. Activar indicador visual (bola sobre el cap)
        if (BombIndicatorPrefab != null)
        {
            GameObject indicator = Instantiate(BombIndicatorPrefab, owner.transform);
            indicator.transform.localPosition = Offset;
            _indicators[owner] = indicator;
        }

        // 2. Aplicar outline vermell (si tenen SpriteRenderer)
        var sr = owner.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Per simplicitat, assumim que el shader té una propietat _OutlineColor o similar
            // O simplement canviem el color del material si és un shader d'outline
            sr.material.SetColor("_OutlineColor", Color.red);
        }

        // 3. Multiplicar velocitat
        // Intentem trobar el controlador de moviment (Player o Bot)
        var playerCtrl = owner.GetComponent<PlayerModeController2D>();
        if (playerCtrl != null) playerCtrl.ApplySpeedMultiplier(SpeedMultiplier);
        
        var botCtrl = owner.GetComponent<BotController>();
        if (botCtrl != null) botCtrl.ApplySpeedMultiplier(SpeedMultiplier);
    }

    private void RemoveBombState(GameObject owner)
    {
        // 1. Destruir indicador
        if (_indicators.ContainsKey(owner))
        {
            Destroy(_indicators[owner]);
            _indicators.Remove(owner);
        }

        // 2. Tornar outline a negre
        var sr = owner.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.material.SetColor("_OutlineColor", Color.black);
        }

        // 3. Reset velocitat
        var playerCtrl = owner.GetComponent<PlayerModeController2D>();
        if (playerCtrl != null) playerCtrl.ResetSpeedMultiplier();

        var botCtrl = owner.GetComponent<BotController>();
        if (botCtrl != null) botCtrl.ResetSpeedMultiplier();
    }
}
