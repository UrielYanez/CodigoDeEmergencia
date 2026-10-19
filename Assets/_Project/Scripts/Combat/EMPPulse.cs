using UnityEngine;
using CodigoDeEmergencia.Core;

namespace CodigoDeEmergencia.Combat
{
    /// <summary>
    /// Proyectil de pulso electromagnético (PEM) disparado por el protagonista.
    /// Viaja en línea recta e interactúa únicamente con interfaces IDamageable
    /// marcando el daño como electromagnético (no hiere físicamente a las personas).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EMPPulse : MonoBehaviour
    {
        [Header("--- Parámetros del Proyectil ---")]
        [Tooltip("Velocidad de traslación del pulso.")]
        [SerializeField] private float speed = 16f;

        [Tooltip("Tiempo máximo de vida antes de desvanecerse (en segundos).")]
        [SerializeField] private float lifetime = 1.8f;

        [Tooltip("Prefab opcional de partículas o destello al impactar.")]
        [SerializeField] private GameObject impactVfxPrefab;

        [Tooltip("Radio del haz de colisión continuo para no atravesar plataformas ni muros.")]
        [SerializeField] private float collisionRadius = 0.30f;

        private Vector2 moveDirection;
        private int damageAmount;
        private GameObject ownerObject;
        private bool isEnemyProjectile;
        private bool hasHit;

        private void Awake()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.simulated = true;
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (hasHit) return;

            // Salvaguarda: si excede los límites exteriores del mapa, detonar
            if (transform.position.x < -50f || transform.position.x > 1300f || transform.position.y < -15f || transform.position.y > 100f)
            {
                TriggerImpact();
                return;
            }

            float stepDistance = speed * Time.deltaTime;
            Vector2 currentPos = transform.position;

            // Detección continua de impacto con CircleCast para que NINGÚN pulso atraviese plataformas ni muros
            RaycastHit2D[] hits = Physics2D.CircleCastAll(currentPos, collisionRadius, moveDirection, stepDistance);
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.collider == null || hit.collider.gameObject == gameObject) continue;

                // Ignorar al emisor (dueño) del disparo
                if (ownerObject != null && (hit.collider.gameObject == ownerObject || hit.collider.transform.IsChildOf(ownerObject.transform)))
                {
                    continue;
                }

                // Ignorar otros proyectiles
                if (hit.collider.GetComponent<EMPPulse>() != null)
                {
                    continue;
                }

                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

                // Si es un trigger sin IDamageable (como escaleras, checkpoints o zonas de audio), el pulso lo ignora
                if (hit.collider.isTrigger && damageable == null)
                {
                    continue;
                }

                // Si impacta con una entidad con IDamageable (enemigos, jefe o jugador)
                if (damageable != null)
                {
                    bool isTargetPlayer = hit.collider.CompareTag("Player") || (hit.collider.transform.root != null && hit.collider.transform.root.CompareTag("Player"));
                    if (isEnemyProjectile && !isTargetPlayer)
                    {
                        continue;
                    }

                    DamageInfo info = new DamageInfo(
                        damageAmount,
                        ownerObject,
                        hit.point != Vector2.zero ? (Vector3)hit.point : transform.position,
                        -moveDirection,
                        isElectromagnetic: !isEnemyProjectile
                    );
                    damageable.TakeDamage(info);
                }

                // Si es un obstáculo sólido (plataforma, suelo, techo, muro, compuerta o límite del mapa) o una entidad válida:
                // Posicionar en el punto de contacto y detonar el impacto de inmediato
                Vector3 contactPoint = (hit.point != Vector2.zero) ? (Vector3)hit.point : (Vector3)(currentPos + moveDirection * hit.distance);
                transform.position = contactPoint;
                TriggerImpact();
                return;
            }

            transform.Translate(moveDirection * stepDistance, Space.World);
        }

        /// <summary>
        /// Inicializa la dirección, el daño y el emisor del pulso PEM (o proyectil de energía del Jefe).
        /// </summary>
        public void Initialize(Vector2 direction, int damage, GameObject owner, bool isEnemyShot = false, float customSpeed = -1f)
        {
            moveDirection = direction.normalized;
            damageAmount = damage;
            ownerObject = owner;
            isEnemyProjectile = isEnemyShot;
            if (customSpeed > 0f)
            {
                speed = customSpeed;
            }

            // Orientar la rotación del sprite hacia la dirección de avance
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit || other == null) return;
            ProcessCollision(other, other.ClosestPoint(transform.position));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (hasHit || collision == null) return;
            Vector2 contactPt = collision.contactCount > 0 ? collision.GetContact(0).point : (Vector2)transform.position;
            ProcessCollision(collision.collider, contactPt);
        }

        private void ProcessCollision(Collider2D other, Vector2 contactPoint)
        {
            if (hasHit || other == null) return;

            // Ignorar al emisor
            if (ownerObject != null && (other.gameObject == ownerObject || other.transform.IsChildOf(ownerObject.transform)))
            {
                return;
            }

            // Ignorar otros proyectiles
            if (other.GetComponent<EMPPulse>() != null)
            {
                return;
            }

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (other.isTrigger && damageable == null)
            {
                return;
            }

            if (damageable != null)
            {
                bool isTargetPlayer = other.CompareTag("Player") || (other.transform.root != null && other.transform.root.CompareTag("Player"));
                if (isEnemyProjectile && !isTargetPlayer)
                {
                    return;
                }

                DamageInfo info = new DamageInfo(
                    damageAmount,
                    ownerObject,
                    (Vector3)contactPoint,
                    -moveDirection,
                    isElectromagnetic: !isEnemyProjectile
                );
                damageable.TakeDamage(info);
            }

            if (contactPoint != Vector2.zero)
            {
                transform.position = contactPoint;
            }
            TriggerImpact();
        }

        private void TriggerImpact()
        {
            hasHit = true;

            if (impactVfxPrefab != null)
            {
                Instantiate(impactVfxPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
