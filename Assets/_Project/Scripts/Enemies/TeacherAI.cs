using System.Collections;
using UnityEngine;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Player;
using CodigoDeEmergencia.Combat;

namespace CodigoDeEmergencia.Enemies
{
    /// <summary>
    /// Inteligencia Artificial para los docentes bajo el control de la IA S.I.A.
    /// Implementa:
    /// - Rutina de patrullaje entre waypoints o por detección de obstáculos.
    /// - Persecución al detectar al estudiante en su cono de visión.
    /// - Daño por contacto al jugador.
    /// - Estado de Des-corrupción: al sobrecargarse la pulsera, el profesor se libera del trance y huye.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(BraceletHealth))]
    public class TeacherAI : MonoBehaviour
    {
        public enum TeacherState
        {
            Patrolling,
            Chasing,
            DeCorrupted,
            Fleeing
        }

        [Header("--- Movimiento y Patrulla ---")]
        [Tooltip("Velocidad normal de patrullaje en trance.")]
        [SerializeField] private float patrolSpeed = 2.5f;

        [Tooltip("Velocidad al perseguir al estudiante.")]
        [SerializeField] private float chaseSpeed = 4.5f;

        [Tooltip("Velocidad de huida tras ser liberado del trance.")]
        [SerializeField] private float fleeSpeed = 6.0f;

        [Tooltip("Puntos de patrullaje opcionales.")]
        [SerializeField] private Transform[] patrolWaypoints;

        [Header("--- Detección y Combate ---")]
        [Tooltip("Distancia de visión para detectar al jugador (13.5m = distancia de pantalla compartida).")]
        [SerializeField] private float detectionRange = 13.5f;

        [Tooltip("Capa donde se encuentra el jugador.")]
        [SerializeField] private LayerMask playerLayer;

        [Tooltip("Capa de obstáculos que bloquean la línea de visión.")]
        [SerializeField] private LayerMask obstacleLayer;

        [Tooltip("Daño físico que causa el profesor al tocar al estudiante (escala 0-100).")]
        [SerializeField] private int contactDamage = 12;

        [Header("--- Configuración de Jefe Flotante y Arena ---")]
        [Tooltip("Si está activo, el Jefe levita permanentemente en el aire sin tocar el suelo para que el jugador pase por debajo.")]
        [SerializeField] private bool isFloatingBoss = false;

        [Tooltip("Altura Y en el mundo a la que flota el Jefe por encima del suelo.")]
        [SerializeField] private float floatingTargetY = 2.45f;

        [Tooltip("Radio horizontal de desplazamiento aéreo dentro de la Arena del Jefe.")]
        [SerializeField] private float arenaPatrolRadius = 12.5f;

        [Header("--- Ataque a Distancia del Jefe (3 Patrones Aleatorios) ---")]
        [Tooltip("Si está activo, el Jefe lanza proyectiles de energía a distancia al detectar al jugador.")]
        [SerializeField] private bool canRangedAttack = true;

        [Tooltip("Intervalo en segundos entre ataques a distancia del Jefe.")]
        [SerializeField] private float rangedAttackInterval = 2.5f;

        [Tooltip("Daño del proyectil de energía del Jefe (escala 0-100).")]
        [SerializeField] private int rangedDamage = 10;

        [Tooltip("Velocidad base del proyectil de energía del Jefe.")]
        [SerializeField] private float projectileSpeed = 9.2f;

        [Tooltip("Sprite opcional para el proyectil de energía del Jefe (Spells-Effect).")]
        [SerializeField] private Sprite projectileSprite;

        [Header("--- Recompensa y Destransformación a Humano tras Des-corrupción ---")]
        [Tooltip("Prefab de objeto que suelta el profesor al ser liberado (ej. café o tarjeta de memoria).")]
        [SerializeField] private GameObject dropItemPrefab;

        [Tooltip("Sprite del Docente Humano en reposo cuando se destransforma al ser liberado del control S.I.A.")]
        [SerializeField] private Sprite humanIdleSprite;

        [Tooltip("Sprite del Docente Humano corriendo cuando agradece y se retira hacia la izquierda.")]
        [SerializeField] private Sprite humanRunSprite;

        [Tooltip("Fotogramas de animación Idle del Docente Humano liberado.")]
        [SerializeField] private Sprite[] humanIdleFrames;

        [Tooltip("Fotogramas de animación Run (6 cuadros) del Docente Humano liberado.")]
        [SerializeField] private Sprite[] humanRunFrames;

