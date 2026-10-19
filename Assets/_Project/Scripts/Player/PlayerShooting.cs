using System;
using System.Collections;
using UnityEngine;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Combat;

namespace CodigoDeEmergencia.Player
{
    /// <summary>
    /// Mecánica de disparo del pulso electromagnético (PEM) desde el celular modificado y corte de Katana (Issue #03).
    /// Bloquea por completo cualquier ataque (disparo normal, cargado o katana) mientras el jugador
    /// está subiendo/bajando escaleras (IsClimbing) o planeando con el paracaídas (IsGliding).
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

        [Header("--- Progresión de Daño (0 a 100) ---")]
        [Tooltip("Daño actual que causa cada pulso PEM a las pulseras (escala 0-100).")]
        [SerializeField] private int currentDamage = 15;

        [Tooltip("Daño base inicial (escala 0-100).")]
        [SerializeField] private int baseDamage = 15;

        [Tooltip("Daño máximo alcanzable con mejoras.")]
        [SerializeField] private int maxDamage = 100;

        [Header("--- Disparo Cargado ---")]
        [Tooltip("Tiempo manteniendo pulsado para alcanzar carga completa (en segundos).")]
        [SerializeField] private float chargeDuration = 0.55f;

        [Tooltip("Daño del pulso electromagnético cargado (escala 0-100).")]
        [SerializeField] private int chargedDamage = 35;

        [Tooltip("Multiplicador de tamaño visual para el pulso cargado.")]
        [SerializeField] private float chargedScaleMultiplier = 2.0f;

        [Header("--- Ataque Cuerpo a Cuerpo (Katana PEM) ---")]
        [Tooltip("Daño electromagnético de cada corte de katana cuerpo a cuerpo (escala 0-100).")]
        [SerializeField] private int meleeDamage = 30;

        [Tooltip("Radio del corte frontal de katana.")]
        [SerializeField] private float meleeRange = 1.55f;

        [Tooltip("Tiempo de enfriamiento entre cortes cuerpo a cuerpo (en segundos).")]
        [SerializeField] private float meleeCooldown = 0.32f;

        [Tooltip("Prefab opcional de chispas electromagnéticas al impactar cuerpo a cuerpo.")]
        [SerializeField] private GameObject meleeHitVfxPrefab;

        // Referencias
        private PlayerMovement playerMovement;
        private PlayerHealth playerHealth;
        private PlayerDash playerDash;
        private static Sprite cachedSlashArcSprite;

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

        /// <summary>
        /// Indica si las acciones ofensivas están bloqueadas por estar en escalera, planeando con paracaídas o en dash.
        /// </summary>
        public bool IsActionLockedByState =>
            (playerMovement != null && (playerMovement.IsClimbing || playerMovement.IsGliding)) ||
            (playerDash != null && playerDash.IsDashing);

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerHealth = GetComponent<PlayerHealth>();
            playerDash = GetComponent<PlayerDash>();
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
            if (Time.timeScale <= 0.01f) return;

            // REGLA ESTRICTA: Mientras el jugador está en las escaleras (IsClimbing) o planeando con el paracaídas (IsGliding),
            // NO puede realizar ningún ataque (ni Katana, ni disparo normal, ni disparo cargado).
            if (IsActionLockedByState)
            {
                if (isCharging)
                {
                    isCharging = false;
                    chargeTimer = 0f;
                    playedChargeReadySfx = false;
                }
                return;
            }

            // 0. Ataque cuerpo a cuerpo con Katana PEM (Tecla U, X, E o Clic Derecho)
            if (GameInput.GetMeleeDown() && Time.time >= nextMeleeTime)
            {
                ExecuteMeleeSlash();
            }

