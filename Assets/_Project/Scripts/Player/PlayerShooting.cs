using System;
using UnityEngine;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Combat;

namespace CodigoDeEmergencia.Player
{
    /// <summary>
    /// Mecánica de disparo del pulso electromagnético (PEM) desde el celular modificado (Issue #03).
    /// Daño base: 10, escalable hasta 100 mediante mejoras y crafteo.
    /// Disparo cargado: 30 de daño manteniendo pulsado ~0.55s.
    /// Disparo a prueba de bloqueos: se ejecuta inmediatamente al soltar el botón.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerShooting : MonoBehaviour
    {
        [Header("--- Parámetros del Pulso PEM ---")]
        [Tooltip("Prefab del proyectil electromagnético.")]
        [SerializeField] private GameObject empPulsePrefab;

        [Tooltip("Punto de salida del disparo en el celular del personaje.")]
        [SerializeField] private Transform firePoint;

        [Tooltip("Tiempo mínimo entre disparos (en segundos).")]
        [SerializeField] private float fireRate = 0.25f;

        [Header("--- Progresión de Daño (10 a 100) ---")]
        [Tooltip("Daño actual que causa cada pulso PEM a las pulseras.")]
        [SerializeField] private int currentDamage = 10;

        [Tooltip("Daño base inicial.")]
        [SerializeField] private int baseDamage = 10;

        [Tooltip("Daño máximo alcanzable con mejoras.")]
        [SerializeField] private int maxDamage = 100;

        [Header("--- Disparo Cargado ---")]
        [Tooltip("Tiempo manteniendo pulsado para alcanzar carga completa (en segundos).")]
        [SerializeField] private float chargeDuration = 0.55f;

        [Tooltip("Daño del pulso electromagnético cargado.")]
        [SerializeField] private int chargedDamage = 30;

        [Tooltip("Multiplicador de tamaño visual para el pulso cargado.")]
        [SerializeField] private float chargedScaleMultiplier = 2.0f;

        [Header("--- Ataque Cuerpo a Cuerpo (Katana PEM) ---")]
        [Tooltip("Daño electromagnético de cada corte de katana cuerpo a cuerpo.")]
        [SerializeField] private int meleeDamage = 25;

        [Tooltip("Radio del corte frontal de katana.")]
        [SerializeField] private float meleeRange = 1.45f;

        [Tooltip("Tiempo de enfriamiento entre cortes cuerpo a cuerpo (en segundos).")]
        [SerializeField] private float meleeCooldown = 0.32f;

        [Tooltip("Prefab opcional de chispas electromagnéticas al impactar cuerpo a cuerpo.")]
        [SerializeField] private GameObject meleeHitVfxPrefab;

        // Referencias
        private PlayerMovement playerMovement;
        private PlayerHealth playerHealth;

        // Estado
        private float nextFireTime;
        private float nextMeleeTime;
        private bool shootingEnabled = true;
        private bool isCharging;
        private float chargeTimer;
        private bool playedChargeReadySfx;

        // Eventos
        public event Action<int> OnDamageChanged;
        public event Action OnShotFired;
        public event Action OnChargedShotFired;
        public event Action OnMeleeAttackExecuted;

        // Propiedades públicas
        public int CurrentDamage => currentDamage;
        public int MaxDamage => maxDamage;
        public int MeleeDamage => meleeDamage;
        public bool IsCharging => isCharging;
        public float ChargeProgress => isCharging
            ? Mathf.Clamp01(chargeTimer / Mathf.Max(0.1f, chargeDuration))
            : 0f;
        public bool IsFullyCharged => isCharging && chargeTimer >= chargeDuration;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerHealth = GetComponent<PlayerHealth>();
            currentDamage = Mathf.Clamp(currentDamage, baseDamage, maxDamage);
        }

        private void Start()
        {
            GameEvents.TriggerWeaponDamageUpgraded(currentDamage);
        }

