using System;
using System.Collections.Generic;
using UnityEngine;
using CodigoDeEmergencia.Core;

namespace CodigoDeEmergencia.Player
{
    /// <summary>
    /// Controlador de movimiento y físicas 2D del protagonista (Unity 6).
    /// Proporciona un control fluido, responsivo y preciso tipo Metroidvania HD:
    /// - Movimiento horizontal con aceleración y frenado suave.
    /// - Salto con Coyote Time y Jump Buffer (evita saltos perdidos).
    /// - Salto variable (salto corto si se suelta el botón antes).
    /// - Caída rápida para evitar sensación de flotabilidad.
    /// - Orientación y volteo horizontal (Facing).
    /// - Gravedad base constante (BaseGravity = 3f) inmune a bloqueos flotantes.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerMovement : MonoBehaviour
    {
        public const float BaseGravity = 3.0f;

        [Header("--- Movimiento Horizontal ---")]
        [Tooltip("Velocidad máxima horizontal.")]
        [SerializeField] private float moveSpeed = 8.5f;

        [Tooltip("Tasa de aceleración al arrancar.")]
        [SerializeField] private float acceleration = 65f;

        [Tooltip("Tasa de desaceleración al frenar.")]
        [SerializeField] private float deceleration = 70f;

        [Header("--- Salto y Físicas ---")]
        [Tooltip("Fuerza vertical del impulso de salto.")]
        [SerializeField] private float jumpForce = 15.0f;

        [Tooltip("Multiplicador de gravedad al descender para caída ágil.")]
        [SerializeField] private float fallGravityMultiplier = 2.4f;

        [Tooltip("Multiplicador si se suelta el botón de salto antes del punto más alto.")]
        [SerializeField] private float lowJumpGravityMultiplier = 2.0f;

        [Tooltip("Tiempo de gracia tras dejar una plataforma (Coyote Time).")]
        [SerializeField] private float coyoteTime = 0.15f;

        [Tooltip("Tiempo que se recuerda la pulsación de salto antes de tocar suelo (Jump Buffer).")]
        [SerializeField] private float jumpBufferTime = 0.15f;

        [Header("--- Detección de Suelo ---")]
        [Tooltip("Transform ubicado en los pies del personaje para chequear suelo.")]
        [SerializeField] private Transform groundCheckPoint;

        [Tooltip("Radio del círculo de detección de suelo.")]
        [SerializeField] private float groundCheckRadius = 0.22f;

        [Tooltip("Capa de colisión que se considera suelo transitable.")]
        [SerializeField] private LayerMask groundLayer;

        [Header("--- Planeo (Glide) y Escaleras (Climb) ---")]
        [Tooltip("Velocidad vertical al subir o bajar escaleras.")]
        [SerializeField] private float climbSpeed = 5.5f;

        [Tooltip("Velocidad máxima de descenso suave al planear (Glide).")]
        [SerializeField] private float glideFallSpeed = -3.2f;

        // Componentes y referencias
        private Rigidbody2D rb;
        private Collider2D mainCollider;
        private PlayerDash playerDash;

        // Variables de estado
        private float horizontalInput;
        private float verticalInput;
        private bool isGrounded;
        private bool isFacingRight = true;
        private bool isGliding;
        private bool isClimbing;
        private bool isTouchingLadder;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private float jumpLaunchGraceTimer;
        private float knockbackTimer;
        private bool movementEnabled = true;
        private readonly List<Collider2D> ignoredPlatformColliders = new List<Collider2D>();

        // Eventos públicos
        public event Action<bool> OnGroundedChanged;
        public event Action OnJumpExecuted;
        public event Action<bool> OnFacingDirectionChanged;

        // Propiedades públicas
        public bool IsGrounded => isGrounded;
        public bool IsFacingRight => isFacingRight;
        public bool IsGliding => isGliding;
        public bool IsClimbing => isClimbing;
        public float HorizontalInput => horizontalInput;
        public float VerticalInput => verticalInput;
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
        public Rigidbody2D Rigidbody => rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mainCollider = GetComponent<Collider2D>();
            playerDash = GetComponent<PlayerDash>();

            rb.gravityScale = BaseGravity;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            // Garantizar material sin fricción en el colisionador para evitar quedarse pegado en paredes o bordes
            if (mainCollider != null && mainCollider.sharedMaterial == null)
            {
                PhysicsMaterial2D noFriction = new PhysicsMaterial2D("PlayerZeroFriction")
                {
                    friction = 0f,
                    bounciness = 0f
                };
                mainCollider.sharedMaterial = noFriction;
            }

            // Fallback: si no se configuró capa de suelo en el inspector, detectar todo excepto Ignore Raycast
            if (groundLayer.value == 0)
            {
                groundLayer = ~LayerMask.GetMask("Ignore Raycast");
            }
        }