        // Componentes y referencias
        private Rigidbody2D rb;
        private Collider2D mainCollider;
        private BraceletHealth bracelet;
        private Animator animator;
        private Transform playerTarget;

        // Estado
        private TeacherState currentState = TeacherState.Patrolling;
        private int currentWaypointIndex;
        private bool movingRight = true;
        private float startPosX;
        private float nextRangedAttackTime;
        private int lastAttackPattern = -1;
        private const float DefaultPatrolRadius = 7.0f;

        public TeacherState CurrentState => currentState;
        public bool IsFloatingBoss => isFloatingBoss;

        public void ConfigureAsFloatingBoss(float hoverY, float arenaRadius, int projectileDmg, float attackCooldown)
        {
            isFloatingBoss = true;
            canRangedAttack = true;
            floatingTargetY = hoverY;
            arenaPatrolRadius = arenaRadius;
            rangedDamage = projectileDmg;
            rangedAttackInterval = attackCooldown;
            detectionRange = 28.0f;
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mainCollider = GetComponent<Collider2D>();
            bracelet = GetComponent<BraceletHealth>();
            animator = GetComponentInChildren<Animator>();

            if (mainCollider != null && mainCollider.sharedMaterial == null)
            {
                mainCollider.sharedMaterial = new PhysicsMaterial2D("TeacherZeroFriction")
                {
                    friction = 0f,
                    bounciness = 0f
                };
            }
        }

        private void OnEnable()
        {
            if (bracelet != null)
            {
                bracelet.OnOverloaded += HandleBraceletOverloaded;
                bracelet.OnHealthChanged += HandleBraceletHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (bracelet != null)
            {
                bracelet.OnOverloaded -= HandleBraceletOverloaded;
                bracelet.OnHealthChanged -= HandleBraceletHealthChanged;
            }
        }

        private void Start()
        {
            startPosX = transform.position.x;

            if (isFloatingBoss && rb != null)
            {
                rb.gravityScale = 0f;
                transform.position = new Vector3(transform.position.x, floatingTargetY, transform.position.z);
            }

            // Buscar al jugador por tag
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }

        private void Update()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetFloat("Speed", rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f);
                animator.SetBool("IsDead", currentState == TeacherState.DeCorrupted);
            }

            if (currentState == TeacherState.DeCorrupted) return;

            if (currentState != TeacherState.Fleeing)
            {
                CheckForPlayerDetection();
            }

            // Ejecutar patrón de ataque aleatorio a distancia si está en estado de persecución
            if (currentState == TeacherState.Chasing && canRangedAttack && playerTarget != null && Time.time >= nextRangedAttackTime)
            {
                nextRangedAttackTime = Time.time + rangedAttackInterval;
                ExecuteBossRangedAttack();
            }
        }

        private void FixedUpdate()
        {
            if (isFloatingBoss && rb != null && currentState != TeacherState.DeCorrupted)
            {
                rb.gravityScale = 0f;
                ExecuteFloatingBossMovement();
                return;
            }

            switch (currentState)
            {
                case TeacherState.Patrolling:
                    ExecutePatrol();
                    break;
                case TeacherState.Chasing:
                    ExecuteChase();
                    break;
                case TeacherState.Fleeing:
                    ExecuteFlee();
                    break;
            }
        }

        private void ExecuteFloatingBossMovement()
        {
            // El Jefe levita permanentemente en el aire (nunca toca el suelo) para que el jugador pase libremente por debajo
            float desiredY = floatingTargetY + Mathf.Sin(Time.time * 2.1f) * 0.35f;
            float velY = (desiredY - transform.position.y) * 5.0f;

            if (movingRight && transform.position.x >= startPosX + arenaPatrolRadius)
            {
                movingRight = false;
            }
            else if (!movingRight && transform.position.x <= startPosX - arenaPatrolRadius)
            {
                movingRight = true;
            }

            float speedX = (currentState == TeacherState.Chasing) ? chaseSpeed : patrolSpeed;
            float moveX = movingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(moveX * speedX, velY);

            // Mirar hacia el jugador mientras ataca en el aire
            if (playerTarget != null && currentState == TeacherState.Chasing)
            {
                bool facePlayerRight = playerTarget.position.x >= transform.position.x;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (facePlayerRight ? 1f : -1f);
                transform.localScale = scale;
            }
            else
            {
                UpdateFlip();
            }
        }

