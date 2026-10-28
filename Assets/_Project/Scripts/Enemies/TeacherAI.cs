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

        [Tooltip("Daño físico que causa el profesor al tocar al estudiante.")]
        [SerializeField] private int contactDamage = 20;

        [Header("--- Ataque a Distancia del Jefe (Boss Ranged Attack) ---")]
        [Tooltip("Si está activo, el Jefe lanza proyectiles de energía a distancia al detectar al jugador.")]
        [SerializeField] private bool canRangedAttack = true;

        [Tooltip("Intervalo en segundos entre disparos a distancia del Jefe.")]
        [SerializeField] private float rangedAttackInterval = 2.1f;

        [Tooltip("Daño del proyectil de energía del Jefe.")]
        [SerializeField] private int rangedDamage = 15;

        [Tooltip("Velocidad del proyectil de energía del Jefe.")]
        [SerializeField] private float projectileSpeed = 11.5f;

        [Tooltip("Sprite opcional para el proyectil de energía del Jefe (Spells-Effect).")]
        [SerializeField] private Sprite projectileSprite;

        [Header("--- Recompensa tras Des-corrupción ---")]
        [Tooltip("Prefab de objeto que suelta el profesor al ser liberado (ej. café o tarjeta de memoria).")]
        [SerializeField] private GameObject dropItemPrefab;

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
        private const float DefaultPatrolRadius = 7.0f;

        public TeacherState CurrentState => currentState;

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

            // Ejecutar ataque a distancia si está en estado de persecución y tiene ataque a distancia activo
            if (currentState == TeacherState.Chasing && canRangedAttack && playerTarget != null && Time.time >= nextRangedAttackTime)
            {
                nextRangedAttackTime = Time.time + rangedAttackInterval;
                FireRangedProjectileAtPlayer();
            }
        }

        private void FixedUpdate()
        {
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

        private void HandleBraceletHealthChanged(int current, int max)
        {
            if (current < max && current > 0 && animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Hurt");
            }
        }

        private void FireRangedProjectileAtPlayer()
        {
            if (playerTarget == null) return;

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetTrigger("Attack");
            }

            GameEvents.PlaySfx(SfxType.BossCast);

            float dirX = (playerTarget.position.x >= transform.position.x) ? 1f : -1f;
            float spawnHeightOffset = mainCollider != null ? mainCollider.bounds.extents.y * 0.95f : 1.5f;
            Vector3 spawnPos = transform.position + new Vector3(dirX * 1.4f, spawnHeightOffset, 0f);
            Vector2 targetCenter = (Vector2)playerTarget.position + Vector2.up * 1.0f;
            Vector2 aimDir = (targetCenter - (Vector2)spawnPos).normalized;

            GameObject projObj = new GameObject("Boss_Energy_Bolt");
            projObj.transform.position = spawnPos;
            projObj.transform.localScale = new Vector3(1.35f, 1.35f, 1f);

            SpriteRenderer sr = projObj.AddComponent<SpriteRenderer>();
            if (projectileSprite != null)
            {
                sr.sprite = projectileSprite;
                sr.color = new Color(1.0f, 0.35f, 0.95f, 1.0f);
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
                sr.color = new Color(1.0f, 0.2f, 0.85f, 1.0f);
            }
            sr.sortingOrder = 9;

            CircleCollider2D col = projObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.38f;

            EMPPulse pulse = projObj.AddComponent<EMPPulse>();
            pulse.Initialize(aimDir, rangedDamage, gameObject, isEnemyShot: true, customSpeed: projectileSpeed);
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
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            // Soltar recompensa (café, cables o tarjeta)
            if (dropItemPrefab != null)
            {
                Instantiate(dropItemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            // Pausa de 1 segundo para que el profesor "recobre el sentido"
            yield return new WaitForSeconds(1.2f);

            // Huir hacia el lado despejado
            if (playerTarget != null)
            {
                movingRight = transform.position.x > playerTarget.position.x;
                UpdateFlip();
            }

            currentState = TeacherState.Fleeing;

            // Destruir al salir de escena tras unos segundos
            Destroy(gameObject, 4.0f);
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
                lastContactDamageTime = Time.time;
                Vector2 hitPoint = collision.contactCount > 0 ? collision.contacts[0].point : (Vector2)transform.position;
                Vector2 hitNormal = collision.contactCount > 0 ? collision.contacts[0].normal : Vector2.up;
                DamageInfo damage = new DamageInfo(contactDamage, gameObject, hitPoint, hitNormal);
                playerHealth.TakeDamage(damage);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
