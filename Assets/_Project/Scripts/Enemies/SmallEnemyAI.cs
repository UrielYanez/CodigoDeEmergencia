using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Player;

namespace CodigoDeEmergencia.Enemies
{
    /// <summary>
    /// Mini-enemigo del Sistema S.I.A.
    /// Persigue al jugador incluyendo navegación inteligente por escaleras y recibe daño de pulsos PEM o Katana.
    /// Incluye cooldown de daño por contacto y validación estricta de suelo antes de saltar.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class SmallEnemyAI : MonoBehaviour, IDamageable
    {
        public enum DroneVariant
        {
            YellowDrone,       // Drone Amarillo Estándar S.I.A. (Equilibrado)
            CrimsonHunter,     // Cazador Carmesí (Veloz y agresivo)
            PlasmaScout,       // Explorador Cian Centinela (Ágil)
            HeavyTitanium      // Unidad Blindada de Titanio (Mayor resistencia y peso)
        }

        [Header("--- Parámetros del Mini-Enemigo (Escala 0-100) ---")]
        [Tooltip("Salud máxima del enemigo (38 HP base en escala 0-100, +50% resistencia).")]
        [SerializeField] private int maxHealth = 38;

        [Tooltip("Salud actual.")]
        [SerializeField] private int currentHealth = 38;

        [Tooltip("Velocidad de persecución terrestre.")]
        [SerializeField] private float moveSpeed = 3.2f;

        [Tooltip("Velocidad al subir o bajar escaleras.")]
        [SerializeField] private float climbSpeed = 3.8f;

        [Tooltip("Daño por contacto al jugador (escala 0-100).")]
        [SerializeField] private int contactDamage = 8;

        [Tooltip("Efecto de chispas/explosión al morir.")]
        [SerializeField] private GameObject explosionVfxPrefab;

        [Header("--- Variantes de Apariencia e IA ---")]
        [SerializeField] private DroneVariant variant = DroneVariant.YellowDrone;
        [SerializeField] private bool randomizeVariantOnStart = true;
        [SerializeField] private float difficultyMultiplier = 1.0f;

        private static readonly List<Collider2D> cachedLadders = new List<Collider2D>();
        private static ulong cachedLadderSceneHandle = ulong.MaxValue;

        private Rigidbody2D rb;
        private Collider2D mainCollider;
        private Transform playerTarget;
        private bool isDead;
        private float lastDamageTime;
        private float knockbackTimer;
        private float dropCooldownTimer;
        private float jumpCooldownTimer;
        private float ladderSearchCooldown;
        private float ladderExitCooldown;
        private float baseGravityScale = 2.5f;
        private bool isClimbingLadder;
        private bool isDroppingThrough;
        private Collider2D targetLadder;
        private float climbDirection;
        private readonly List<Collider2D> ignoredPlatformColliders = new List<Collider2D>();
        private const float DamageCooldown = 1.0f;

        // Propiedades IDamageable
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => !isDead;

        public event Action<int, int> OnHealthChanged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mainCollider = GetComponent<Collider2D>();
            currentHealth = maxHealth;
            if (rb != null)
            {
                baseGravityScale = rb.gravityScale > 0.1f ? rb.gravityScale : 2.5f;
            }

            if (mainCollider != null && mainCollider.sharedMaterial == null)
            {
                mainCollider.sharedMaterial = new PhysicsMaterial2D("EnemyZeroFriction")
                {
                    friction = 0f,
                    bounciness = 0f
                };
            }
        }

        private void Start()
        {
            EnsureLadderCache();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;

                // Solo ignorar colisión si el jugador está activamente en pleno Dash
                PlayerDash pd = playerObj.GetComponent<PlayerDash>();
                if (pd != null && pd.IsDashing)
                {
                    pd.SetIgnoreEnemyCollisions(true);
                }
            }

            if (randomizeVariantOnStart)
            {
                int vIdx = Mathf.Abs(Mathf.RoundToInt(transform.position.x * 7.3f + transform.position.y)) % 4;
                ApplyVariant((DroneVariant)vIdx);
            }
            else
            {
                ApplyVariant(variant);
            }

