using System;
using System.Collections;
using UnityEngine;
using CodigoDeEmergencia.Core;

namespace CodigoDeEmergencia.Player
{
    /// <summary>
    /// Sistema de vida y daño del protagonista (0 a 100 HP).
    /// Implementa IDamageable, marcos de invulnerabilidad (i-frames) con parpadeo visual,
    /// inmunidad durante el Dash, registro de logs claros y botón de reintento/revivir inmediato (R).
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("--- Parámetros de Salud ---")]
        [Tooltip("Salud máxima del protagonista (0 a 100).")]
        [SerializeField] private int maxHealth = 100;

        [Tooltip("Salud con la que inicia el jugador.")]
        [SerializeField] private int currentHealth = 100;

        [Header("--- Invulnerabilidad y Feedback ---")]
        [Tooltip("Duración de invulnerabilidad tras recibir un impacto (en segundos).")]
        [SerializeField] private float invulnerabilityDuration = 1.2f;

        [Tooltip("Frecuencia de parpadeo del sprite durante la invulnerabilidad.")]
        [SerializeField] private float flashInterval = 0.1f;

        [Tooltip("SpriteRenderer del jugador para efecto de parpadeo.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Referencias
        private PlayerDash playerDash;
        private PlayerMovement playerMovement;
        private PlayerShooting playerShooting;

        // Estado interno
        private bool isInvulnerable;
        private bool isDead;
        private Vector3 respawnPosition = new Vector3(-21f, -1.75f, 0f);
        private int activeCheckpointIndex;

        // Propiedades públicas (IDamageable)
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => !isDead;
        public bool IsInvulnerable => isInvulnerable;
        public int ActiveCheckpointIndex => activeCheckpointIndex;

        // Eventos locales
        public event Action<int, int> OnHealthChanged;
        public event Action OnDamaged;
        public event Action OnDied;

        private void Awake()
        {
            playerDash = GetComponent<PlayerDash>();
            playerMovement = GetComponent<PlayerMovement>();
            playerShooting = GetComponent<PlayerShooting>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            currentHealth = maxHealth;
        }

        private void Start()
        {
            respawnPosition = transform.position;
            NotifyHealth();
        }

        private void Update()
        {
            // Atajo de reintento / revivir instantáneo si el estudiante fue derrotado
            if (isDead && (GameInput.GetRestartDown() || GameInput.GetJumpDown()))
            {
                Revive();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || !other.isTrigger) return;
            if (!other.gameObject.name.StartsWith("Checkpoint_", StringComparison.OrdinalIgnoreCase)) return;

            Vector3 cpPos = new Vector3(other.transform.position.x, other.bounds.min.y, 0f);
            if (Vector3.Distance(respawnPosition, cpPos) < 1.0f) return;

            respawnPosition = cpPos;
            int cpIndex = other.gameObject.name.Contains("2") ? 2 : 1;
            activeCheckpointIndex = cpIndex;

            // Cambiar indicador visual del Checkpoint a verde esmeralda activo
            SpriteRenderer[] renderers = other.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].gameObject.name.Contains("Light") || renderers[i].gameObject.name.Contains("Beacon"))
                {
                    renderers[i].color = new Color(0.1f, 1.0f, 0.45f, 1f);
                }
                else
                {
                    renderers[i].color = new Color(0.75f, 1.0f, 0.85f, 1f);
                }
            }

            Heal(35);
            GameEvents.PlaySfx(SfxType.Checkpoint);
            GameEvents.TriggerCheckpointActivated(cpIndex);
            Debug.Log($"<color=green><b>[CHECKPOINT #{cpIndex} ACTIVADO]</b> Punto de reaparición guardado en X={cpPos.x:F1}, Y={cpPos.y:F1}.</color>");
        }

        /// <summary>
        /// Aplica daño al protagonista respetando invulnerabilidad e inmunidad por esquive.
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (isDead || isInvulnerable) return;

            // Inmunidad si el jugador está realizando un Dash
            if (playerDash != null && playerDash.IsDashing)
            {
                Debug.Log("<color=magenta>[JUGADOR] ¡Ataque ESQUIVADO con éxito gracias al Dash!</color>");
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - damageInfo.amount);
            string attacker = damageInfo.damageSource != null ? damageInfo.damageSource.name : "Enemigo";
            Debug.Log($"<color=orange><b>[JUGADOR]</b> Impacto de {attacker} (-{damageInfo.amount} HP). Salud restante: {currentHealth}/{maxHealth}</color>");

            // Aplicar retroceso (Knockback) para separar al jugador del enemigo atacante
            if (playerMovement != null && currentHealth > 0)
            {
                float knockDir = playerMovement.IsFacingRight ? -1f : 1f;
                if (damageInfo.damageSource != null)
                {
                    knockDir = (transform.position.x >= damageInfo.damageSource.transform.position.x) ? 1f : -1f;
                }
                playerMovement.ApplyKnockback(new Vector2(knockDir * 6.5f, 5.0f), 0.16f);
            }

            NotifyHealth();
            OnDamaged?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                GameEvents.PlaySfx(SfxType.PlayerHurt);
                StartCoroutine(InvulnerabilityRoutine());
            }
        }

        /// <summary>
        /// Recupera puntos de vida (café, botiquín o mochila).
        /// </summary>
        public void Heal(int amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            Debug.Log($"<color=green>[JUGADOR] Curación recibida (+{amount} HP). Salud: {currentHealth}/{maxHealth}</color>");
            NotifyHealth();
        }

        private void NotifyHealth()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            GameEvents.TriggerPlayerHealthChanged(currentHealth, maxHealth);
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            GameEvents.PlaySfx(SfxType.PlayerDead);
            Debug.LogError("<color=red><b>[ESTUDIANTE DERROTADO]</b> La salud llegó a 0. Presiona <b>'R'</b> o <b>Espacio</b> para revivir en el último Checkpoint.</color>");

            OnDied?.Invoke();
            GameEvents.TriggerPlayerDied();

            // Bloquear controles de movimiento y disparo temporalmente
            if (playerMovement != null) playerMovement.SetMovementEnabled(false);
            if (playerShooting != null) playerShooting.SetShootingEnabled(false);
        }

        public void Revive()
        {
            StopAllCoroutines();
            isDead = false;
            isInvulnerable = false;
            currentHealth = maxHealth;
            transform.position = respawnPosition;

            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            if (playerMovement != null) playerMovement.SetMovementEnabled(true);
            if (playerShooting != null) playerShooting.SetShootingEnabled(true);
            if (playerDash != null) playerDash.SetIgnoreEnemyCollisions(false);

            NotifyHealth();
            Debug.Log($"<color=green><b>[REVIVIDO]</b> Salud restaurada a 100 HP. Reaparición en {(activeCheckpointIndex > 0 ? "Checkpoint #" + activeCheckpointIndex : "Inicio del Nivel")} ({respawnPosition.x:F1}, {respawnPosition.y:F1}).</color>");
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            if (playerDash != null) playerDash.SetIgnoreEnemyCollisions(true);

            float elapsed = 0f;

            Color originalColor = Color.white;
            Color flashColor = new Color(1f, 0.45f, 0.45f, 0.35f);

            while (elapsed < invulnerabilityDuration)
            {
                if (spriteRenderer != null) spriteRenderer.color = flashColor;
                yield return new WaitForSeconds(flashInterval);

                if (spriteRenderer != null) spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);

                elapsed += (flashInterval * 2f);
            }

            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            isInvulnerable = false;

            if (playerDash != null && !playerDash.IsDashing)
            {
                playerDash.SetIgnoreEnemyCollisions(false);
            }
        }

        public void ResetHealth()
        {
            Revive();
        }
    }
}
