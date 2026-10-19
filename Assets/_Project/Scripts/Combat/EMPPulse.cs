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
    public class EMPPulse : MonoBehaviour
    {
        [Header("--- Parámetros del Proyectil ---")]
        [Tooltip("Velocidad de traslación del pulso.")]
        [SerializeField] private float speed = 16f;

        [Tooltip("Tiempo máximo de vida antes de desvanecerse (en segundos).")]
        [SerializeField] private float lifetime = 1.8f;

        [Tooltip("Prefab opcional de partículas o destello al impactar.")]
        [SerializeField] private GameObject impactVfxPrefab;

        private Vector2 moveDirection;
        private int damageAmount;
        private GameObject ownerObject;
        private bool isEnemyProjectile;
        private bool hasHit;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (hasHit) return;

            transform.Translate(moveDirection * (speed * Time.deltaTime), Space.World);
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

            // Ignorar al propio emisor que disparó el proyectil
            if (ownerObject != null && (other.gameObject == ownerObject || other.transform.IsChildOf(ownerObject.transform)))
            {
                return;
            }

            // Ignorar otros proyectiles o triggers de escaleras/checkpoints
            if (other.GetComponent<EMPPulse>() != null || other.isTrigger)
            {
                return;
            }

            // Comprobar si impacta con una entidad con IDamageable
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                // Si es un proyectil del Jefe, solo daña al Jugador (ignora otros enemigos)
                bool isTargetPlayer = other.CompareTag("Player") || (other.transform.root != null && other.transform.root.CompareTag("Player"));
                if (isEnemyProjectile && !isTargetPlayer)
                {
                    return;
                }

                DamageInfo info = new DamageInfo(
                    damageAmount,
                    ownerObject,
                    transform.position,
                    -moveDirection,
                    isElectromagnetic: !isEnemyProjectile
                );

                damageable.TakeDamage(info);
            }

            // Si choca con un obstáculo sólido o entidad válida, impacta
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
