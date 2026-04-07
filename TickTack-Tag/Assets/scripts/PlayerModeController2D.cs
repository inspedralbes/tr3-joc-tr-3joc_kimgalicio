using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerModeController2D : MonoBehaviour
{
    public enum GameMode { TopDown, Platformer }

    [Header("Settings")]
    public GameStateSO GameState;

    [Header("Mode")]
    [SerializeField] private GameMode mode = GameMode.Platformer;
    [SerializeField] private bool useAiInput = false;

    [Header("Movement")]
    [SerializeField] private float topDownSpeed = 4f;
    [SerializeField] private float platformerSpeed = 6f;

    [Header("Jump (Platformer only)")]
    [SerializeField] private float jumpImpulse = 9f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animator Reference")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 input;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ApplyModeSettings();
    }

    void Update()
    {
        if (GameState != null && GameState.GameOver)
        {
            input = Vector2.zero;
            jumpRequested = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (!useAiInput)
        {
            // Input WASD / fletxes
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (mode == GameMode.Platformer && Input.GetKeyDown(KeyCode.Space))
            {
                jumpRequested = true;
            }

            // Toggle mode with '0' for debugging
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                mode = (mode == GameMode.TopDown) ? GameMode.Platformer : GameMode.TopDown;
                ApplyModeSettings();
            }
        }

        if (animator != null)
        {
            animator.SetFloat("x", input.normalized.x);
            animator.SetFloat("y", input.normalized.y);
            animator.SetFloat("speed", Mathf.Abs(input.normalized.x) + Mathf.Abs(input.normalized.y));
        }
    }

    private void FixedUpdate()
    {
        if (mode == GameMode.TopDown)
        {
            Vector2 move = input.normalized * topDownSpeed;
            Vector2 nextPos = rb.position + move * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);
        }
        else
        {
            float x = input.x;
            rb.linearVelocity = new Vector2(x * platformerSpeed, rb.linearVelocity.y);

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

    public void SetInput(float horizontal, float vertical, bool jump)
    {
        if (!useAiInput) return;
        input = new Vector2(horizontal, vertical);
        if (jump) jumpRequested = true;
    }

    private void ApplyModeSettings()
    {
        if (mode == GameMode.TopDown)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.angularVelocity = 0f;
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        Gizmos.color = (hit != null) ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
#endif
}
