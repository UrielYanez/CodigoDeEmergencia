using System;
using UnityEngine;

namespace CodigoDeEmergencia.Player
{
    /// <summary>
    /// Controlador principal de movimiento 2D y físicas del protagonista.
    /// Resuelve el Issue #01 (Sprint 1) cumpliendo con:
    /// - Movimiento horizontal fluido y configurable.
    /// - Salto con Coyote Time y Jump Buffering (game-feel responsivo).
    /// - Jump cut (salto variable al soltar el botón).
    /// - Detección precisa de suelo mediante OverlapCircle/BoxCast.
    /// - Orientación y volteo horizontal (Facing direction).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("--- Movimiento Horizontal ---")]
        [Tooltip("Velocidad máxima de desplazamiento horizontal.")]
        [SerializeField] private float moveSpeed = 8f;

        [Tooltip("Suavizado de aceleración al arrancar.")]
        [SerializeField] private float acceleration = 60f;

        [Tooltip("Suavizado de frenado al detenerse.")]
        [SerializeField] private float deceleration = 60f;

        [Header("--- Salto y Físicas ---")]
        [Tooltip("Fuerza aplicada al realizar el salto.")]
        [SerializeField] private float jumpForce = 14f;

        [Tooltip("Multiplicador de gravedad al caer para evitar sensación de flotabilidad.")]
        [SerializeField] private float fallGravityMultiplier = 2.5f;

        [Tooltip("Multiplicador de caída si se suelta el botón de salto prematuramente (salto variable).")]
        [SerializeField] private float lowJumpGravityMultiplier = 2f;

        [Tooltip("Tiempo de gracia (en segundos) para saltar tras caer de una plataforma (Coyote Time).")]
        [SerializeField] private float coyoteTime = 0.15f;

        [Tooltip("Ventana de tiempo para almacenar el input de salto antes de tocar suelo (Jump Buffer).")]
        [SerializeField] private float jumpBufferTime = 0.15f;

        [Header("--- Detección de Suelo ---")]
        [Tooltip("Punto de verificación de contacto con el suelo.")]
        [SerializeField] private Transform groundCheckPoint;

        [Tooltip("Radio de detección para el OverlapCircle de suelo.")]
        [SerializeField] private float groundCheckRadius = 0.22f;

        [Tooltip("Capa que define qué objetos son considerados suelo transitable.")]
        [SerializeField] private LayerMask groundLayer;

        [Header("--- Referencias Visuales ---")]
        [Tooltip("SpriteRenderer del personaje para voltear la orientación.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Componentes internos
        private Rigidbody2D rb;
        private CapsuleCollider2D col;

        // Variables de estado
        private float horizontalInput;
        private bool isGrounded;
        private bool isFacingRight = true;
        private float originalGravityScale;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool canMove = true;

        // Eventos para sincronización con otros módulos (Audio, Animator, VFX)
        public event Action<bool> OnGroundedStateChanged;
        public event Action OnJumped;

        // Propiedades públicas para consultar estado
        public bool IsGrounded => isGrounded;
        public bool IsFacingRight => isFacingRight;
        public float HorizontalInput => horizontalInput;
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CapsuleCollider2D>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            originalGravityScale = rb.gravityScale;
        }

        private void Update()
        {
            if (!canMove) return;

            // 1. Lectura de Input horizontal (Compatible con Teclado y Gamepad)
            horizontalInput = Input.GetAxisRaw("Horizontal");

            // 2. Comprobación y temporizadores de Coyote Time y Jump Buffer
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            // 3. Ejecutar salto si el buffer y el coyote time están activos
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            {
                ExecuteJump();
                jumpBufferCounter = 0f;
            }

            // 4. Volteo horizontal del personaje
            HandleFlip();
        }

        private void FixedUpdate()
        {
            CheckGround();
            ApplyHorizontalMovement();
            ApplyGravityModifiers();
        }

        /// <summary>
        /// Aplica la aceleración y velocidad horizontal en Rigidbody2D.
        /// </summary>
        private void ApplyHorizontalMovement()
        {
            if (!canMove) return;

            float targetSpeed = horizontalInput * moveSpeed;
            float speedDiff = targetSpeed - rb.linearVelocity.x;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            float movement = speedDiff * accelRate * Time.fixedDeltaTime;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);
        }

        /// <summary>
        /// Realiza el impulso vertical del salto.
        /// </summary>
        private void ExecuteJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimeCounter = 0f;
            OnJumped?.Invoke();
        }

        /// <summary>
        /// Modifica dinámicamente la escala de gravedad para que la caída se sienta ágil y responsiva.
        /// </summary>
        private void ApplyGravityModifiers()
        {
            if (rb.linearVelocity.y < 0)
            {
                // Cayendo: caída más rápida para evitar flotabilidad excesiva
                rb.gravityScale = originalGravityScale * fallGravityMultiplier;
            }
            else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            {
                // Subiendo pero soltó el botón de salto: salto corto
                rb.gravityScale = originalGravityScale * lowJumpGravityMultiplier;
            }
            else
            {
                rb.gravityScale = originalGravityScale;
            }
        }

        /// <summary>
        /// Comprueba si el personaje está tocando el suelo mediante un círculo de colisión.
        /// </summary>
        private void CheckGround()
        {
            Vector2 checkPosition = groundCheckPoint != null 
                ? (Vector2)groundCheckPoint.position 
                : (Vector2)transform.position + Vector2.down * (col.size.y * 0.5f);

            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer);

            if (wasGrounded != isGrounded)
            {
                OnGroundedStateChanged?.Invoke(isGrounded);
            }
        }

        /// <summary>
        /// Gira la orientación del personaje según la dirección de movimiento.
        /// </summary>
        private void HandleFlip()
        {
            if (horizontalInput > 0.01f && !isFacingRight)
            {
                Flip();
            }
            else if (horizontalInput < -0.01f && isFacingRight)
            {
                Flip();
            }
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;

            // Invertir escala en X para que todos los hijos (puntos de disparo, colisionadores secundarios) giren con él
            Vector3 scaler = transform.localScale;
            scaler.x *= -1f;
            transform.localScale = scaler;
        }

        /// <summary>
        /// Permite pausar o habilitar el movimiento externamente (útil para diálogos, cinemáticas o menús).
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            canMove = enabled;
            if (!enabled)
            {
                horizontalInput = 0f;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Gizmo en el Editor para calibrar visualmente el radio de detección de suelo
            Vector2 checkPosition = groundCheckPoint != null 
                ? (Vector2)groundCheckPoint.position 
                : (Vector2)transform.position + Vector2.down * (col != null ? col.size.y * 0.5f : 0.5f);

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
        }
    }
}
