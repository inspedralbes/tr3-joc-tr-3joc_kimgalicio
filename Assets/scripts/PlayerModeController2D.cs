using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerModeController2D : MonoBehaviour
{
    public enum GameMode { TopDown, Platformer }

    [Header("Mode")]
    [SerializeField] private GameMode mode = GameMode.TopDown;


    [Header("Movement")]
    [SerializeField] private float topDownSpeed = 4f;
    [SerializeField] private float platformerSpeed = 6f;

    [Header("Jump (Platformer only)")]
    [SerializeField] private float jumpImpulse = 9f;
    [SerializeField] private Transform groundCheck;          // un empty al peu del player
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;          // assigna World

    [Header("Animator Reference")]
    [SerializeField] private Animator animator ;


    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ApplyModeSettings();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Toggle amb tecla 0 (Alpha0 és la tecla 0 del teclat normal)
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            mode = (mode == GameMode.TopDown) ? GameMode.Platformer : GameMode.TopDown;
            ApplyModeSettings();
        }

        // Input WASD / fletxes
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Salt només en mode Platformer
        if (mode == GameMode.Platformer && Input.GetKeyDown(KeyCode.Space))
        {
            if (IsGrounded())
             {
                 // Reset vertical per un salt consistent (evita salts “tristos” si estàs caient)
                 rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                 rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
             }
        }

        animator.SetFloat("x", input.normalized.x);
        animator.SetFloat("y", input.normalized.y);
        animator.SetFloat("speed", Mathf.Abs(input.normalized.x) + Mathf.Abs(input.normalized.y));

    }
    private void FixedUpdate()
    {
        if (mode == GameMode.TopDown)
        {
            // TopDown: ignorem la gravetat i ens movem en 2D (X,Y)
            Vector2 move = input.normalized * topDownSpeed;
            Vector2 nextPos = rb.position + move * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);
        }
        else
        {
            // Platformer: moviment només horitzontal, la Y la mana la gravetat + salt
            float x = input.x; // només esquerda/dreta
            rb.linearVelocity = new Vector2(x * platformerSpeed, rb.linearVelocity.y);
        }
    }
    private void ApplyModeSettings()
    {
        if (mode == GameMode.TopDown)
        {
            rb.gravityScale = 0f;
            // Evitem que la física “s’escapi” en Z i rotacions ridícules
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Opcional: parar inèrcies quan canvies de mode
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Quan entres al mode plataforma, la Y ja la farà la gravetat
            // No toquem la posició, només netegem rotació/ang vel.
            rb.angularVelocity = 0f;
        }

        Debug.Log($"Mode canviat a: {mode}");
    }
    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

#if UNITY_EDITOR
    //private void OnDrawGizmosSelected()
    private void OnDrawGizmos()
    {

    if (groundCheck == null) return;

    Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    Gizmos.color = (hit != null) ? Color.green : Color.red;
    Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

    if (hit != null)
    {
     UnityEngine.Debug.Log($"Gizmo hit: {hit.name} (layer={LayerMask.LayerToName(hit.gameObject.layer)}) trigger={hit.isTrigger}");
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(groundCheck.position, hit.bounds.center);
    }

    }
#endif
}