            Debug.Log($"<color=yellow>[ENEMIGO S.I.A.] Drone ({variant}) activado en X={transform.position.x:F1}, Y={transform.position.y:F1} con {maxHealth} HP.</color>");
        }

        public void ApplyVariant(DroneVariant newVariant)
        {
            variant = newVariant;
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

            switch (variant)
            {
                case DroneVariant.YellowDrone:
                    if (sr != null) sr.color = new Color(1.0f, 0.95f, 0.35f, 1f);
                    moveSpeed = 3.2f;
                    maxHealth = 38;
                    contactDamage = 8;
                    break;
                case DroneVariant.CrimsonHunter:
                    if (sr != null) sr.color = new Color(1.0f, 0.32f, 0.38f, 1f);
                    moveSpeed = 3.9f;
                    maxHealth = 30;
                    contactDamage = 10;
                    break;
                case DroneVariant.PlasmaScout:
                    if (sr != null) sr.color = new Color(0.35f, 0.95f, 1.0f, 1f);
                    moveSpeed = 3.5f;
                    maxHealth = 33;
                    contactDamage = 7;
                    break;
                case DroneVariant.HeavyTitanium:
                    if (sr != null) sr.color = new Color(0.70f, 0.78f, 0.90f, 1f);
                    moveSpeed = 2.6f;
                    maxHealth = 60;
                    contactDamage = 12;
                    break;
            }
            maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * difficultyMultiplier));
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Escala la vida de este drone según la dificultad del nivel (1.0 = Nivel 1, +20% por nivel). Debe llamarse antes de Start().
        /// </summary>
        public void SetDifficultyMultiplier(float multiplier)
        {
            difficultyMultiplier = Mathf.Max(0.5f, multiplier);
        }

        private void Update()
        {
            if (knockbackTimer > 0f) knockbackTimer -= Time.deltaTime;
            if (dropCooldownTimer > 0f) dropCooldownTimer -= Time.deltaTime;
            if (jumpCooldownTimer > 0f) jumpCooldownTimer -= Time.deltaTime;
            if (ladderSearchCooldown > 0f) ladderSearchCooldown -= Time.deltaTime;
            if (ladderExitCooldown > 0f) ladderExitCooldown -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            UpdateEnemyPlatformPassThrough();

            if (isDead || playerTarget == null) return;
            if (knockbackTimer > 0f) return;

            if (isClimbingLadder)
            {
                HandleClimbingStep();
                return;
            }

            float distToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distToPlayer > 22.0f)
            {
                targetLadder = null;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            float diffX = playerTarget.position.x - transform.position.x;
            float verticalDiff = playerTarget.position.y - transform.position.y;
            float absDiffX = Mathf.Abs(diffX);

            // 1. NAVEGACIÓN POR ESCALERAS CUANDO HAY DIFERENCIA DE PISO
            if (Mathf.Abs(verticalDiff) > 1.35f && ladderExitCooldown <= 0f)
            {
                if (ladderSearchCooldown <= 0f || targetLadder == null)
                {
                    targetLadder = FindBestReachableLadder(verticalDiff > 0f);
                    ladderSearchCooldown = 0.25f;
                }

                if (targetLadder != null)
                {
                    float ladderCenterX = targetLadder.bounds.center.x;
                    float distToLadderX = ladderCenterX - transform.position.x;

                    if (Mathf.Abs(distToLadderX) <= 0.34f && IsStrictlyGrounded())
                    {
                        isClimbingLadder = true;
                        climbDirection = verticalDiff > 0f ? 1f : -1f;
                        rb.gravityScale = 0f;
                        rb.linearVelocity = Vector2.zero;
                        transform.position = new Vector3(ladderCenterX, transform.position.y, transform.position.z);
                        IgnorePlatformsAlongLadder(climbDirection);
                        return;
                    }

                    float toLadderDirX = Mathf.Abs(distToLadderX) > 0.05f ? Mathf.Sign(distToLadderX) : 0f;
                    rb.linearVelocity = new Vector2(toLadderDirX * moveSpeed, rb.linearVelocity.y);
                    UpdateFacing(toLadderDirX);
                    return;
                }
            }
            else
            {
                targetLadder = null;
            }

            // 2. RESPALDO SOLO SI NO HAY ESCALERA ALCANZABLE Y EL ENEMIGO ESTÁ FIRMEMENTE EN EL SUELO
            bool grounded = IsStrictlyGrounded();
            if (targetLadder == null && grounded)
            {
                if (verticalDiff < -1.4f && absDiffX < 3.8f && dropCooldownTimer <= 0f && mainCollider != null)
                {
                    Bounds b = mainCollider.bounds;
                    RaycastHit2D[] platHits = Physics2D.RaycastAll(new Vector2(b.center.x, b.min.y - 0.04f), Vector2.down, 0.85f);
                    for (int i = 0; i < platHits.Length; i++)
                    {
                        Collider2D c = platHits[i].collider;
                        if (c == null || c.isTrigger || c == mainCollider || c.transform.IsChildOf(transform)) continue;
                        if (c.gameObject.name.StartsWith("Platform_", StringComparison.OrdinalIgnoreCase))
                        {
                            StartCoroutine(DropThroughPlatformRoutine(c));
                            break;
                        }
                    }
                }
                else if (verticalDiff > 1.4f && absDiffX < 3.0f && jumpCooldownTimer <= 0f)
                {
                    jumpCooldownTimer = 2.2f;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 8.8f);
                }
            }

            // 3. PERSECUCIÓN HORIZONTAL EVITANDO EMPALMARSE SOBRE EL JUGADOR
            float diffYAbs = Mathf.Abs(verticalDiff);
            if (absDiffX < 0.88f && diffYAbs < 1.5f)
            {
                float pushOutDir = diffX >= 0f ? -1f : 1f;
                rb.linearVelocity = new Vector2(pushOutDir * 1.8f, rb.linearVelocity.y);
                return;
            }

            float directionX = absDiffX > 0.1f ? Mathf.Sign(diffX) : 0f;
            rb.linearVelocity = new Vector2(directionX * moveSpeed, rb.linearVelocity.y);
            UpdateFacing(directionX);
        }

        private void UpdateFacing(float directionX)
        {
            if (Mathf.Abs(directionX) > 0.01f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (directionX > 0f ? 1f : -1f);
                transform.localScale = scale;
            }
        }

        private static void EnsureLadderCache()
        {
            ulong activeHandle = SceneManager.GetActiveScene().handle.GetRawData();
            if (cachedLadderSceneHandle != activeHandle || cachedLadders.Count == 0)
            {
                cachedLadderSceneHandle = activeHandle;
                cachedLadders.Clear();
                Collider2D[] allCols = FindObjectsByType<Collider2D>();
                for (int i = 0; i < allCols.Length; i++)
                {
                    Collider2D c = allCols[i];
                    if (c != null && c.isTrigger && c.gameObject.name.IndexOf("Ladder", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        cachedLadders.Add(c);
                    }
                }
            }
            else
            {
                for (int i = cachedLadders.Count - 1; i >= 0; i--)
                {
                    if (cachedLadders[i] == null) cachedLadders.RemoveAt(i);
                }
            }
        }

        private bool IsStrictlyGrounded()
        {
            if (mainCollider == null || rb == null) return false;
            if (Mathf.Abs(rb.linearVelocity.y) > 0.25f) return false;

            Bounds b = mainCollider.bounds;
            Vector2 checkCenter = new Vector2(b.center.x, b.min.y - 0.05f);
            Vector2 checkSize = new Vector2(b.size.x * 0.70f, 0.14f);
            Collider2D[] hits = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f);

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i];
                if (col == null || col.isTrigger) continue;
                if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;
                if (ignoredPlatformColliders.Contains(col)) continue;
                if (col.GetComponentInParent<IDamageable>() != null) continue;

                if (col.bounds.max.y <= b.min.y + 0.18f)
                {
                    return true;
                }
            }
            return false;
        }

        private Collider2D FindBestReachableLadder(bool needUp)
        {
            EnsureLadderCache();
            if (mainCollider == null || playerTarget == null) return null;

            float myX = transform.position.x;
            float myFeetY = mainCollider.bounds.min.y;
            float torsoY = mainCollider.bounds.center.y;
            float playerX = playerTarget.position.x;

            Collider2D best = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < cachedLadders.Count; i++)
            {
                Collider2D ladder = cachedLadders[i];
                if (ladder == null || !ladder.enabled || !ladder.gameObject.activeInHierarchy) continue;

                Bounds lb = ladder.bounds;
                float ladderX = lb.center.x;
                float distToMe = Mathf.Abs(ladderX - myX);
                if (distToMe > 18.0f) continue;

                if (needUp)
                {
                    if (!(lb.min.y <= myFeetY + 0.90f && lb.max.y >= myFeetY + 1.50f)) continue;
                }
                else
                {
                    if (!(lb.max.y >= myFeetY - 0.70f && lb.max.y <= myFeetY + 1.15f && lb.min.y < myFeetY - 1.50f)) continue;
                }

                // Verificar que no exista un muro bloqueando el paso horizontal hacia la escalera
                if (distToMe > 0.35f)
                {
                    Vector2 dir = ladderX > myX ? Vector2.right : Vector2.left;
                    RaycastHit2D[] wallHits = Physics2D.RaycastAll(new Vector2(myX, torsoY), dir, distToMe);
                    bool blockedByWall = false;
                    for (int w = 0; w < wallHits.Length; w++)
                    {
                        Collider2D wc = wallHits[w].collider;
                        if (wc == null || wc.isTrigger || wc == mainCollider || wc.transform.IsChildOf(transform)) continue;
                        if (wc.gameObject.name.IndexOf("Wall", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            blockedByWall = true;
                            break;
                        }
                    }
                    if (blockedByWall) continue;
                }

                float score = distToMe + 0.35f * Mathf.Abs(ladderX - playerX);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = ladder;
                }
            }

            return best;
        }

        private void HandleClimbingStep()
        {
            if (targetLadder == null || mainCollider == null || isDead)
            {
                ExitLadderCleanly();
                return;
            }

            rb.gravityScale = 0f;
            float alignedX = Mathf.MoveTowards(transform.position.x, targetLadder.bounds.center.x, 10.0f * Time.fixedDeltaTime);
            transform.position = new Vector3(alignedX, transform.position.y, transform.position.z);
            rb.linearVelocity = new Vector2(0f, climbDirection * climbSpeed);

            IgnorePlatformsAlongLadder(climbDirection);

            float feetY = mainCollider.bounds.min.y;
            if (climbDirection > 0f)
            {
                if (feetY >= targetLadder.bounds.max.y + 0.04f)
                {
                    float pivotToFeet = transform.position.y - feetY;
                    transform.position = new Vector3(transform.position.x, targetLadder.bounds.max.y + pivotToFeet + 0.04f, transform.position.z);
                    ExitLadderCleanly();
                }
            }
            else
            {
                bool reachedBottom = feetY <= targetLadder.bounds.min.y + 0.10f;
                if (!reachedBottom)
                {
                    RaycastHit2D[] groundHits = Physics2D.RaycastAll(new Vector2(mainCollider.bounds.center.x, feetY + 0.05f), Vector2.down, 0.25f);
                    for (int i = 0; i < groundHits.Length; i++)
                    {
                        Collider2D gc = groundHits[i].collider;
                        if (gc == null || gc.isTrigger || gc == mainCollider || gc.transform.IsChildOf(transform)) continue;
                        if (ignoredPlatformColliders.Contains(gc)) continue;
                        if (gc.gameObject.name.StartsWith("Ground_", StringComparison.OrdinalIgnoreCase))
                        {
                            reachedBottom = true;
                            break;
                        }
                    }
                }

                if (reachedBottom)
                {
                    ExitLadderCleanly();
                }
            }
        }

        private void IgnorePlatformsAlongLadder(float climbDir)
        {
            if (targetLadder == null || mainCollider == null) return;

            Bounds lb = targetLadder.bounds;
            Collider2D[] hits = Physics2D.OverlapBoxAll(lb.center, new Vector2(lb.size.x + 1.4f, lb.size.y + 1.2f), 0f);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i];
                if (col == null || col.isTrigger || col == mainCollider) continue;
                if (!col.gameObject.name.StartsWith("Platform_", StringComparison.OrdinalIgnoreCase)) continue;

                // Ignorar únicamente plataformas intersectadas por la escalera (o justo debajo al iniciar descenso)
                bool isAlongShaft = col.bounds.max.y >= lb.min.y + 0.2f && col.bounds.min.y <= lb.max.y + 0.4f;
                if (isAlongShaft || climbDir < 0f)
                {
                    if (!ignoredPlatformColliders.Contains(col))
                    {
                        Physics2D.IgnoreCollision(mainCollider, col, true);
                        ignoredPlatformColliders.Add(col);
                    }
                }
            }
        }

        private void UpdateEnemyPlatformPassThrough()
        {
            if (isClimbingLadder || isDroppingThrough || mainCollider == null) return;

            float feetY = mainCollider.bounds.min.y;
            float headY = mainCollider.bounds.max.y;

            for (int i = ignoredPlatformColliders.Count - 1; i >= 0; i--)
            {
                Collider2D col = ignoredPlatformColliders[i];
                if (col == null)
                {
                    ignoredPlatformColliders.RemoveAt(i);
                    continue;
                }

                bool clearAbove = feetY >= col.bounds.max.y - 0.04f;
                bool clearBelow = headY <= col.bounds.min.y - 0.05f;
                bool farHorizontal = Mathf.Abs(mainCollider.bounds.center.x - col.bounds.center.x) > col.bounds.extents.x + mainCollider.bounds.extents.x + 0.3f;

                if (clearAbove || clearBelow || farHorizontal)
                {
                    Physics2D.IgnoreCollision(mainCollider, col, false);
                    ignoredPlatformColliders.RemoveAt(i);
                }
            }
        }

        private void ExitLadderCleanly()
        {
            isClimbingLadder = false;
            targetLadder = null;
            ladderExitCooldown = 0.45f;
            if (rb != null)
            {
                rb.gravityScale = baseGravityScale;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
        }

        private IEnumerator DropThroughPlatformRoutine(Collider2D platCol)
        {
            dropCooldownTimer = 1.8f;
            if (platCol != null && mainCollider != null)
            {
                isDroppingThrough = true;
                if (!ignoredPlatformColliders.Contains(platCol))
                {
                    ignoredPlatformColliders.Add(platCol);
                }
                Physics2D.IgnoreCollision(mainCollider, platCol, true);
                yield return new WaitForSeconds(0.48f);
                isDroppingThrough = false;
                if (platCol != null && mainCollider != null)
                {
                    Physics2D.IgnoreCollision(mainCollider, platCol, false);
                }
                ignoredPlatformColliders.Remove(platCol);
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (isDead) return;

            currentHealth = Mathf.Max(0, currentHealth - damageInfo.amount);
            Debug.Log($"<color=green><b>[ENEMIGO IMPACTADO]</b> Recibió {damageInfo.amount} daño PEM. Salud restante: {currentHealth}/{maxHealth}</color>");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                GameEvents.PlaySfx(SfxType.EnemyHit);
            }
        }

        private void OnDestroy()
        {
            if (mainCollider == null) return;
            for (int i = 0; i < ignoredPlatformColliders.Count; i++)
            {
                if (ignoredPlatformColliders[i] != null)
                {
                    Physics2D.IgnoreCollision(mainCollider, ignoredPlatformColliders[i], false);
                }
            }
            ignoredPlatformColliders.Clear();
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            GameEvents.PlaySfx(SfxType.EnemyDestroyed);
            Debug.Log("<color=red><b>[ENEMIGO DESTRUIDO]</b> Drone S.I.A. sobrecargado y eliminado.</color>");

            if (explosionVfxPrefab != null)
            {
                Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleContactDamage(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            HandleContactDamage(collision);
        }

        private void HandleContactDamage(Collision2D collision)
        {
            if (isDead || Time.time < lastDamageTime + DamageCooldown) return;

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.IsInvulnerable || !playerHealth.IsAlive) return;

                lastDamageTime = Time.time;
                Vector2 hitPoint = collision.contactCount > 0 ? collision.contacts[0].point : (Vector2)transform.position;
                Vector2 hitNormal = collision.contactCount > 0 ? collision.contacts[0].normal : Vector2.up;
                DamageInfo damage = new DamageInfo(contactDamage, gameObject, hitPoint, hitNormal);
                playerHealth.TakeDamage(damage);

                // Retroceso también para el enemigo tras golpear al jugador para evitar empalmarse
                knockbackTimer = 0.36f;
                float recoilDir = (transform.position.x <= collision.transform.position.x) ? -1f : 1f;
                rb.linearVelocity = new Vector2(recoilDir * 4.6f, 2.2f);
            }
        }
    }
}
