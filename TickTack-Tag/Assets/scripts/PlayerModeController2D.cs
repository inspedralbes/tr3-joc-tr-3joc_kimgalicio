using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerModeController2D : MonoBehaviour
{
    public enum GameMode { TopDown, Platformer }

    [Header("Settings")]
    public GameStateSO GameState;

    [Header("Mode")]
    [SerializeField] private GameMode mode = GameMode.Platformer;
    [SerializeField] public bool useAiInput = false;

    [Header("Movement")]
    [SerializeField] private float topDownSpeed = 4f;
    [SerializeField] private float platformerSpeed = 6f;
    private float _currentSpeedMultiplier = 1f;

    [Header("Jump (Platformer only)")]
    [SerializeField] private float jumpImpulse = 9f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animator & Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private string _currentAnimState;
    private const string ANIM_IDLE = "Player_Vita";
    private const string ANIM_WALK = "Vita_Walk";
    private const string ANIM_JUMP = "Vita_Jump";
    private const string ANIM_FALL = "Vita_Fall";
    private const string ANIM_DAMAGE = "Vita_Damage";

    private Rigidbody2D rb;
    private Vector2 input;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ApplyModeSettings();
        if (GameState != null) GameState.InitializeEntity(gameObject.name);
    }

    void Update()
    {
        if (GameState != null && (GameState.GameOver || GameState.Spectators.Contains(gameObject.name)))
        {
            input = Vector2.zero;
            jumpRequested = false;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!useAiInput)
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (mode == GameMode.Platformer && Input.GetKeyDown(KeyCode.Space))
            {
                jumpRequested = true;
            }
        }

        if (animator != null)
        {
            UpdateAnimations();
        }
    }

    private void UpdateAnimations()
    {
        string newState;

        if (!IsGrounded())
        {
            if (rb.linearVelocity.y > 0.1f)
                newState = ANIM_JUMP;
            else if (rb.linearVelocity.y < -0.1f)
                newState = ANIM_FALL;
            else
                newState = _currentAnimState; // Mantener el anterior si estamos en el ápice
        }
        else
        {
            if (Mathf.Abs(input.x) > 0.01f || (mode == GameMode.TopDown && Mathf.Abs(input.y) > 0.01f))
                newState = ANIM_WALK;
            else
                newState = ANIM_IDLE;
        }

        ChangeAnimationState(newState);

        // Girar el personaje
        if (spriteRenderer != null && input.x != 0)
        {
            spriteRenderer.flipX = input.x < 0;
        }
    }

    private void ChangeAnimationState(string newState)
    {
        if (_currentAnimState == newState) return;

        animator.Play(newState);
        _currentAnimState = newState;
    }

    public void TriggerDamage()
    {
        if (animator != null)
        {
            ChangeAnimationState(ANIM_DAMAGE);
            // Opcional: Podríamos volver a idle después de un tiempo si no hay más inputs, 
            // pero el estado se actualizará en el próximo Update() basándose en el movimiento.
        }
    }

    private void FixedUpdate()
    {
        if (GameState != null && (GameState.GameOver || GameState.Spectators.Contains(gameObject.name))) return;

        float effectiveSpeed = (mode == GameMode.TopDown ? topDownSpeed : platformerSpeed) * _currentSpeedMultiplier;

        if (mode == GameMode.TopDown)
        {
            Vector2 move = input.normalized * effectiveSpeed;
            rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = new Vector2(input.x * effectiveSpeed, rb.linearVelocity.y);

            if (jumpRequested)
            {
                if (IsGrounded())
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
                }
                jumpRequested = false;
            }
        }
    }

    public void ApplySpeedMultiplier(float multiplier)
    {
        _currentSpeedMultiplier = multiplier;
    }

    public void ResetSpeedMultiplier()
    {
        _currentSpeedMultiplier = 1f;
    }

    public void SetInput(float horizontal, float vertical, bool jump)
    {
        if (!useAiInput) return;
        input = new Vector2(horizontal, vertical);
        if (jump) jumpRequested = true;
    }

    private void ApplyModeSettings()
    {
        rb.gravityScale = (mode == GameMode.TopDown) ? 0f : 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