        private void Update()
        {
            if (jumpLaunchGraceTimer > 0f)
            {
                jumpLaunchGraceTimer -= Time.deltaTime;
            }

            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.deltaTime;
            }

            if (!movementEnabled)
            {
                isGliding = false;
                isClimbing = false;
                return;
            }

            // 1. Lectura de Input horizontal y vertical (Teclado o Gamepad)
            horizontalInput = GameInput.GetHorizontal();
            verticalInput = GameInput.GetVertical();

            // 2. Gestión de escaleras (Climb)
            if (isTouchingLadder && Mathf.Abs(verticalInput) > 0.15f)
            {
                isClimbing = true;
                isGliding = false;
            }
            else if (!isTouchingLadder)
            {
                isClimbing = false;
            }

            // 3. Temporizadores de Coyote Time y Jump Buffer
            if (isGrounded || isClimbing)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // Si está en una escalera, solo salta si presiona Espacio (para que W/Arriba sirva para subir)
            bool jumpPressed = isTouchingLadder
                ? GameInput.GetJumpSpaceOnlyDown()
                : GameInput.GetJumpDown();

            if (jumpPressed)
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            // 4. Ejecutar salto si el buffer y el coyote time están activos
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            {
                isClimbing = false;
                ExecuteJump();
                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;
            }