            // 1. Iniciar carga al presionar el botón de disparo
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
                if (chargeTimer >= chargeDuration)
                {
                    ShootEMPPulse(chargedDamage, chargedScaleMultiplier);
                    CameraFollow2D.RequestShake(0.16f, 0.16f);
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
                : (Vector2)transform.position + new Vector2(dirX * 0.95f, 1.05f);

            // Efecto visual de estela de corte de Katana
            StartCoroutine(SpawnKatanaSlashArcRoutine(center, dirX));

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

            Debug.Log($"<color=cyan><b>[KATANA PEM]</b> Corte cuerpo a cuerpo ejecutado — Impactos: {hitCount}</color>");
        }

        private IEnumerator SpawnKatanaSlashArcRoutine(Vector2 center, float dirX)
        {
            if (cachedSlashArcSprite == null)
            {
                int w = 64, h = 64;
                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float nx = (x - 20f) / 36f;
                        float ny = (y - 32f) / 28f;
                        float r = Mathf.Sqrt(nx * nx + ny * ny);
                        float crescent = 1f - Mathf.Abs(r - 0.78f) / 0.18f;
                        if (nx > 0.05f && crescent > 0f)
                        {
                            float a = Mathf.Clamp01(crescent) * Mathf.Clamp01(1f - Mathf.Abs(ny));
                            tex.SetPixel(x, y, new Color(0.75f, 0.98f, 1.0f, a));
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                tex.Apply();
                cachedSlashArcSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 40f);
            }

            GameObject arcObj = new GameObject("Katana_Slash_VFX");
            arcObj.transform.position = new Vector3(center.x + dirX * 0.25f, center.y, 0f);
            arcObj.transform.localScale = new Vector3(dirX * 1.35f, 1.35f, 1f);
            SpriteRenderer sr = arcObj.AddComponent<SpriteRenderer>();
            sr.sprite = cachedSlashArcSprite;
            sr.color = new Color(0.45f, 0.96f, 1.0f, 0.92f);
            sr.sortingOrder = 12;

            float elapsed = 0f;
            const float duration = 0.14f;
            while (elapsed < duration && arcObj != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                arcObj.transform.localScale = new Vector3(dirX * Mathf.Lerp(1.1f, 1.75f, t), Mathf.Lerp(1.35f, 0.85f, t), 1f);
                sr.color = new Color(0.55f, 0.98f, 1.0f, Mathf.Lerp(0.92f, 0f, t));
                yield return null;
            }

            if (arcObj != null) Destroy(arcObj);
        }

        private void ShootEMPPulse(int damageToApply, float scaleMult)
        {
            if (empPulsePrefab == null)
            {
                Debug.LogWarning("[PlayerShooting] empPulsePrefab no está asignado.");
                return;
            }

            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.9f;

            // Apuntado 360° hacia la posición del cursor del mouse en el mundo
            Vector3 mouseWorld = GameInput.GetMouseWorldPosition(spawnPosition + ((playerMovement != null && !playerMovement.IsFacingRight) ? Vector3.left : Vector3.right));
            Vector2 aimDirection = (Vector2)(mouseWorld - spawnPosition);
            if (aimDirection.sqrMagnitude < 0.01f)
            {
                aimDirection = (playerMovement != null && !playerMovement.IsFacingRight) ? Vector2.left : Vector2.right;
            }
            else
            {
                aimDirection.Normalize();
            }

            if (playerMovement != null && Mathf.Abs(aimDirection.x) > 0.08f)
            {
                playerMovement.FaceDirection(aimDirection.x >= 0f);
            }

            GameObject pulseObj = Instantiate(empPulsePrefab, spawnPosition, Quaternion.identity);
            pulseObj.transform.localScale = Vector3.Scale(pulseObj.transform.localScale, new Vector3(scaleMult, scaleMult, 1f));

            EMPPulse pulse = pulseObj.GetComponent<EMPPulse>();
            if (pulse != null)
            {
                pulse.Initialize(aimDirection, damageToApply, gameObject);
            }

            OnShotFired?.Invoke();
        }

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