        private void Update()
        {
            if (!shootingEnabled) return;
            if (playerHealth != null && !playerHealth.IsAlive) return;

            // 0. Ataque cuerpo a cuerpo con Katana PEM (Tecla U, X, E o Clic Derecho)
            if (GameInput.GetMeleeDown() && Time.time >= nextMeleeTime)
            {
                ExecuteMeleeSlash();
            }

            // 1. Iniciar carga al presionar el botón de disparo (sin disparar pulso pequeño antes de saber si es carga)
            if (GameInput.GetShootDown())
            {
                isCharging = true;
                chargeTimer = 0f;
                playedChargeReadySfx = false;
            }

            // 2. Acumular carga mientras se mantenga presionado el botón
            if (GameInput.GetShootHeld() && isCharging)
            {
                chargeTimer += Time.deltaTime;
                if (chargeTimer >= chargeDuration && !playedChargeReadySfx)
                {
                    playedChargeReadySfx = true;
                    GameEvents.PlaySfx(SfxType.ChargeReady);
                }
            }
            else if (isCharging)
            {
                // 3. Al soltar el botón:
                // - Si alcanzó carga completa: dispara ÚNICAMENTE el Pulso Cargado (30 DMG, escala 2x)
                // - Si fue pulsación rápida: dispara ÚNICAMENTE el Pulso Normal (10 DMG, escala 1x)
                if (chargeTimer >= chargeDuration)
                {
                    ShootEMPPulse(chargedDamage, chargedScaleMultiplier);
                    GameEvents.PlaySfx(SfxType.ShootCharged);
                    Debug.Log($"<color=cyan><b>[DISPARO]</b> ¡Pulso PEM CARGADO liberado! (Daño: {chargedDamage}, Escala: {chargedScaleMultiplier}x)</color>");
                    OnChargedShotFired?.Invoke();
                    nextFireTime = Time.time + fireRate;
                }
                else if (Time.time >= nextFireTime)
                {
                    ShootEMPPulse(currentDamage, 1.0f);
                    GameEvents.PlaySfx(SfxType.ShootNormal);
                    Debug.Log($"<color=cyan>[DISPARO] Pulso PEM normal disparado (Daño: {currentDamage})</color>");
                    nextFireTime = Time.time + fireRate;
                }

                isCharging = false;
                chargeTimer = 0f;
                playedChargeReadySfx = false;
            }
        }

        private void ExecuteMeleeSlash()
        {
            nextMeleeTime = Time.time + meleeCooldown;
            GameEvents.PlaySfx(SfxType.MeleeSlash);
            OnMeleeAttackExecuted?.Invoke();

            float dirX = (playerMovement != null && !playerMovement.IsFacingRight) ? -1f : 1f;
            Vector2 center = firePoint != null
                ? (Vector2)firePoint.position
                : (Vector2)transform.position + new Vector2(dirX * 0.9f, 1.0f);

            Collider2D[] hits = Physics2D.OverlapCircleAll(center, meleeRange);
            int hitCount = 0;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i];
                if (col == null) continue;
                if (col.gameObject == gameObject || col.transform.IsChildOf(transform)) continue;

                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    DamageInfo info = new DamageInfo(
                        meleeDamage,
                        gameObject,
                        col.ClosestPoint(center),
                        new Vector2(dirX, 0.2f).normalized,
                        isElectromagnetic: true
                    );
                    damageable.TakeDamage(info);
                    hitCount++;

                    if (meleeHitVfxPrefab != null)
                    {
                        Instantiate(meleeHitVfxPrefab, col.ClosestPoint(center), Quaternion.identity);
                    }
                }
            }

            Debug.Log($"<color=cyan><b>[KATANA PEM]</b> Corte cuerpo a cuerpo ejecutado ({(playerMovement != null && !playerMovement.IsGrounded ? "AÉREO" : "TERRESTRE")}) — Impactos: {hitCount}</color>");
        }

        private void ShootEMPPulse(int damageToApply, float scaleMult)
        {
            if (empPulsePrefab == null)
            {
                Debug.LogWarning("[PlayerShooting] empPulsePrefab no está asignado.");
                return;
            }

            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
            GameObject pulseObj = Instantiate(empPulsePrefab, spawnPosition, Quaternion.identity);

            // Ajustar escala visual
            pulseObj.transform.localScale = Vector3.Scale(pulseObj.transform.localScale, new Vector3(scaleMult, scaleMult, 1f));

            EMPPulse pulse = pulseObj.GetComponent<EMPPulse>();
            if (pulse != null)
            {
                Vector2 direction = playerMovement.IsFacingRight ? Vector2.right : Vector2.left;
                pulse.Initialize(direction, damageToApply, gameObject);
            }

            OnShotFired?.Invoke();
        }

        /// <summary>
        /// Aumenta el daño del celular PEM (10 -> 25 -> 50 -> 100).
        /// </summary>
        public void UpgradeDamage(int newDamage)
        {
            currentDamage = Mathf.Clamp(newDamage, baseDamage, maxDamage);
            OnDamageChanged?.Invoke(currentDamage);
            GameEvents.TriggerWeaponDamageUpgraded(currentDamage);
        }

        public void SetShootingEnabled(bool enabled)
        {
            shootingEnabled = enabled;
            if (!enabled)
            {
                isCharging = false;
                chargeTimer = 0f;
            }
        }
    }
}
