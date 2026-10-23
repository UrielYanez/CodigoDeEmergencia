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
        [Header("--- Parámetros de Salud y Vidas (Escala 0-100) ---")]
        [Tooltip("Salud máxima del protagonista (0 a 100).")]
        [SerializeField] private int maxHealth = 100;

        [Tooltip("Salud con la que inicia el jugador.")]
        [SerializeField] private int currentHealth = 100;

        [Tooltip("Número máximo de vidas por nivel antes de Game Over definitivo.")]
        [SerializeField] private int maxLives = 3;

        [Tooltip("Vidas actuales restantes del jugador.")]
        [SerializeField] private int currentLives = 3;

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
        private string lastCheckpointRewardNote = "";
        private GameObject lockedArenaGate;
        private Collider2D lockedArenaTrigger;

        // Propiedades públicas (IDamageable)
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int CurrentLives => currentLives;
        public int MaxLives => maxLives;
        public bool IsAlive => !isDead;
        public bool IsInvulnerable => isInvulnerable;
        public int ActiveCheckpointIndex => activeCheckpointIndex;
        public string LastCheckpointRewardNote => lastCheckpointRewardNote;
        public Vector3 LastCheckpointPosition => respawnPosition;

        // Eventos locales
        public event Action<int, int> OnHealthChanged;
        public event Action<int, int> OnLivesChanged;
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
            currentLives = maxLives;
        }

        private void Start()
        {
            respawnPosition = transform.position;

            int pendingSlot = PlayerPrefs.GetInt("CDE_PendingLoadSlot", -1);
            if (pendingSlot >= 0)
            {
                PlayerPrefs.SetInt("CDE_ActiveSaveSlot", pendingSlot);
                PlayerPrefs.SetInt("CDE_PendingLoadSlot", -1);
                PlayerPrefs.Save();

                string prefix = $"CDE_SaveSlot_{pendingSlot}_";
                if (PlayerPrefs.GetInt(prefix + "HasData", 0) == 1)
                {
                    int savedHp = PlayerPrefs.GetInt(prefix + "Health", maxHealth);
                    int savedLives = PlayerPrefs.GetInt(prefix + "Lives", maxLives);
                    int savedCp = PlayerPrefs.GetInt(prefix + "Checkpoint", 0);
                    float savedX = PlayerPrefs.GetFloat(prefix + "PosX", transform.position.x);
                    float savedY = PlayerPrefs.GetFloat(prefix + "PosY", transform.position.y);

                    Vector3 finalPos = new Vector3(savedX, savedY, 0f);
                    if (savedCp > 0)
                    {
                        GameObject cpObj = GameObject.Find($"Checkpoint_{savedCp}");
                        if (cpObj != null)
                        {
                            finalPos = new Vector3(cpObj.transform.position.x, cpObj.transform.position.y - 1.05f, 0f);
                        }
                    }

                    RestoreSavedState(savedHp, savedLives, savedCp, finalPos);
                    return;
                }
            }

            maxLives = GameSettings.StartingLives;
            currentLives = maxLives;
            NotifyHealth();
            NotifyLives();
        }

        /// <summary>
        /// Restaura el estado completo del jugador desde una ranura de partida guardada.
        /// </summary>
        public void RestoreSavedState(int savedHp, int savedLives, int savedCheckpoint, Vector3 savedPos)
        {
            maxLives = Mathf.Max(savedLives, GameSettings.StartingLives);
            currentHealth = Mathf.Clamp(savedHp, 1, maxHealth);
            currentLives = Mathf.Clamp(savedLives, 1, maxLives);
            activeCheckpointIndex = Mathf.Max(0, savedCheckpoint);
            respawnPosition = savedPos;
            transform.position = savedPos;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            for (int i = 1; i <= activeCheckpointIndex; i++)
            {
                GameObject cpObj = GameObject.Find($"Checkpoint_{i}");
                if (cpObj != null)
                {
                    MarkCheckpointVisualActive(cpObj);
                }
            }

            CameraFollow2D camFollow = FindAnyObjectByType<CameraFollow2D>();
            if (camFollow != null)
            {
                camFollow.SnapToTargetImmediate();
            }

            NotifyHealth();
            NotifyLives();
            lastCheckpointRewardNote = "Partida cargada";
            if (activeCheckpointIndex > 0)
            {
                GameEvents.TriggerCheckpointActivated(activeCheckpointIndex);
            }
            Debug.Log($"<color=cyan><b>[PARTIDA CARGADA]</b> Posición=({savedPos.x:F1}, {savedPos.y:F1}), HP={currentHealth}/{maxHealth}, Vidas={currentLives}/{maxLives}, Checkpoint=#{activeCheckpointIndex}.</color>");
        }

        private void Update()
        {
            // Atajo de reintento instantáneo en el Checkpoint solo si aún quedan vidas disponibles
            if (isDead && currentLives > 0 && (GameInput.GetRestartDown() || GameInput.GetJumpDown()))
            {
                Revive();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || !other.isTrigger) return;

            // 1. Activación de encerrona en la Arena del Jefe (BossArenaTrigger)
            if (other.gameObject.name.StartsWith("BossArenaTrigger", StringComparison.OrdinalIgnoreCase))
            {
                other.enabled = false;
                lockedArenaTrigger = other;

                // Garantizar siempre el Checkpoint previo al Jefe justo antes de la compuerta de la arena
                float preBossCheckX = other.transform.position.x - 12.0f;
                if (respawnPosition.x < preBossCheckX - 5.0f)
                {
                    respawnPosition = new Vector3(preBossCheckX, -1.75f, 0f);
                    activeCheckpointIndex = Mathf.Max(activeCheckpointIndex, 8);
                    lastCheckpointRewardNote = "Guardado";
                    GameEvents.TriggerCheckpointActivated(activeCheckpointIndex);
                }

                GameObject gate = GameObject.Find("Boss_Arena_Gate");
                if (gate != null)
                {
                    StartCoroutine(AnimateHeavyGateDescentRoutine(gate));
                }
                else
                {
                    GameEvents.PlaySfx(SfxType.GateClose);
                    GameEvents.TriggerBossArenaLocked();
                }
                return;
            }

            // 2. Activación de Checkpoints a lo largo del nivel
            if (!other.gameObject.name.StartsWith("Checkpoint_", StringComparison.OrdinalIgnoreCase)) return;

            Vector3 cpPos = new Vector3(other.transform.position.x, other.transform.position.y - 1.05f, 0f);
            int cpIndex = 1;
            string suffix = other.gameObject.name.Replace("Checkpoint_", "");
            if (!int.TryParse(suffix, out cpIndex))
            {
                cpIndex = activeCheckpointIndex + 1;
            }
            if (cpIndex == activeCheckpointIndex && Vector3.Distance(respawnPosition, cpPos) < 1.2f) return;

            respawnPosition = cpPos;
            activeCheckpointIndex = cpIndex;

            MarkCheckpointVisualActive(other.gameObject);

            // Regla mejorada de Checkpoint: regenera la salud al 100%; si ya está al 100%, suma +1 vida extra si faltan vidas
            if (currentHealth < maxHealth)
            {
                currentHealth = maxHealth;
                NotifyHealth();
                lastCheckpointRewardNote = "Salud 100%";
            }
            else if (currentLives < maxLives)
            {
                currentLives = Mathf.Min(maxLives, currentLives + 1);
                NotifyLives();
                lastCheckpointRewardNote = "+1 Vida extra";
            }
            else
            {
                lastCheckpointRewardNote = "Guardado";
            }

            GameEvents.TriggerCheckpointActivated(cpIndex);
            Debug.Log($"<color=green><b>[CHECKPOINT #{cpIndex} ACTIVADO]</b> {lastCheckpointRewardNote} en X={cpPos.x:F1}, Y={cpPos.y:F1}.</color>");
        }

        private static void MarkCheckpointVisualActive(GameObject cpObj)
        {
            if (cpObj == null) return;

            Transform idlePost = cpObj.transform.Find("Checkpoint_Post_Idle");
            Transform activePost = cpObj.transform.Find("Checkpoint_Post_Active");
            if (idlePost != null) idlePost.gameObject.SetActive(false);
            if (activePost != null) activePost.gameObject.SetActive(true);

            SpriteRenderer[] renderers = cpObj.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                string n = renderers[i].gameObject.name;
                if (n == "Checkpoint_Post_Active")
                {
                    renderers[i].color = Color.white;
                }
                else if (n.Contains("Halo") || n.Contains("Beacon") || n.Contains("Light"))
                {
                    renderers[i].color = new Color(0.18f, 1.0f, 0.55f, 0.92f);
                }
                else if (n.Contains("Glow"))
                {
                    renderers[i].color = new Color(0.18f, 1.0f, 0.55f, 0.22f);
                }
                else if (n != "Checkpoint_Post_Idle")
                {
                    renderers[i].color = new Color(0.78f, 1.0f, 0.88f, 1f);
                }
            }
        }

        private IEnumerator AnimateHeavyGateDescentRoutine(GameObject gate)
        {
            if (gate == null) yield break;

            lockedArenaGate = gate;
            Collider2D gateCol = gate.GetComponent<Collider2D>();
            if (gateCol != null)
            {
                gateCol.enabled = false;
            }

            Vector3 startPos = new Vector3(gate.transform.position.x, 18.0f, 0f);
            Vector3 endPos = new Vector3(gate.transform.position.x, 6.25f, 0f);
            gate.transform.position = startPos;

            SpriteRenderer[] gateRends = gate.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < gateRends.Length; i++)
            {
                gateRends[i].enabled = true;
                gateRends[i].color = new Color(1f, 0.22f, 0.35f, 0.98f);
            }

            float duration = 0.45f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Aceleración de caída pesada hidráulica
                float curvedT = t * t;
                gate.transform.position = Vector3.Lerp(startPos, endPos, curvedT);
                yield return null;
            }

            gate.transform.position = endPos;
            if (gateCol != null)
            {
                gateCol.enabled = true;
                gateCol.isTrigger = false;
            }

            GameEvents.PlaySfx(SfxType.GateClose);
            CameraFollow2D.RequestShake(0.32f, 0.35f);
            GameEvents.TriggerBossArenaLocked();
            Debug.Log("<color=red><b>[COMPUERTA PESADA CERRADA]</b> ¡Impacto masivo de compuerta blindada en la arena del Jefe!</color>");
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

            int scaledAmount = Mathf.Max(1, Mathf.RoundToInt(damageInfo.amount * GameSettings.PlayerDamageMultiplier));
            currentHealth = Mathf.Max(0, currentHealth - scaledAmount);
            string attacker = damageInfo.damageSource != null ? damageInfo.damageSource.name : "Enemigo";
            Debug.Log($"<color=orange><b>[JUGADOR]</b> Impacto de {attacker} (-{scaledAmount} HP). Salud restante: {currentHealth}/{maxHealth}</color>");
            CameraFollow2D.RequestShake(0.24f, 0.22f);

            // Aplicar retroceso (Knockback) consistente en cada impacto para separar al jugador del enemigo atacante
            if (playerMovement != null && currentHealth > 0)
            {
                float knockDir = playerMovement.IsFacingRight ? -1f : 1f;
                if (damageInfo.damageSource != null)
                {
                    float diffX = transform.position.x - damageInfo.damageSource.transform.position.x;
                    knockDir = Mathf.Abs(diffX) > 0.05f ? Mathf.Sign(diffX) : (playerMovement.IsFacingRight ? -1f : 1f);
                }
                playerMovement.ApplyKnockback(new Vector2(knockDir * 7.2f, 5.2f), 0.20f);
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

        private void NotifyLives()
        {
            OnLivesChanged?.Invoke(currentLives, maxLives);
            GameEvents.TriggerPlayerLivesChanged(currentLives, maxLives);
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            currentLives = Mathf.Max(0, currentLives - 1);
            NotifyLives();

            GameEvents.PlaySfx(SfxType.PlayerDead);

            OnDied?.Invoke();
            GameEvents.TriggerPlayerDied();

            // Bloquear controles de movimiento y disparo temporalmente
            if (playerMovement != null) playerMovement.SetMovementEnabled(false);
            if (playerShooting != null) playerShooting.SetShootingEnabled(false);

            if (currentLives > 0)
            {
                Debug.LogWarning($"<color=orange><b>[VIDA PERDIDA]</b> Vidas restantes: {currentLives}/{maxLives}. Reapareciendo en el último Checkpoint...</color>");
                StartCoroutine(AutoRespawnAtCheckpointRoutine(1.15f));
            }
            else
            {
                Debug.LogError("<color=red><b>[GAME OVER]</b> Se agotaron las 3 vidas. Reinicio obligatorio del nivel.</color>");
                GameEvents.TriggerLevelStateChanged(LevelState.GameOver);
            }
        }

        private IEnumerator AutoRespawnAtCheckpointRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (isDead && currentLives > 0)
            {
                Revive();
            }
        }

        public void Revive()
        {
            if (currentLives <= 0) return;

            StopAllCoroutines();
            isDead = false;
            isInvulnerable = false;
            currentHealth = maxHealth;
            transform.position = respawnPosition;

            // Si reaparece fuera de la arena del jefe, reabrir la compuerta para que pueda volver a entrar
            if (lockedArenaGate != null && respawnPosition.x < lockedArenaGate.transform.position.x)
            {
                Collider2D gateCol = lockedArenaGate.GetComponent<Collider2D>();
                if (gateCol != null) gateCol.enabled = false;
                lockedArenaGate.transform.position = new Vector3(lockedArenaGate.transform.position.x, 18.0f, 0f);
                SpriteRenderer[] gateRends = lockedArenaGate.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < gateRends.Length; i++)
                {
                    gateRends[i].color = new Color(1f, 0.18f, 0.32f, 0.18f);
                }
                if (lockedArenaTrigger != null) lockedArenaTrigger.enabled = true;
            }

            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            if (playerMovement != null) playerMovement.SetMovementEnabled(true);
            if (playerShooting != null) playerShooting.SetShootingEnabled(true);
            if (playerDash != null) playerDash.SetIgnoreEnemyCollisions(false);

            NotifyHealth();
            NotifyLives();
            StartCoroutine(InvulnerabilityRoutine());
            Debug.Log($"<color=green><b>[REVIVIDO EN CHECKPOINT]</b> Salud restaurada a 100 HP (Vidas: {currentLives}/{maxLives}). Posición: ({respawnPosition.x:F1}, {respawnPosition.y:F1}).</color>");
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            // Mantener colisión física sólida con los enemigos durante la invulnerabilidad por daño
            // para evitar que los enemigos atraviesen o se empalmen con el jugador estático.

            float elapsed = 0f;

            Color originalColor = Color.white;
            Color flashColor = new Color(1f, 0.45f, 0.45f, 0.35f);

            float duration = Mathf.Max(0.35f, invulnerabilityDuration * (GameSettings.InvulnerabilityDuration / 1.2f));
            while (elapsed < duration)
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