        private void HandleBraceletHealthChanged(int current, int max)
        {
            if (current < max && current > 0 && animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Hurt");
            }
        }

        private void ExecuteBossRangedAttack()
        {
            if (playerTarget == null) return;

            if (!isFloatingBoss)
            {
                FireSingleBoltTowardPlayer(0f, projectileSpeed);
                return;
            }

            // Elegir entre 3 patrones de ataque aleatorios sin repetir más de dos veces seguidas
            int pattern = Random.Range(0, 3);
            if (pattern == lastAttackPattern)
            {
                pattern = (pattern + Random.Range(1, 3)) % 3;
            }
            lastAttackPattern = pattern;

            switch (pattern)
            {
                case 0:
                    // Patrón 1: Disparo Dirigido Doble hacia la posición del jugador
                    StartCoroutine(AimedDoubleBoltRoutine());
                    break;
                case 1:
                    // Patrón 2: Ráfaga en Abanico de 10 Disparos que cubren el área con huecos esquivables
                    FireTenShotAreaBarrage();
                    break;
                case 2:
                    // Patrón 3: Barrido Aéreo Secuencial
                    StartCoroutine(AerialSweepingVolleyRoutine());
                    break;
            }
        }

        private IEnumerator AimedDoubleBoltRoutine()
        {
            FireSingleBoltTowardPlayer(0f, projectileSpeed);
            yield return new WaitForSeconds(0.28f);
            if (currentState == TeacherState.Chasing && playerTarget != null)
            {
                FireSingleBoltTowardPlayer(6f, projectileSpeed * 1.05f);
            }
        }

