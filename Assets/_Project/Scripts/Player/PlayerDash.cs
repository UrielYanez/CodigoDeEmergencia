using System;
using System.Collections;
using UnityEngine;
using CodigoDeEmergencia.Core;

namespace CodigoDeEmergencia.Player
{
    /// <summary>
    /// Mecánica de esquive (Dash) ágil del protagonista (Issue #02).
    /// Permite evadir rápidamente a profesores en trance y sortear obstáculos.
    /// Incluye invulnerabilidad temporal y cooldown configurable.
    /// Garantiza la restauración de gravedad a PlayerMovement.BaseGravity sin dejar al personaje flotando.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerDash : MonoBehaviour
    {
        [Header("--- Parámetros del Dash ---")]
        [Tooltip("Velocidad de avance durante el esquive.")]
        [SerializeField] private float dashSpeed = 22f;

        [Tooltip("Duración activa del desplazamiento rápido (en segundos).")]
        [SerializeField] private float dashDuration = 0.2f;

        [Tooltip("Tiempo de enfriamiento entre esquives para evitar spam.")]
        [SerializeField] private float dashCooldown = 0.75f;

        // Referencias internas
        private PlayerMovement playerMovement;
        private PlayerHealth playerHealth;
        private Rigidbody2D rb;
        private Collider2D playerCollider;

        // Estado
        private bool isDashing;
        private bool canDash = true;
        private float dashCooldownTimer;
        private float currentDashDirection = 1f;

        // Eventos
        public event Action OnDashStarted;
        public event Action OnDashEnded;

        // Propiedades públicas
        public bool IsDashing => isDashing;
        public bool CanDash => canDash && !isDashing;
        public float CooldownProgress => isDashing ? 0f : (canDash ? 1f : Mathf.Clamp01(1f - (dashCooldownTimer / Mathf.Max(0.05f, dashCooldown))));

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerHealth = GetComponent<PlayerHealth>();
            rb = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            // Control de temporizador de cooldown
            if (!canDash && !isDashing)
            {
                dashCooldownTimer -= Time.deltaTime;
                if (dashCooldownTimer <= 0f)
                {
                    canDash = true;
                }
            }

            // Lectura de input de dash compatible con New Input System y Gamepad
            if (GameInput.GetDashDown() && canDash && !isDashing)
            {
                if (playerHealth != null && !playerHealth.IsAlive) return;
                StartCoroutine(PerformDashRoutine());
            }
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                // Mantener velocidad constante y atravesar enemigos durante todo el dash
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(currentDashDirection * dashSpeed, 0f);
                SetIgnoreEnemyCollisions(true);
            }
        }

        private IEnumerator PerformDashRoutine()
        {
            isDashing = true;
            canDash = false;
            dashCooldownTimer = dashCooldown;

            // Determinar dirección según input actual o hacia dónde mira el jugador
            float inputX = GameInput.GetHorizontal();
            if (Mathf.Abs(inputX) > 0.05f)
            {
                currentDashDirection = Mathf.Sign(inputX);
            }
            else
            {
                currentDashDirection = playerMovement.IsFacingRight ? 1f : -1f;
            }

            // Pausar movimiento regular, gravedad y desactivar colisión física con enemigos
            playerMovement.SetMovementEnabled(false);
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(currentDashDirection * dashSpeed, 0f);
            SetIgnoreEnemyCollisions(true);

            Debug.Log($"<color=magenta>[DASH] Esquive ejecutado hacia la {(currentDashDirection > 0 ? "DERECHA" : "IZQUIERDA")} a {dashSpeed}m/s (Atravesando enemigos)</color>");
            GameEvents.PlaySfx(SfxType.Dash);
            OnDashStarted?.Invoke();

            yield return new WaitForSeconds(dashDuration);

            // Restaurar gravedad fija base y reactivar movimiento normal
            isDashing = false;
            rb.gravityScale = PlayerMovement.BaseGravity;
            rb.linearVelocity = new Vector2(currentDashDirection * 4f, 0f);
            playerMovement.SetMovementEnabled(true);

            // Solo reactivar colisión con enemigos si el jugador no está en i-frames por daño
            if (playerHealth == null || !playerHealth.IsInvulnerable)
            {
                SetIgnoreEnemyCollisions(false);
            }

            OnDashEnded?.Invoke();
            Debug.Log($"<color=magenta>[DASH] Fin del esquive. Gravedad restablecida a {PlayerMovement.BaseGravity}</color>");
        }

        public void SetIgnoreEnemyCollisions(bool ignore)
        {
            if (playerCollider == null) return;

            Collider2D[] allColliders = FindObjectsByType<Collider2D>();
            for (int i = 0; i < allColliders.Length; i++)
            {
                Collider2D col = allColliders[i];
                if (col == null || col == playerCollider || col.isTrigger) continue;
                if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;

                // Si el collider pertenece a un enemigo o pulsera (IDamageable), ignorar/restaurar colisión física
                if (col.GetComponentInParent<IDamageable>() != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, col, ignore);
                }
            }
        }

        private void OnDisable()
        {
            if (isDashing)
            {
                isDashing = false;
                if (rb != null) rb.gravityScale = PlayerMovement.BaseGravity;
                if (playerMovement != null) playerMovement.SetMovementEnabled(true);
                SetIgnoreEnemyCollisions(false);
            }
        }

        /// <summary>
        /// Reinicia el cooldown del dash inmediatamente.
        /// </summary>
        public void ResetDashCooldown()
        {
            canDash = true;
            dashCooldownTimer = 0f;
        }
    }
}
