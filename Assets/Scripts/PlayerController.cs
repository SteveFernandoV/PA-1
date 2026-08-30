using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador principal del jugador (Misión 3 - Hito 1).
/// Gestiona movimiento responsivo, salto con detección física de suelo y Game Feel.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Parámetros de Movimiento (Game Feel)")]
    [Tooltip("Velocidad horizontal calibrada y controlable")]
    [SerializeField] private float speed = 3.8f;

    [Tooltip("Fuerza del impulso vertical al saltar")]
    [SerializeField] private float jumpForce = 6.2f;

    [Header("Detección de Suelo")]
    [Tooltip("Punto de referencia en los pies del personaje para chequear contacto")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Radio del círculo de detección de suelo")]
    [SerializeField] private float groundRadius = 0.15f;

    [Tooltip("Capa que define qué objetos son suelo caminable")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Interfaz de Usuario")]
    [Tooltip("Texto en pantalla para el contador de monedas")]
    [SerializeField] private TMP_Text TextCoins;

    // Componentes internos
    private Rigidbody2D rb2D;
    private Animator animator;

    // Variables de estado
    private float move;
    private bool isGrounded;
    private int coins = 0;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. Lectura de entrada horizontal con GetAxisRaw para evitar aceleración/desaceleración artificial
        move = Input.GetAxisRaw("Horizontal");

        // 2. Aplicación directa de velocidad horizontal manteniendo la velocidad vertical actual
        rb2D.linearVelocity = new Vector2(move * speed, rb2D.linearVelocity.y);

        // 3. Volteo (Flip) del sprite según la dirección de avance
        if (move != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);
        }

        // 4. Salto: Solo se ejecuta si se presiona el botón y el personaje está tocando el suelo (evita salto infinito)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
        }

        // 5. Sincronización de animaciones con el estado físico
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(move));
            animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    void FixedUpdate()
    {
        // Comprobación de suelo en el ciclo de física mediante un círculo de colisión
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Recolección de coleccionable (Moneda con Trigger)
        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coins++;
            if (TextCoins != null)
            {
                TextCoins.text = coins.ToString();
            }
        }

        // Trampa mortal de pinchos: Reinicia la escena
        if (collision.CompareTag("Spikes"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Obstáculo destructible (Bomba/Barril) con knockback y explosión
        if (collision.CompareTag("Barrel") || collision.CompareTag("Bomb"))
        {
            Vector2 knockbackDir = (rb2D.position - (Vector2)collision.transform.position).normalized;
            rb2D.linearVelocity = Vector2.zero;
            rb2D.AddForce(knockbackDir * 3f, ForceMode2D.Impulse);

            BoxCollider2D[] colliders = collision.gameObject.GetComponents<BoxCollider2D>();
            foreach (BoxCollider2D col in colliders)
            {
                col.enabled = false;
            }

            if (collision.TryGetComponent(out Animator barrelAnim))
            {
                barrelAnim.enabled = true;
            }

            Destroy(collision.gameObject, 0.5f);
        }
    }

    // Dibuja el radio de detección de suelo en la vista de escena de Unity para facilitar el ajuste
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