            // 5. Volteo horizontal
            HandleFlip();
        }

        private void FixedUpdate()
        {
            CheckGroundAndLadder();

            if (movementEnabled)
            {
                if (isClimbing)
                {
                    ApplyClimbingMovement();
                }
                else
                {
                    ApplyHorizontalMovement();
                    ApplyGravityModifiers();
                }
            }
        }

        private void ApplyClimbingMovement()
        {
            isGliding = false;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(horizontalInput * (moveSpeed * 0.65f), verticalInput * climbSpeed);
        }

        private void ApplyHorizontalMovement()
        {
            // Respetar el impulso de retroceso (knockback) al recibir daño
            if (knockbackTimer > 0f) return;

            float targetSpeed = horizontalInput * moveSpeed;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

            // MoveTowards evita el overshooting/vibración cuando accelRate * fixedDeltaTime > 1.0
            float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
        }

        private void ExecuteJump()
        {
            jumpLaunchGraceTimer = 0.09f;
            isGliding = false;
            isClimbing = false;

            if (isGrounded)
            {
                isGrounded = false;
                OnGroundedChanged?.Invoke(false);
            }

            rb.gravityScale = BaseGravity;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            GameEvents.PlaySfx(SfxType.Jump);
            Debug.Log("<color=green>[MOVIMIENTO] Salto ejecutado con fuerza " + jumpForce + "</color>");
            OnJumpExecuted?.Invoke();
        }

        private void ApplyGravityModifiers()
        {
            // Si el jugador está ejecutando un dash, no intervenir con la gravedad
            if (playerDash != null && playerDash.IsDashing)
            {
                isGliding = false;
                return;
            }

            // Planeo suave (Glide): en el aire, descendiendo y manteniendo pulsado el botón de salto
            if (!isGrounded && rb.linearVelocity.y < -0.6f && GameInput.GetJumpHeld())
            {
                isGliding = true;
                rb.gravityScale = BaseGravity * 0.35f;
                if (rb.linearVelocity.y < glideFallSpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, glideFallSpeed);
                }
                return;
            }

            isGliding = false;

            if (rb.linearVelocity.y < -0.05f)
            {
                // Cayendo: más gravedad para un arco de salto ágil y sin flotabilidad
                rb.gravityScale = BaseGravity * fallGravityMultiplier;
            }
            else if (rb.linearVelocity.y > 0.05f && !GameInput.GetJumpHeld())
            {
                // Subiendo pero el jugador soltó el botón: salto corto
                rb.gravityScale = BaseGravity * lowJumpGravityMultiplier;
            }
            else
            {
                rb.gravityScale = BaseGravity;
            }

            // Limitar velocidad máxima de caída para evitar atravesar plataformas
            if (rb.linearVelocity.y < -22f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -22f);
            }
        }

        private void CheckGroundAndLadder()
        {
            // Detectar si el cuerpo o los pies del jugador están tocando una escalera interactiva (Trigger con nombre Ladder)
            Vector2 bodyCenter = mainCollider != null ? (Vector2)mainCollider.bounds.center : (Vector2)transform.position + Vector2.up * 1.0f;
            Collider2D[] bodyHits = Physics2D.OverlapBoxAll(bodyCenter + Vector2.down * 0.25f, new Vector2(0.85f, 2.5f), 0f);
            bool foundLadder = false;
            for (int i = 0; i < bodyHits.Length; i++)
            {
                if (bodyHits[i] != null && bodyHits[i].isTrigger && bodyHits[i].gameObject.name.IndexOf("Ladder", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    foundLadder = true;
                    break;
                }
            }
            isTouchingLadder = foundLadder;
            if (!isTouchingLadder && isClimbing)
            {
                isClimbing = false;
                rb.gravityScale = BaseGravity;
            }

            // Permitir atravesar plataformas elevadas ("Platform_") al subir o bajar escaleras
            UpdateLadderPlatformPassThrough(bodyCenter);

            // Durante los primeros milisegundos del despegue de un salto, mantener en aire
            if (jumpLaunchGraceTimer > 0f && rb.linearVelocity.y > 0.1f)
            {
                if (isGrounded)
                {
                    isGrounded = false;
                    OnGroundedChanged?.Invoke(false);
                }
                return;
            }

            Vector2 checkPos = groundCheckPoint != null
                ? (Vector2)groundCheckPoint.position
                : (mainCollider != null
                    ? new Vector2(mainCollider.bounds.center.x, mainCollider.bounds.min.y + 0.04f)
                    : (Vector2)transform.position + Vector2.down * 0.05f);

            LayerMask effectiveMask = (groundLayer.value == 0) ? ~LayerMask.GetMask("Ignore Raycast") : groundLayer;

            // Usar OverlapCircleAll para ignorar el propio colisionador del jugador, triggers y enemigos
            Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, groundCheckRadius, effectiveMask);
            bool foundGround = false;
            float feetMaxY = checkPos.y + groundCheckRadius + 0.12f;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i];
                if (col == null || col.isTrigger) continue;
                if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;
                if (ignoredPlatformColliders.Contains(col)) continue;

                // Ignorar enemigos para que pararse sobre ellos no bloquee ni regale saltos infinitos
                if (col.GetComponentInParent<IDamageable>() != null) continue;

                // Verificar que la superficie esté debajo de los pies y no sea una pared lateral alta
                if (col.bounds.min.y < feetMaxY)
                {
                    foundGround = true;
                    break;
                }
            }

            bool wasGrounded = isGrounded;
            isGrounded = foundGround;

            if (isGrounded)
            {
                isGliding = false;
            }

            if (wasGrounded != isGrounded)
            {
                OnGroundedChanged?.Invoke(isGrounded);
            }
        }

        private void UpdateLadderPlatformPassThrough(Vector2 bodyCenter)
        {
            if (mainCollider == null) return;

            float feetY = mainCollider.bounds.min.y;
            float headY = mainCollider.bounds.max.y;

            if (isClimbing || (isTouchingLadder && verticalInput < -0.15f))
            {
                Collider2D[] nearHits = Physics2D.OverlapBoxAll(bodyCenter, new Vector2(1.6f, 3.6f), 0f);
                for (int i = 0; i < nearHits.Length; i++)
                {
                    Collider2D col = nearHits[i];
                    if (col == null || col.isTrigger || col == mainCollider) continue;
                    if (!col.gameObject.name.StartsWith("Platform_", StringComparison.OrdinalIgnoreCase)) continue;

                    float platTopY = col.bounds.max.y;
                    float platBottomY = col.bounds.min.y;

                    // Ignorar colisión si el jugador está subiendo por debajo de la superficie superior, o bajando por la escalera
                    bool shouldIgnore = (feetY < platTopY - 0.05f && headY > platBottomY - 0.35f) || (verticalInput < -0.1f);
                    if (shouldIgnore)
                    {
                        if (!ignoredPlatformColliders.Contains(col))
                        {
                            Physics2D.IgnoreCollision(mainCollider, col, true);
                            ignoredPlatformColliders.Add(col);
                        }
                    }
                    else if (feetY >= platTopY - 0.04f && verticalInput >= -0.05f)
                    {
                        if (ignoredPlatformColliders.Contains(col))
                        {
                            Physics2D.IgnoreCollision(mainCollider, col, false);
                            ignoredPlatformColliders.Remove(col);
                        }
                    }
                }
            }

            // Restaurar colisiones con plataformas que ya quedaron por debajo de los pies o lejos del jugador
            for (int i = ignoredPlatformColliders.Count - 1; i >= 0; i--)
            {
                Collider2D col = ignoredPlatformColliders[i];
                if (col == null)
                {
                    ignoredPlatformColliders.RemoveAt(i);
                    continue;
                }

                float platTopY = col.bounds.max.y;
                float platBottomY = col.bounds.min.y;
                bool clearAbove = feetY >= platTopY - 0.04f && verticalInput >= -0.05f;
                bool clearBelow = headY <= platBottomY - 0.05f;

                if (!isClimbing || clearAbove || clearBelow)
                {
                    // Solo restaurar si no estamos atorados dentro del grosor de la plataforma
                    bool overlappingHeight = feetY < platTopY - 0.04f && headY > platBottomY + 0.04f;
                    if (!overlappingHeight || clearAbove)
                    {
                        Physics2D.IgnoreCollision(mainCollider, col, false);
                        ignoredPlatformColliders.RemoveAt(i);
                    }
                }
            }
        }

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
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (isFacingRight ? 1f : -1f);
            transform.localScale = scale;

            OnFacingDirectionChanged?.Invoke(isFacingRight);
        }

        /// <summary>
        /// Aplica un impulso de retroceso (knockback) limpio al recibir daño.
        /// </summary>
        public void ApplyKnockback(Vector2 impulseVelocity, float duration = 0.15f)
        {
            if (playerDash != null && playerDash.IsDashing) return;

            knockbackTimer = duration;
            rb.gravityScale = BaseGravity;
            rb.linearVelocity = impulseVelocity;
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            if (!enabled)
            {
                horizontalInput = 0f;
                if (playerDash == null || !playerDash.IsDashing)
                {
                    rb.gravityScale = BaseGravity;
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 checkPos = groundCheckPoint != null
                ? (Vector2)groundCheckPoint.position
                : (Vector2)transform.position + Vector2.down * 0.1f;

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPos, groundCheckRadius);
        }
    }
}
