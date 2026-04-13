using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AntiGravityEffect : MonoBehaviour
{
    [Header("Anti-Gravity Settings")]
    [Tooltip("Fuerza aplicada hacia arriba. Debe ser suficiente para contrarrestar la gravedad.")]
    public float antiGravityForce = 15f;
    
    [Tooltip("Rozamiento lineal (drag) mientras la antigravedad está activa para suavizar el movimiento.")]
    public float antiGravityDrag = 5f;
    
    [Tooltip("Velocidad máxima permitida hacia arriba.")]
    public float terminalVelocity = 5f;

    [Header("State")]
    public bool isAntiGravityActive = false;

    private Rigidbody2D rb;
    private float originalDrag;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalDrag = rb.linearDamping;
    }

    private void Update()
    {
        // Detectar input
        isAntiGravityActive = Input.GetKey(KeyCode.Space);

        // Ajustar el drag dinámicamente
        if (isAntiGravityActive)
        {
            rb.linearDamping = antiGravityDrag;
        }
        else
        {
            rb.linearDamping = originalDrag;
        }
    }

    private void FixedUpdate()
    {
        if (isAntiGravityActive)
        {
            // Aplicar fuerza constante hacia arriba
            rb.AddForce(Vector2.up * antiGravityForce, ForceMode2D.Force);

            // Limitar la velocidad máxima (Terminal Velocity)
            if (rb.linearVelocity.y > terminalVelocity)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, terminalVelocity);
            }
        }
    }
}
