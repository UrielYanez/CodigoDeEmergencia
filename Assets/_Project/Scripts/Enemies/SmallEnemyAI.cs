using System;
using UnityEngine;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Player;

namespace CodigoDeEmergencia.Enemies
{
    /// <summary>
    /// Mini-enemigo del Sistema S.I.A. (50 HP).
    /// Aparece por los flancos o cayendo del techo.
    /// Persigue al jugador y recibe daño de pulsos PEM normales (10 dmg) o cargados (30 dmg).
    /// Incluye cooldown de daño por contacto y logs informativos para depuración.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class SmallEnemyAI : MonoBehaviour, IDamageable
    {
        [Header("--- Parámetros del Mini-Enemigo ---")]
        [Tooltip("Salud máxima del enemigo (50 HP).")]
        [SerializeField] private int maxHealth = 50;

        [Tooltip("Salud actual.")]
        [SerializeField] private int currentHealth = 50;

        [Tooltip("Velocidad de persecución terrestre.")]
        [SerializeField] private float moveSpeed = 3.6f;

        [Tooltip("Daño por contacto al jugador.")]
        [SerializeField] private int contactDamage = 10;

        [Tooltip("Efecto de chispas/explosión al morir.")]
        [SerializeField] private GameObject explosionVfxPrefab;

        private Rigidbody2D rb;
        private Collider2D mainCollider;
        private Transform playerTarget;
        private bool isDead;
        private float lastDamageTime;
        private float knockbackTimer;
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
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;

                // Si el jugador está en pleno Dash o Invulnerabilidad al aparecer este enemigo, sincronizar colisión
                PlayerDash pd = playerObj.GetComponent<PlayerDash>();
                PlayerHealth ph = playerObj.GetComponent<PlayerHealth>();
                if (pd != null && ((pd.IsDashing) || (ph != null && ph.IsInvulnerable)))
                {
                    pd.SetIgnoreEnemyCollisions(true);
                }
            }

            Debug.Log($"<color=yellow>[ENEMIGO S.I.A.] Drone activado en X={transform.position.x:F1}, Y={transform.position.y:F1} con {maxHealth} HP.</color>");
        }

        private void Update()
        {
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (isDead || playerTarget == null) return;
            if (knockbackTimer > 0f) return;

            float distToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distToPlayer > 16.5f)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }

            // Moverse hacia la posición del jugador
            float diffX = playerTarget.position.x - transform.position.x;
            float directionX = Mathf.Abs(diffX) > 0.1f ? Mathf.Sign(diffX) : 0f;
            rb.linearVelocity = new Vector2(directionX * moveSpeed, rb.linearVelocity.y);

            // Orientación horizontal (Flip)
            if (Mathf.Abs(directionX) > 0.01f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (directionX > 0 ? 1f : -1f);
                transform.localScale = scale;
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (isDead) return;

            currentHealth = Mathf.Max(0, currentHealth - damageInfo.amount);
            Debug.Log($"<color=green><b>[ENEMIGO IMPACTADO]</b> Recibió {damageInfo.amount} daño PEM. Salud restante: {currentHealth}/{maxHealth}</color>");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Retroceso (Knockback) con temporizador para que FixedUpdate no lo sobrescriba en el siguiente frame
            if (damageInfo.damageSource != null)
            {
                knockbackTimer = 0.18f;
                float knockDir = (transform.position.x >= damageInfo.damageSource.transform.position.x) ? 1f : -1f;
                rb.linearVelocity = new Vector2(knockDir * 5.0f, 3.0f);
            }

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                GameEvents.PlaySfx(SfxType.EnemyHit);
            }
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
                lastDamageTime = Time.time;
                Vector2 hitPoint = collision.contactCount > 0 ? collision.contacts[0].point : (Vector2)transform.position;
                Vector2 hitNormal = collision.contactCount > 0 ? collision.contacts[0].normal : Vector2.up;
                DamageInfo damage = new DamageInfo(contactDamage, gameObject, hitPoint, hitNormal);
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