        private void FireTenShotAreaBarrage()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Attack");
            }

            GameEvents.PlaySfx(SfxType.BossCast);
            Vector3 spawnPos = transform.position + new Vector3(0f, -0.35f, 0f);

            // 10 disparos distribuidos en un abanico inferior (-162° a -18°, separación de 16° entre proyectiles)
            // Cubren un área amplia de la arena pero dejan huecos claros para que el jugador esquive o haga Dash
            const int shotCount = 10;
            float startAngleDeg = -162f;
            float endAngleDeg = -18f;
            float step = (endAngleDeg - startAngleDeg) / (shotCount - 1);

            for (int i = 0; i < shotCount; i++)
            {
                float angleDeg = startAngleDeg + step * i;
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
                SpawnBossProjectile(spawnPos, dir, projectileSpeed * 0.78f, new Color(1.0f, 0.32f, 0.75f, 1.0f), 1.15f);
            }

            Debug.Log("<color=magenta><b>[JEFE S.I.A.]</b> ¡Patrón Ráfaga de Área de 10 Disparos ejecutado!</color>");
        }

        private IEnumerator AerialSweepingVolleyRoutine()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Attack");
            }

            GameEvents.PlaySfx(SfxType.BossCast);
            Debug.Log("<color=magenta><b>[JEFE S.I.A.]</b> ¡Patrón Barrido Aéreo Secuencial ejecutado!</color>");

            for (int i = 0; i < 5; i++)
            {
                if (currentState != TeacherState.Chasing || playerTarget == null) yield break;

                float angleOffset = -24f + (i * 12f);
                FireSingleBoltTowardPlayer(angleOffset, projectileSpeed * 0.88f);
                yield return new WaitForSeconds(0.18f);
            }
        }

        private void FireSingleBoltTowardPlayer(float angleOffsetDeg, float customSpd)
        {
            if (playerTarget == null) return;

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Attack");
            }

            GameEvents.PlaySfx(SfxType.BossCast);

            float dirX = (playerTarget.position.x >= transform.position.x) ? 1f : -1f;
            Vector3 spawnPos = transform.position + new Vector3(dirX * 1.1f, isFloatingBoss ? -0.2f : 1.2f, 0f);
            Vector2 targetCenter = (Vector2)playerTarget.position + Vector2.up * 0.9f;
            Vector2 baseDir = (targetCenter - (Vector2)spawnPos).normalized;

            if (Mathf.Abs(angleOffsetDeg) > 0.01f)
            {
                float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg + angleOffsetDeg;
                float rad = baseAngle * Mathf.Deg2Rad;
                baseDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            }

            SpawnBossProjectile(spawnPos, baseDir, customSpd, new Color(1.0f, 0.35f, 0.95f, 1.0f), 1.3f);
        }

        private void SpawnBossProjectile(Vector3 spawnPos, Vector2 aimDir, float spd, Color tint, float scale)
        {
            GameObject projObj = new GameObject("Boss_Energy_Bolt");
            projObj.transform.position = spawnPos;
            projObj.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = projObj.AddComponent<SpriteRenderer>();
            if (projectileSprite != null)
            {
                sr.sprite = projectileSprite;
                sr.color = tint;
            }
            else
            {
                Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                        tex.SetPixel(x, y, d <= 7.5f ? Color.white : Color.clear);
                    }
                }
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 24f);
                sr.color = tint;
            }
            sr.sortingOrder = 9;

            CircleCollider2D col = projObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.34f;

            Rigidbody2D rb = projObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            EMPPulse pulse = projObj.AddComponent<EMPPulse>();
            pulse.Initialize(aimDir, rangedDamage, gameObject, isEnemyShot: true, customSpeed: spd);
        }

        private void CheckForPlayerDetection()
        {
            if (playerTarget == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= detectionRange)
            {
                Vector2 origin = (Vector2)transform.position + Vector2.up * 1.5f;
                Vector2 targetPos = (Vector2)playerTarget.position + Vector2.up * 1.0f;
                Vector2 directionToPlayer = (targetPos - origin).normalized;

                // El Jefe detecta al jugador en cuanto ambos comparten pantalla (<= detectionRange), sin importar hacia dónde miraba
                bool isPlayerInFront = canRangedAttack || (movingRight && directionToPlayer.x >= -0.1f) || (!movingRight && directionToPlayer.x <= 0.1f) || distanceToPlayer <= 4.0f;

                if (isPlayerInFront)
                {
                    bool blockedByWall = false;
                    if (obstacleLayer.value != 0)
                    {
                        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, directionToPlayer, distanceToPlayer, obstacleLayer);
                        for (int i = 0; i < hits.Length; i++)
                        {
                            Collider2D col = hits[i].collider;
                            if (col == null || col.isTrigger) continue;
                            if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;
                            if (col.gameObject == playerTarget.gameObject || col.transform.IsChildOf(playerTarget)) continue;
                            if (col.GetComponentInParent<IDamageable>() != null) continue;

                            blockedByWall = true;
                            break;
                        }
                    }

                    if (!blockedByWall)
                    {
                        if (currentState != TeacherState.Chasing)
                        {
                            currentState = TeacherState.Chasing;
                            nextRangedAttackTime = Time.time + 0.65f;
                            if (bracelet != null)
                            {
                                GameEvents.TriggerBraceletDamaged(bracelet.CurrentHealth, bracelet.MaxHealth, transform);
                            }
                        }
                        return;
                    }
                }
            }

            // Si el jugador se aleja fuera de la pantalla, volver a patrullar
            if (currentState == TeacherState.Chasing && distanceToPlayer > detectionRange * 1.4f)
            {
                currentState = TeacherState.Patrolling;
                startPosX = transform.position.x;
            }
        }

        private void ExecutePatrol()
        {
            if (patrolWaypoints != null && patrolWaypoints.Length > 1)
            {
                Transform targetWaypoint = patrolWaypoints[currentWaypointIndex];
                Vector2 direction = (targetWaypoint.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(direction.x * patrolSpeed, rb.linearVelocity.y);

                if (Mathf.Abs(transform.position.x - targetWaypoint.position.x) < 0.3f)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
                    movingRight = patrolWaypoints[currentWaypointIndex].position.x > transform.position.x;
                    UpdateFlip();
                }
            }
            else
            {
                // Patrullaje en zona alrededor de su posición base
                if (movingRight && transform.position.x >= startPosX + DefaultPatrolRadius)
                {
                    movingRight = false;
                    UpdateFlip();
                }
                else if (!movingRight && transform.position.x <= startPosX - DefaultPatrolRadius)
                {
                    movingRight = true;
                    UpdateFlip();
                }

                float moveX = movingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(moveX * patrolSpeed, rb.linearVelocity.y);

                // Detección de pared frontal si obstacleLayer está configurado
                if (obstacleLayer.value != 0)
                {
                    Vector2 rayOrigin = (Vector2)transform.position + new Vector2(moveX * 0.6f, 1.0f);
                    RaycastHit2D wallCheck = Physics2D.Raycast(rayOrigin, Vector2.right * moveX, 0.4f, obstacleLayer);
                    if (wallCheck.collider != null && !wallCheck.collider.isTrigger && wallCheck.collider.gameObject != gameObject)
                    {
                        movingRight = !movingRight;
                        UpdateFlip();
                    }
                }
            }
        }

        private void ExecuteChase()
        {
            if (playerTarget == null) return;

            float directionX = (playerTarget.position.x > transform.position.x) ? 1f : -1f;
            rb.linearVelocity = new Vector2(directionX * chaseSpeed, rb.linearVelocity.y);

            bool shouldFaceRight = directionX > 0f;
            if (movingRight != shouldFaceRight)
            {
                movingRight = shouldFaceRight;
                UpdateFlip();
            }
        }

        private void ExecuteFlee()
        {
            // Huye en dirección opuesta al jugador
            float fleeDirection = movingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(fleeDirection * fleeSpeed, rb.linearVelocity.y);
        }

        private void UpdateFlip()
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (movingRight ? 1f : -1f);
            transform.localScale = scale;
        }

        private void HandleBraceletOverloaded()
        {
            StartCoroutine(DeCorruptionSequence());
        }

        private IEnumerator DeCorruptionSequence()
        {
            currentState = TeacherState.DeCorrupted;
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }
            if (mainCollider != null) mainCollider.enabled = false;

            if (dropItemPrefab != null)
            {
                Instantiate(dropItemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            Transform floatingBar = transform.Find("FloatingHealthBar");
            if (floatingBar != null) floatingBar.gameObject.SetActive(false);

            if (isFloatingBoss)
            {
                // Limpiar cualquier proyectil enemigo activo para que el jugador no reciba daño durante la cinemática
                EMPPulse[] activePulses = FindObjectsByType<EMPPulse>();
                for (int i = 0; i < activePulses.Length; i++)
                {
                    if (activePulses[i] != null) Destroy(activePulses[i].gameObject);
                }

                // Delegar al HUDController la cinemática interactiva completa de liberación del Jefe (Tutorial y Nivel 1)
                GameEvents.TriggerBossLiberationCutscene(transform);
                yield break;
            }

            // PATRULLEROS INTERMEDIOS:
            // 1. Caer primero hasta el suelo sólido más cercano (sin atravesar el piso hacia el vacío)
            float groundY = FindSolidGroundBelow(transform.position, -1.75f);
            if (transform.position.y > groundY + 0.04f)
            {
                float fallVel = 0f;
                while (transform.position.y > groundY + 0.02f)
                {
                    fallVel += 26.0f * Time.deltaTime;
                    float nextY = Mathf.Max(groundY, transform.position.y - fallVel * Time.deltaTime);
                    transform.position = new Vector3(transform.position.x, nextY, transform.position.z);
                    if (sr != null)
                    {
                        sr.color = (Mathf.FloorToInt(Time.time * 20f) % 2 == 0)
                            ? new Color(0.35f, 1.0f, 1.0f, 1f)
                            : Color.white;
                    }
                    yield return null;
                }
            }
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

            // 2. Transformarse a humano sobre el suelo (sin diálogo ni agradecimiento)
            if (animator != null) animator.enabled = false;
            StartCoroutine(SpawnPurificationWaveRoutine(transform.position + Vector3.up * 1.1f, 2.4f));

            Sprite patrolHumanIdle = (humanIdleFrames != null && humanIdleFrames.Length > 0 && humanIdleFrames[0] != null)
                ? humanIdleFrames[0]
                : humanIdleSprite;

            if (sr != null && patrolHumanIdle != null)
            {
                sr.sprite = patrolHumanIdle;
                sr.color = new Color(0.45f, 1.0f, 1.0f, 1f);
            }

            Transform braceletGlow = transform.Find("Bracelet_SIA_Glow");
            if (braceletGlow != null) braceletGlow.gameObject.SetActive(false);

            movingRight = false;
            transform.localScale = new Vector3(-1.15f, 1.15f, 1f);

            float flashTimer = 0f;
            const float transformFlashDuration = 0.35f;
            while (flashTimer < transformFlashDuration)
            {
                flashTimer += Time.deltaTime;
                float t = Mathf.Clamp01(flashTimer / transformFlashDuration);
                if (sr != null) sr.color = Color.Lerp(new Color(0.45f, 1.0f, 1.0f, 1f), Color.white, t);
                yield return null;
            }
            if (sr != null) sr.color = Color.white;

            // 3. Salir corriendo hacia la izquierda sobre el piso y desaparecer en cuanto salga de pantalla
            float runTimer = 0f;
            float runVelY = 0f;
            Camera mainCam = Camera.main;

            while (runTimer < 6.0f)
            {
                runTimer += Time.deltaTime;
                transform.position += Vector3.left * (8.8f * Time.deltaTime);

                float stepGroundY = FindSolidGroundBelow(transform.position, -1.75f);
                if (transform.position.y > stepGroundY + 0.03f)
                {
                    runVelY += 26.0f * Time.deltaTime;
                    float nextY = Mathf.Max(stepGroundY, transform.position.y - runVelY * Time.deltaTime);
                    transform.position = new Vector3(transform.position.x, nextY, transform.position.z);
                }
                else
                {
                    runVelY = 0f;
                    transform.position = new Vector3(transform.position.x, stepGroundY, transform.position.z);
                }

                if (sr != null)
                {
                    if (humanRunFrames != null && humanRunFrames.Length > 0)
                    {
                        int runIdx = Mathf.FloorToInt(runTimer * 14f) % humanRunFrames.Length;
                        if (humanRunFrames[runIdx] != null) sr.sprite = humanRunFrames[runIdx];
                    }
                    else if (humanRunSprite != null && humanIdleSprite != null)
                    {
                        sr.sprite = (Mathf.FloorToInt(runTimer * 12f) % 2 == 0) ? humanRunSprite : humanIdleSprite;
                    }
                }

                if (mainCam != null)
                {
                    Vector3 vp = mainCam.WorldToViewportPoint(transform.position);
                    if (vp.x < -0.08f || vp.x > 1.08f || vp.y < -0.15f || vp.y > 1.15f)
                    {
                        break;
                    }
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private float FindSolidGroundBelow(Vector2 originPos, float fallbackY)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(originPos.x, originPos.y + 0.35f), Vector2.down, 25f);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i].collider;
                if (col == null || col.isTrigger) continue;
                if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;
                if (col.GetComponentInParent<IDamageable>() != null) continue;
                return hits[i].point.y;
            }
            return fallbackY;
        }

        private IEnumerator SpawnPurificationWaveRoutine(Vector3 center, float maxScale)
        {
            GameObject ring = new GameObject("Purification_EMP_Wave");
            ring.transform.position = center;
            ring.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            SpriteRenderer sr = ring.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 31.5f;
                    float ringAlpha = 1f - Mathf.Abs(d - 0.78f) / 0.16f;
                    tex.SetPixel(x, y, ringAlpha > 0f ? new Color(0.35f, 0.98f, 1f, Mathf.Clamp01(ringAlpha)) : Color.clear);
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 32f);
            sr.sortingOrder = 14;

            float t = 0f;
            while (t < 0.48f && ring != null)
            {
                t += Time.deltaTime;
                float p = t / 0.48f;
                float s = Mathf.Lerp(0.4f, maxScale, Mathf.SmoothStep(0f, 1f, p));
                ring.transform.localScale = new Vector3(s, s, 1f);
                sr.color = new Color(0.35f, 0.98f, 1f, Mathf.Lerp(0.85f, 0f, p));
                yield return null;
            }

            if (ring != null) Destroy(ring);
        }

        private float lastContactDamageTime;
        private const float TeacherDamageCooldown = 1.2f;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleTeacherContactDamage(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            HandleTeacherContactDamage(collision);
        }

        private void HandleTeacherContactDamage(Collision2D collision)
        {
            if (currentState == TeacherState.DeCorrupted || currentState == TeacherState.Fleeing) return;
            if (Time.time < lastContactDamageTime + TeacherDamageCooldown) return;

            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (playerHealth.IsInvulnerable || !playerHealth.IsAlive) return;

                lastContactDamageTime = Time.time;
                Vector2 hitPoint = collision.contactCount > 0 ? collision.contacts[0].point : (Vector2)transform.position;
                Vector2 hitNormal = collision.contactCount > 0 ? collision.contacts[0].normal : Vector2.up;
                DamageInfo damage = new DamageInfo(contactDamage, gameObject, hitPoint, hitNormal);
                playerHealth.TakeDamage(damage);

                if (!isFloatingBoss && rb != null)
                {
                    float recoilDir = (transform.position.x <= collision.transform.position.x) ? -1f : 1f;
                    rb.linearVelocity = new Vector2(recoilDir * 4.2f, 2.0f);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
