using UnityEngine;
using UnityEngine.UI;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Player;

namespace CodigoDeEmergencia.UI
{
    /// <summary>
    /// Controlador principal de la interfaz en pantalla (HUD) y Sistema de Audio:
    /// - Vida del jugador en la esquina SUPERIOR DERECHA (con Sprites Sci-Fi Wenrexa).
    /// - Indicador de Disparo Cargado PEM y Disponibilidad de Dash en la esquina INFERIOR IZQUIERDA.
    /// - Barra del Jefe en la parte superior central al detectar al jugador.
    /// - Reproductor central de Música de Fondo (Acción/Suspenso estilo Metal Slug 3) y Efectos de Sonido (SFX).
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("--- Jugador: Salud (Superior Derecha) ---")]
        [Tooltip("Barra de relleno de salud del jugador (0 a 100).")]
        [SerializeField] private Image playerHealthFill;

        [Tooltip("Texto con la salud numérica del jugador.")]
        [SerializeField] private Text playerHealthText;

        [Tooltip("Texto opcional que indica el último Checkpoint activado.")]
        [SerializeField] private Text checkpointStatusText;

        [Header("--- Jugador: Carga PEM y Dash (Inferior Izquierda) ---")]
        [Tooltip("Barra de relleno que muestra la carga del disparo PEM.")]
        [SerializeField] private Image playerChargeFill;

        [Tooltip("Texto informativo del estado de carga y daño del arma.")]
        [SerializeField] private Text weaponDamageText;

        [Tooltip("Barra de relleno que indica cuando el Dash está disponible o recargándose.")]
        [SerializeField] private Image playerDashFill;

        [Tooltip("Texto del indicador de disponibilidad del Dash.")]
        [SerializeField] private Text dashStatusText;

        [Header("--- Jefe Docente (Superior Central / Derecha) ---")]
        [Tooltip("Contenedor completo de la barra del jefe (se activa/desactiva dinámicamente).")]
        [SerializeField] private GameObject bossHealthContainer;

        [Tooltip("Barra de relleno de la pulsera del jefe (0 a 500).")]
        [SerializeField] private Image bossHealthFill;

        [Tooltip("Texto con el nombre y estado del jefe.")]
        [SerializeField] private Text bossTitleText;

        [Tooltip("Texto con los puntos numéricos de la pulsera.")]
        [SerializeField] private Text bossHealthText;

        [Tooltip("Tiempo en segundos antes de ocultar la barra del jefe si el jugador se aleja.")]
        [SerializeField] private float bossBarHideDelay = 6.0f;

        [Header("--- Sistema de Audio (BGM y SFX) ---")]
        [SerializeField] private AudioClip bgmActionTrack;
        [SerializeField] private AudioClip sfxShootNormal;
        [SerializeField] private AudioClip sfxShootCharged;
        [SerializeField] private AudioClip sfxChargeReady;
        [SerializeField] private AudioClip sfxMeleeSlash;
        [SerializeField] private AudioClip sfxJump;
        [SerializeField] private AudioClip sfxDash;
        [SerializeField] private AudioClip sfxPlayerHurt;
        [SerializeField] private AudioClip sfxPlayerDead;
        [SerializeField] private AudioClip sfxEnemyHit;
        [SerializeField] private AudioClip sfxEnemyDestroyed;
        [SerializeField] private AudioClip sfxBossCast;
        [SerializeField] private AudioClip sfxCheckpoint;

        private PlayerShooting playerShooting;
        private PlayerDash playerDash;
        private AudioSource bgmSource;
        private AudioSource sfxSource;
        private float bossHideTimer;
        private bool isBossBarVisible;
        private static Sprite fallbackWhiteSprite;

        private void Awake()
        {
            EnsureFillSprite(playerHealthFill);
            EnsureFillSprite(playerChargeFill);
            EnsureFillSprite(playerDashFill);
            EnsureFillSprite(bossHealthFill);
            EnsureRuntimeTextLabels();
            SetupAudioSources();
        }

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerShooting = playerObj.GetComponent<PlayerShooting>();
                playerDash = playerObj.GetComponent<PlayerDash>();
                PlayerHealth ph = playerObj.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    UpdatePlayerHealth(ph.CurrentHealth, ph.MaxHealth);
                }
            }

            if (bossHealthContainer != null)
            {
                bossHealthContainer.SetActive(false);
            }

            if (weaponDamageText != null)
            {
                int dmg = playerShooting != null ? playerShooting.CurrentDamage : 10;
                weaponDamageText.text = $"CARGA PEM ({dmg}/30 DMG)";
            }

            if (checkpointStatusText != null)
            {
                checkpointStatusText.text = "INICIO DEL NIVEL (CP: 0/2)";
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerHealthChanged += UpdatePlayerHealth;
            GameEvents.OnPlayerDied += HandlePlayerDied;
            GameEvents.OnWeaponDamageUpgraded += UpdateWeaponDamage;
            GameEvents.OnCheckpointActivated += HandleCheckpointActivated;
            GameEvents.OnBraceletDamaged += HandleBraceletDamaged;
            GameEvents.OnBraceletBroken += HandleBraceletBroken;
            GameEvents.OnPlaySfx += HandlePlaySfx;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHealthChanged -= UpdatePlayerHealth;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
            GameEvents.OnWeaponDamageUpgraded -= UpdateWeaponDamage;
            GameEvents.OnCheckpointActivated -= HandleCheckpointActivated;
            GameEvents.OnBraceletDamaged -= HandleBraceletDamaged;
            GameEvents.OnBraceletBroken -= HandleBraceletBroken;
            GameEvents.OnPlaySfx -= HandlePlaySfx;
        }

        private void Update()
        {
            // 1. Actualizar barra de carga del disparo PEM (Inferior Izquierda)
            if (playerShooting != null && playerChargeFill != null)
            {
                EnsureFillSprite(playerChargeFill);
                float charge = playerShooting.ChargeProgress;
                playerChargeFill.fillAmount = charge;
                playerChargeFill.color = playerShooting.IsFullyCharged
                    ? new Color(0.1f, 1.0f, 0.95f, 1f)
                    : new Color(0.2f, 0.78f, 1.0f, 0.92f);

                if (weaponDamageText != null)
                {
                    if (playerShooting.IsFullyCharged)
                        weaponDamageText.text = "¡DISPARO CARGADO LISTO! (30 DMG)";
                    else if (charge > 0.05f)
                        weaponDamageText.text = $"CARGANDO PEM... {Mathf.RoundToInt(charge * 100f)}%";
                    else
                        weaponDamageText.text = $"CARGA PEM ({playerShooting.CurrentDamage}/30 DMG) | KATANA: 25";
                }
            }

            // 2. Actualizar barra de disponibilidad del Dash (Inferior Izquierda)
            if (playerDash != null && playerDashFill != null)
            {
                EnsureFillSprite(playerDashFill);
                float dashReady = playerDash.CooldownProgress;
                playerDashFill.fillAmount = dashReady;
                playerDashFill.color = playerDash.CanDash
                    ? new Color(0.2f, 1.0f, 0.55f, 1f)
                    : new Color(1.0f, 0.65f, 0.15f, 0.85f);

                if (dashStatusText != null)
                {
                    dashStatusText.text = playerDash.CanDash
                        ? "DASH [SHIFT / K]: DISPONIBLE"
                        : $"DASH: RECARGANDO ({Mathf.RoundToInt(dashReady * 100f)}%)";
                }
            }

            // 3. Temporizador para ocultar la barra del jefe si no hay interacción
            if (isBossBarVisible)
            {
                bossHideTimer -= Time.deltaTime;
                if (bossHideTimer <= 0f && bossHealthContainer != null)
                {
                    bossHealthContainer.SetActive(false);
                    isBossBarVisible = false;
                }
            }
        }

        private void SetupAudioSources()
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources.Length >= 2)
            {
                bgmSource = sources[0];
                sfxSource = sources[1];
            }
            else
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            bgmSource.loop = true;
            bgmSource.playOnAwake = true;
            bgmSource.spatialBlend = 0f;
            bgmSource.volume = 0.34f;

            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.78f;

            if (bgmActionTrack != null)
            {
                bgmSource.clip = bgmActionTrack;
                if (!bgmSource.isPlaying) bgmSource.Play();
            }
        }

        private void HandlePlaySfx(SfxType sfx)
        {
            if (sfxSource == null) return;

            AudioClip clipToPlay = null;
            float vol = 0.75f;

            switch (sfx)
            {
                case SfxType.ShootNormal:
                    clipToPlay = sfxShootNormal;
                    vol = 0.62f;
                    break;
                case SfxType.ShootCharged:
                    clipToPlay = sfxShootCharged;
                    vol = 0.90f;
                    break;
                case SfxType.ChargeReady:
                    clipToPlay = sfxChargeReady;
                    vol = 0.55f;
                    break;
                case SfxType.MeleeSlash:
                    clipToPlay = sfxMeleeSlash;
                    vol = 0.80f;
                    break;
                case SfxType.Jump:
                    clipToPlay = sfxJump;
                    vol = 0.55f;
                    break;
                case SfxType.Dash:
                    clipToPlay = sfxDash;
                    vol = 0.75f;
                    break;
                case SfxType.PlayerHurt:
                    clipToPlay = sfxPlayerHurt;
                    vol = 0.85f;
                    break;
                case SfxType.PlayerDead:
                    clipToPlay = sfxPlayerDead;
                    vol = 0.90f;
                    break;
                case SfxType.EnemyHit:
                    clipToPlay = sfxEnemyHit;
                    vol = 0.72f;
                    break;
                case SfxType.EnemyDestroyed:
                    clipToPlay = sfxEnemyDestroyed;
                    vol = 0.82f;
                    break;
                case SfxType.BossCast:
                    clipToPlay = sfxBossCast;
                    vol = 0.78f;
                    break;
                case SfxType.Checkpoint:
                    clipToPlay = sfxCheckpoint;
                    vol = 0.88f;
                    break;
            }

            if (clipToPlay != null)
            {
                sfxSource.PlayOneShot(clipToPlay, vol);
            }
        }

        private void HandleCheckpointActivated(int checkpointIndex)
        {
            if (checkpointStatusText != null)
            {
                checkpointStatusText.text = $"CHECKPOINT #{checkpointIndex}/2 ACTIVADO";
                checkpointStatusText.color = new Color(0.35f, 1.0f, 0.55f, 1f);
            }
        }

        private void UpdatePlayerHealth(int current, int max)
        {
            float fill = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

            if (playerHealthFill != null)
            {
                EnsureFillSprite(playerHealthFill);
                playerHealthFill.fillAmount = fill;

                if (fill > 0.5f)
                    playerHealthFill.color = new Color(0.15f, 0.95f, 0.50f, 1f);
                else if (fill > 0.25f)
                    playerHealthFill.color = new Color(1.0f, 0.78f, 0.15f, 1f);
                else
                    playerHealthFill.color = new Color(1.0f, 0.22f, 0.22f, 1f);
            }

            if (playerHealthText != null)
            {
                playerHealthText.text = current > 0
                    ? $"VIDA: {current} / {max}"
                    : "¡DERROTADO! Pulsa [R] para Revivir";
            }
        }

        private void HandlePlayerDied()
        {
            if (playerHealthText != null)
            {
                playerHealthText.text = "¡DERROTADO! Pulsa [R] para Revivir";
            }
        }

        private void UpdateWeaponDamage(int newDamage)
        {
            if (weaponDamageText != null)
            {
                weaponDamageText.text = $"CARGA PEM ({newDamage}/30 DMG) | KATANA: 25";
            }
        }

        private void HandleBraceletDamaged(int current, int max, Transform teacherTransform)
        {
            string title = (max >= 400)
                ? "JEFE FINAL S.I.A. — ESPECTRO DIRECTIVO"
                : "DOCENTE EN TRANCE — S.I.A.";
            ShowBossHealth(title, current, max);
        }

        private void HandleBraceletBroken(Transform teacherTransform)
        {
            if (bossTitleText != null) bossTitleText.text = "¡PULSERA DES-CORROMPIDA!";
            if (bossHealthFill != null)
            {
                EnsureFillSprite(bossHealthFill);
                bossHealthFill.fillAmount = 0f;
            }
            if (bossHealthText != null) bossHealthText.text = "LIBRE DE CONTROL";

            bossHideTimer = 2.5f;
        }

        public void ShowBossHealth(string title, int current, int max)
        {
            if (bossHealthContainer != null)
            {
                bossHealthContainer.SetActive(true);
                isBossBarVisible = true;
                bossHideTimer = bossBarHideDelay;
            }

            if (bossTitleText != null) bossTitleText.text = title;
            if (bossHealthFill != null)
            {
                EnsureFillSprite(bossHealthFill);
                bossHealthFill.fillAmount = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            }
            if (bossHealthText != null) bossHealthText.text = $"PULSERA S.I.A.: {current} / {max}";
        }

        private static void EnsureFillSprite(Image img)
        {
            if (img == null) return;

            if (img.sprite == null)
            {
                if (fallbackWhiteSprite == null)
                {
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    Color[] cols = { Color.white, Color.white, Color.white, Color.white };
                    tex.SetPixels(cols);
                    tex.Apply();
                    fallbackWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
                }
                img.sprite = fallbackWhiteSprite;
            }

            if (img.type != Image.Type.Filled)
            {
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }

        private void EnsureRuntimeTextLabels()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (playerHealthFill != null && playerHealthText == null)
            {
                Transform panel = playerHealthFill.transform.parent;
                playerHealthText = CreateRuntimeText(panel, "Player_Health_Text", defaultFont, 12, TextAnchor.MiddleCenter, new Vector2(0.04f, 0.35f), new Vector2(0.96f, 0.92f));
            }

            if (bossHealthFill != null)
            {
                Transform panel = bossHealthFill.transform.parent;
                if (bossTitleText == null)
                {
                    bossTitleText = CreateRuntimeText(panel, "Boss_Title_Text", defaultFont, 11, TextAnchor.UpperCenter, new Vector2(0.02f, 0.65f), new Vector2(0.98f, 0.98f));
                }
                if (bossHealthText == null)
                {
                    bossHealthText = CreateRuntimeText(panel, "Boss_Health_Text", defaultFont, 12, TextAnchor.MiddleCenter, new Vector2(0.02f, 0.12f), new Vector2(0.98f, 0.65f));
                }
            }
        }

        private static Text CreateRuntimeText(Transform parent, string objName, Font font, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax)
        {
            Transform existing = parent.Find(objName);
            if (existing != null)
            {
                Text existingText = existing.GetComponent<Text>();
                if (existingText != null) return existingText;
            }

            GameObject txtObj = new GameObject(objName);
            txtObj.transform.SetParent(parent, false);
            RectTransform rt = txtObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Text txt = txtObj.AddComponent<Text>();
            if (font != null) txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = alignment;
            txt.color = Color.white;
            txt.raycastTarget = false;
            return txt;
        }
    }
}
