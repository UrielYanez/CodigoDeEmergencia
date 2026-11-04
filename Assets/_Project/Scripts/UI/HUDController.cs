using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Player;

namespace CodigoDeEmergencia.UI
{
    /// <summary>
    /// Controlador integral de Interfaz (HUD), Audio, Estados del Nivel (Issue #06),
    /// Menú Principal Glassmorphism con Fondo Oscurecido (Issue #07), Cinemática Animada tipo Video (IntroCutscene),
    /// Tutorial Interactivo (Issue #08), Diálogo de Destransformación del Jefe y Transiciones Suaves (Fade + Slide).
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("--- Configuración de Escena ---")]
        [SerializeField] private bool isMainMenuScene = false;
        [SerializeField] private bool isTutorialScene = false;
        [SerializeField] private bool isIntroCutsceneScene = false;
        [SerializeField] private bool isEndingCutsceneScene = false;
        [SerializeField] private string nextSceneName = "SampleScene";

        [Header("--- Tipografía y Recursos Glassmorphism / UI ---")]
        [SerializeField] private Font customUiFont;
        [SerializeField] private Sprite glassCardSprite;
        [SerializeField] private Sprite uiButtonSprite;
        [SerializeField] private Sprite uiBarBgSprite;
        [SerializeField] private Sprite uiBarFillSprite;
        [SerializeField] private Sprite uiBarBorderSprite;

        [Header("--- Esquina Superior Derecha: Vida del Jugador + Vidas + Checkpoint ---")]
        [SerializeField] private Image playerHealthFill;
        [SerializeField] private Text playerHealthText;
        [SerializeField] private Text playerLivesText;
        [SerializeField] private Text checkpointStatusText;

        [Header("--- Parte Superior Central: Progreso del Nivel (%) ---")]
        [SerializeField] private GameObject levelProgressContainer;
        [SerializeField] private Image levelProgressFill;
        [SerializeField] private Text levelProgressText;

        [Header("--- Esquina Inferior Izquierda: Disparo Cargado y Dash ---")]
        [SerializeField] private Image playerChargeFill;
        [SerializeField] private Text weaponDamageText;
        [SerializeField] private Image playerDashFill;
        [SerializeField] private Text dashStatusText;

        [Header("--- Parte Superior Central: Barra del Jefe Final (Escala 0-100) ---")]
        [SerializeField] private GameObject bossHealthContainer;
        [SerializeField] private Image bossHealthFill;
        [SerializeField] private Text bossTitleText;
        [SerializeField] private Text bossHealthText;

        [Header("--- Recursos Visuales del Menú Principal y Cinemática ---")]
        [SerializeField] private Sprite mainMenuBackgroundSprite;
        [SerializeField] private Sprite mainMenuNeonLogoSprite;
        [SerializeField] private Sprite[] electricSparkSprites;
        [SerializeField] private Sprite[] humanIdleFrames;
        [SerializeField] private Sprite[] humanRunFrames;

        [Header("--- Recursos Visuales de Cinemática Final ---")]
        [SerializeField] private Sprite[] endingExplosionFrames;
        [SerializeField] private Sprite[] endingLightningFrames;
        [SerializeField] private Sprite[] endingRobotDeadFrames;
        [SerializeField] private Sprite[] endingWraithDyingFrames;
        [SerializeField] private Sprite endingMedalSprite;
        [SerializeField] private Sprite endingScreenBlueSprite;
        [SerializeField] private Sprite endingScreenCodeSprite;

        [Header("--- Selector de 4 Personajes Jugables ---")]
        [SerializeField] private RuntimeAnimatorController[] playableCharacterControllers;
        [SerializeField] private Sprite[] playableCharacterIdleSprites;

        [Header("--- Prefab de Enemigo para Prácticas del Tutorial ---")]
        [SerializeField] private GameObject tutorialPracticeDronePrefab;

        [Header("--- Música y Efectos de Sonido (SFX) ---")]
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
        [SerializeField] private AudioClip sfxGateClose;
        [SerializeField] private AudioClip[] bgmPlaylist;

        // Referencias en tiempo de ejecución
        private PlayerMovement playerMovement;
        private PlayerDash playerDash;
        private PlayerShooting playerShooting;
        private PlayerHealth playerHealth;
        private AudioSource bgmSource;
        private AudioSource sfxSource;
        private float bossHideTimer;
        private float levelStartX = -24f;
        private float levelGoalX = 1174f;

        private static float masterVolume = 0.85f;
        private static float musicVolume = 0.80f;
        private static float sfxVolume = 0.90f;
        private static float envVolume = 0.80f;
        private static bool jumpSfxEnabled = true;

        // UI unificada de Ajustes en 3 pestañas (compartida entre Menú Principal y Pausa)
        private GameObject unifiedSettingsOverlay;
        private RectTransform unifiedSettingsCardRect;
        private GameObject settingsTabAudio;
        private GameObject settingsTabVideo;
        private GameObject settingsTabControls;
        private Image settingsTabBtnAudioBg;
        private Image settingsTabBtnVideoBg;
        private Image settingsTabBtnControlsBg;
        private bool settingsOpenedFromMainMenu = false;

        private Slider settingsVolumeSlider;
        private Slider settingsMusicSlider;
        private Slider settingsSfxSlider;
        private Slider settingsEnvSlider;
        private Text settingsVolumeLabel;
        private Text settingsMusicLabel;
        private Text settingsSfxLabel;
        private Text settingsEnvLabel;
        private Text settingsAudioMuteLabel;
        private Text settingsJumpSfxLabel;

        private Text settingsFullscreenLabel;
        private Text settingsResolutionLabel;
        private Text settingsVsyncLabel;
        private Text settingsFpsLimitLabel;
        private Text settingsShowFpsLabel;
        private Text settingsBrightnessLabel;
        private Text settingsZoomLabel;
        private Text settingsEffectsLabel;
        private Text settingsEnemyBarsLabel;
        private Text settingsHudScaleLabel;
        private Text settingsDifficultyLabel;
        private Text settingsDifficultySummaryText;

        private readonly Text[] keybindButtonLabels = new Text[GameInput.ActionCount];
        private Text keybindStatusText;
        private bool isListeningForRebind = false;
        private InputActionId listeningActionId = InputActionId.MoveLeft;
        private float rebindArmTimer = 0f;

        private InputField nicknameInputField;
        private Text nicknameValidationText;
        private Text mainMenuControlsCol1Text;
        private Text mainMenuControlsCol2Text;

        private GameObject checkpointToastPanel;
        private Text checkpointToastText;
        private Coroutine checkpointToastCoroutine;

        private GameObject fpsCounterPanel;
        private Text fpsCounterText;
        private float fpsTimer = 0f;
        private int fpsFrames = 0;

        private Component cachedGlobalLight2D;
        private PropertyInfo cachedLightIntensityProp;
        private float baseGlobalLightIntensity = 0.72f;

        private int currentPlaylistTrackIndex = 0;
        private Font cleanBodyFont;

        // Estado del Nivel (Issue #06: 4 estados — Jugando, Pausado, Victoria, Game Over)
        private LevelState currentLevelState = LevelState.Playing;

        // Paneles dinámicos de UI con soporte de transición suave (Fade + Slide)
        private GameObject pausePanel;
        private RectTransform pauseCardRect;

        private GameObject victoryPanel;
        private RectTransform victoryCardRect;

        private GameObject gameOverPanel;
        private RectTransform gameOverCardRect;

        private GameObject loadingScreenPanel;
        private Image loadingProgressBarFill;
        private Text loadingStatusText;
        private Image loadingSparkImage;

        private GameObject tutorialOverlayPanel;
        private RectTransform tutorialCardRect;
        private Text tutorialStepTitleText;
        private Text tutorialStepBodyText;
        private Text tutorialStepPromptText;

        // Diálogo cinemático de destransformación del Jefe al finalizar nivel
        private GameObject cutsceneDialoguePanel;
        private RectTransform cutsceneDialogueCardRect;
        private Text cutsceneSpeakerText;
        private Text cutsceneMessageText;
        private Coroutine activeDialogueCoroutine;

        // Sub-paneles del Menú Principal con fondo oscurecido
        private GameObject mainMenuModalBackdrop;
        private GameObject characterSelectSubPanel;
        private RectTransform characterSelectCardRect;
        private GameObject levelSelectSubPanel;
        private RectTransform levelSelectCardRect;
        private GameObject savedGamesSubPanel;
        private RectTransform savedGamesCardRect;
        private Text[] saveSlotTitleTexts = new Text[4];
        private Text[] saveSlotMetaTexts = new Text[4];
        private Image[] saveSlotPortraitImages = new Image[4];
        private GameObject[] saveSlotDeleteButtons = new GameObject[4];
        private Button[] saveSlotLoadButtons = new Button[4];
        private GameObject deleteConfirmModal;
        private RectTransform deleteConfirmCardRect;
        private Text deleteConfirmBodyText;
        private int pendingDeleteSlotIndex = -1;
        private GameObject creditsSubPanel;
        private RectTransform creditsCardRect;

        // Sub-panel de Guardado / Victoria / Ending
        private GameObject pauseSaveSlotsPanel;
        private RectTransform pauseSaveSlotsCardRect;
        private Text[] pauseSaveSlotTitleTexts = new Text[4];
        private Text[] pauseSaveSlotMetaTexts = new Text[4];
        private Image[] pauseSaveSlotPortraits = new Image[4];
        private GameObject victoryExitConfirmPanel;
        private RectTransform victoryExitConfirmCardRect;
        private Text victoryNotificationText;
        private Text endingSaveNotificationText;
        private bool isSavingFromVictory = false;
        private bool isSavingFromEnding = false;
        private bool isTransitioningFromVictory = false;

        // Control de avance por escenas en la Cinemática
        private bool cutsceneAdvanceRequested = false;
        private bool isPlayingBossCutscene = false;
        private bool bossCutsceneSkipAll = false;

        private static readonly string[] CharacterNames = new string[]
        {
            "NINJA MUJER - KUNOICHI",
            "NINJA HOMBRE - SHINOBI",
            "KUNOICHI CIBER-AZUL",
            "SHINOBI ESMERALDA"
        };

        private static readonly string[] CharacterRoles = new string[]
        {
            "Agente ágil con Katana PEM, escalada rápida y caída suave.",
            "Operador táctico con Katana de alta frecuencia y pulso PEM.",
            "Especialista en infiltración digital y sobrecarga de pulseras.",
            "Unidad de asalto sigiloso entrenada en rescate de docentes."
        };

        // Control de Coroutines de transición por panel
        private readonly Dictionary<GameObject, Coroutine> activePanelTransitions = new Dictionary<GameObject, Coroutine>();

        // Control de estaciones del Tutorial Interactivo (Issue #08)
        private int currentTutorialStepIndex = 0;
        private bool tutorialWaitingForKey = false;
        private bool tutorialCompletedAllStations = false;

        private struct TutorialStationDef
        {
            public float triggerX;
            public string title;
            public string description;
            public string keyPrompt;
            public bool spawnPracticeDrone;
        }

        private readonly TutorialStationDef[] tutorialStations = new TutorialStationDef[]
        {
            new TutorialStationDef
            {
                triggerX = -21.5f,
                title = "1/7 — MOVIMIENTO",
                description = "Desplázate lateralmente por las plataformas para recorrer el laboratorio.",
                keyPrompt = "Pulsa A o D para moverte",
                spawnPracticeDrone = false
            },
            new TutorialStationDef
            {
                triggerX = 12.0f,
                title = "2/7 — ESCALERAS",
                description = "Alíneate al centro de la escalera para subir o bajar entre pisos.",
                keyPrompt = "Pulsa W o S para usar la escalera",
                spawnPracticeDrone = false
            },
            new TutorialStationDef
            {
                triggerX = 46.0f,
                title = "3/7 — SALTO Y CAÍDA SUAVE",
                description = "Salta con ESPACIO y mantén presionado ESPACIO en el aire para descender suavemente.",
                keyPrompt = "Pulsa ESPACIO para saltar",
                spawnPracticeDrone = false
            },
            new TutorialStationDef
            {
                triggerX = 82.0f,
                title = "4/7 — ESQUIVE DASH",
                description = "El Dash te impulsa rápidamente y te vuelve invulnerable por un instante.",
                keyPrompt = "Pulsa SHIFT o K para hacer Dash",
                spawnPracticeDrone = false
            },
            new TutorialStationDef
            {
                triggerX = 116.0f,
                title = "5/7 — CORTE DE KATANA",
                description = "Usa tu Katana a corta distancia para destruir drones y sobrecargar pulseras.",
                keyPrompt = "Pulsa CLIC DERECHO o E para atacar",
                spawnPracticeDrone = true
            },
            new TutorialStationDef
            {
                triggerX = 150.0f,
                title = "6/7 — DISPARO PEM",
                description = "Apunta con el cursor en cualquier dirección y dispara pulsos electromagnéticos.",
                keyPrompt = "Haz CLIC IZQUIERDO o pulsa J para disparar",
                spawnPracticeDrone = true
            },
            new TutorialStationDef
            {
                triggerX = 184.0f,
                title = "7/7 — DISPARO CARGADO",
                description = "Mantén presionado el botón de disparo hasta llenar la barra azul y suéltalo.",
                keyPrompt = "Mantén CLIC IZQUIERDO o J para cargar",
                spawnPracticeDrone = true
            }
        };

        private void Awake()
        {
            Time.timeScale = 1f;
            GameSettings.LoadAll();
            GameInput.LoadBindings();
            GameSettings.ApplySystemVideoSettings();

            masterVolume = PlayerPrefs.GetFloat("CDE_MasterVolume", 0.85f);
            musicVolume = PlayerPrefs.GetFloat("CDE_MusicVolume", 0.80f);
            sfxVolume = PlayerPrefs.GetFloat("CDE_SfxVolume", 0.90f);
            envVolume = PlayerPrefs.GetFloat("CDE_EnvVolume", 0.80f);
            jumpSfxEnabled = PlayerPrefs.GetInt("CDE_JumpSfxEnabled", 1) == 1;
            AudioListener.volume = masterVolume;

            cleanBodyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cleanBodyFont == null) cleanBodyFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            EnsureEventSystemExists();
            SetupAudioSources();

            if (bossHealthContainer != null)
            {
                bossHealthContainer.SetActive(false);
            }
        }

        private void Start()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            if (activeSceneName == "MainMenu") isMainMenuScene = true;
            if (activeSceneName == "TutorialScene") isTutorialScene = true;
            if (activeSceneName == "IntroCutscene") isIntroCutsceneScene = true;
            if (activeSceneName == "EndingCutscene") isEndingCutsceneScene = true;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerMovement = playerObj.GetComponent<PlayerMovement>();
                playerDash = playerObj.GetComponent<PlayerDash>();
                playerShooting = playerObj.GetComponent<PlayerShooting>();
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }

            CacheSceneBrightnessLight();
            ApplyBrightnessLive();
            EnsureStatePanelsUI();
            EnsureUnifiedSettingsUI();
            EnsureLoadingScreenUI();
            EnsureCutsceneDialogueUI();
            EnsureFpsCounterUI();

            if (isMainMenuScene)
            {
                SetGameplayHudVisible(false);
                EnsureMainMenuCanvasUI();
            }
            else if (isIntroCutsceneScene)
            {
                SetGameplayHudVisible(false);
                StartCoroutine(RunAnimatedMovieCutsceneDirector());
            }
            else if (isEndingCutsceneScene)
            {
                SetGameplayHudVisible(false);
                StartCoroutine(RunEndingCutsceneDirector());
            }
            else
            {
                SetGameplayHudVisible(true);
                InitLevelProgressBounds();
                ApplyHudScaleLive();
                EnsureCheckpointToastUI();
                UpdateLivesUI(playerHealth != null ? playerHealth.CurrentLives : GameSettings.StartingLives, playerHealth != null ? playerHealth.MaxLives : GameSettings.StartingLives);
                if (checkpointStatusText != null)
                {
                    if (playerHealth != null && playerHealth.ActiveCheckpointIndex > 0)
                    {
                        checkpointStatusText.text = $"CHECKPOINT: {playerHealth.ActiveCheckpointIndex}/8";
                        checkpointStatusText.color = new Color(0.2f, 1.0f, 0.55f, 1.0f);
                    }
                    else
                    {
                        checkpointStatusText.text = "CHECKPOINT: 0/8";
                    }
                }

                if (isTutorialScene)
                {
                    EnsureTutorialOverlayUI();
                    SyncTutorialStepToPlayerPosition();
                }

                if (playerObj == null)
                {
                    CreatePlaceholderSceneBanner(activeSceneName);
                }
            }
        }

        private void SyncTutorialStepToPlayerPosition()
        {
            if (!isTutorialScene || playerMovement == null) return;
            float px = playerMovement.transform.position.x;
            while (currentTutorialStepIndex < tutorialStations.Length && tutorialStations[currentTutorialStepIndex].triggerX < px - 1.5f)
            {
                currentTutorialStepIndex++;
            }
            if (currentTutorialStepIndex >= tutorialStations.Length)
            {
                tutorialCompletedAllStations = true;
            }
        }

        private void InitLevelProgressBounds()
        {
            levelStartX = isTutorialScene ? -21f : -24f;
            levelGoalX = isTutorialScene ? 237f : 1174f;

            GameObject triggerObj = GameObject.Find("BossArenaTrigger");
            if (triggerObj != null)
            {
                levelGoalX = triggerObj.transform.position.x;
            }
            else
            {
                GameObject gateObj = GameObject.Find("Boss_Arena_Gate");
                if (gateObj != null) levelGoalX = gateObj.transform.position.x;
            }

            if (levelProgressContainer == null)
            {
                Transform lp = transform.Find("Level_Progress_Panel");
                if (lp != null) levelProgressContainer = lp.gameObject;
            }
            UpdateLevelProgressUI();
        }

        private void UpdateLevelProgressUI()
        {
            if (levelProgressFill == null && levelProgressText == null) return;
            if (playerMovement == null) return;

            float progress = Mathf.Clamp01(Mathf.InverseLerp(levelStartX, levelGoalX, playerMovement.transform.position.x));
            int pct = Mathf.RoundToInt(progress * 100f);

            if (levelProgressFill != null)
            {
                levelProgressFill.fillAmount = progress;
                levelProgressFill.color = Color.Lerp(new Color(0.18f, 0.88f, 1.0f, 1f), new Color(0.22f, 1.0f, 0.65f, 1f), progress);
            }
            if (levelProgressText != null)
            {
                levelProgressText.text = isTutorialScene
                    ? $"PROGRESO DEL TUTORIAL: {pct}%"
                    : $"PROGRESO DEL NIVEL: {pct}%";
            }
        }

        private void SetGameplayHudVisible(bool visible)
        {
            Transform hp = transform.Find("Player_Health_Panel");
            Transform ab = transform.Find("Player_Abilities_Panel");
            if (hp != null) hp.gameObject.SetActive(visible);
            if (ab != null) ab.gameObject.SetActive(visible);
            if (levelProgressContainer != null) levelProgressContainer.SetActive(visible);
            else
            {
                Transform lp = transform.Find("Level_Progress_Panel");
                if (lp != null) lp.gameObject.SetActive(visible);
            }
            if (!visible && bossHealthContainer != null) bossHealthContainer.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerHealthChanged += UpdatePlayerHealthUI;
            GameEvents.OnPlayerLivesChanged += UpdateLivesUI;
            GameEvents.OnWeaponDamageUpgraded += UpdateWeaponDamageUI;
            GameEvents.OnBraceletDamaged += UpdateBossBraceletUI;
            GameEvents.OnBraceletBroken += HandleBraceletOverloaded;
            GameEvents.OnPlaySfx += PlaySfx;
            GameEvents.OnCheckpointActivated += HandleCheckpointReached;
            GameEvents.OnLevelStateChanged += HandleLevelStateChanged;
            GameEvents.OnCutsceneDialogueRequested += HandleCutsceneDialogueRequested;
            GameEvents.OnBossLiberationCutsceneRequested += HandleBossLiberationCutsceneRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHealthChanged -= UpdatePlayerHealthUI;
            GameEvents.OnPlayerLivesChanged -= UpdateLivesUI;
            GameEvents.OnWeaponDamageUpgraded -= UpdateWeaponDamageUI;
            GameEvents.OnBraceletDamaged -= UpdateBossBraceletUI;
            GameEvents.OnBraceletBroken -= HandleBraceletOverloaded;
            GameEvents.OnPlaySfx -= PlaySfx;
            GameEvents.OnCheckpointActivated -= HandleCheckpointReached;
            GameEvents.OnLevelStateChanged -= HandleLevelStateChanged;
            GameEvents.OnCutsceneDialogueRequested -= HandleCutsceneDialogueRequested;
            GameEvents.OnBossLiberationCutsceneRequested -= HandleBossLiberationCutsceneRequested;
        }

        private void Update()
        {
            UpdateFpsCounterLive();

            if (isListeningForRebind)
            {
                rebindArmTimer -= Time.unscaledDeltaTime;
                if (rebindArmTimer <= 0f)
                {
                    if (GameInput.CaptureAnyBindingThisFrame(out KeyCode captured, out bool cancelled))
                    {
                        if (GameInput.TryAssignBinding(listeningActionId, captured, out InputActionId conflict))
                        {
                            isListeningForRebind = false;
                            if (keybindStatusText != null)
                            {
                                keybindStatusText.text = $"✔ '{GameInput.GetActionDisplayName(listeningActionId)}' asignado a [{GameInput.FormatKeyCodeSpanish(captured)}].";
                                keybindStatusText.color = new Color(0.25f, 1.0f, 0.65f, 1f);
                            }
                            RefreshSettingsLabels();
                            RefreshDynamicControlsTexts();
                        }
                        else
                        {
                            PlaySfx(SfxType.PlayerHurt);
                            if (keybindStatusText != null)
                            {
                                keybindStatusText.text = $"⚠ [{GameInput.FormatKeyCodeSpanish(captured)}] ya está en '{GameInput.GetActionDisplayName(conflict)}'. Pulsa otra o ESC.";
                                keybindStatusText.color = new Color(1.0f, 0.42f, 0.48f, 1f);
                            }
                        }
                    }
                    else if (cancelled)
                    {
                        CancelRebindListening();
                    }
                }
                return;
            }

            if (isMainMenuScene || isIntroCutsceneScene || isPlayingBossCutscene || isEndingCutsceneScene) return;

            // 1. Control de Pausa con tecla configurada o ESC
            if (!tutorialWaitingForKey && (currentLevelState == LevelState.Playing || currentLevelState == LevelState.Paused))
            {
                if (GameInput.GetPauseDown())
                {
                    TogglePauseState();
                }
            }

            // 2. Lógica de estaciones del Tutorial Interactivo (Issue #08)
            if (isTutorialScene && currentLevelState == LevelState.Playing)
            {
                UpdateInteractiveTutorialFlow();
            }

            // 3. Actualizar barra de progreso del nivel (%) y barras de Disparo Cargado y Dash en tiempo real
            UpdateLevelProgressUI();
            bool lockedByClimbOrGlide = playerMovement != null && (playerMovement.IsClimbing || playerMovement.IsGliding);

            if (playerShooting != null && playerChargeFill != null)
            {
                float charge = playerShooting.ChargeProgress;
                playerChargeFill.fillAmount = charge;
                playerChargeFill.color = playerShooting.IsFullyCharged
                    ? new Color(1.0f, 0.9f, 0.2f, 1.0f)
                    : new Color(0.1f, 0.85f, 1.0f, 1.0f);

                if (weaponDamageText != null)
                {
                    if (lockedByClimbOrGlide)
                    {
                        weaponDamageText.text = "ATAQUE BLOQUEADO";
                    }
                    else
                    {
                        weaponDamageText.text = $"PEM: {playerShooting.CurrentDamage} DMG | KATANA: {playerShooting.MeleeDamage}";
                    }
                }
            }

            if (playerDash != null && playerDashFill != null)
            {
                float dashReady = playerDash.CooldownProgress;
                playerDashFill.fillAmount = lockedByClimbOrGlide ? 0f : dashReady;
                playerDashFill.color = (!lockedByClimbOrGlide && dashReady >= 0.99f)
                    ? new Color(0.2f, 1.0f, 0.55f, 1.0f)
                    : new Color(1.0f, 0.55f, 0.15f, 0.85f);

                if (dashStatusText != null)
                {
                    if (lockedByClimbOrGlide)
                    {
                        dashStatusText.text = "DASH NO DISPONIBLE";
                    }
                    else
                    {
                        dashStatusText.text = dashReady >= 0.99f
                            ? "DASH: DISPONIBLE"
                            : "DASH: RECARGANDO";
                    }
                }
            }

            if (bossHealthContainer != null && bossHealthContainer.activeSelf)
            {
                bossHideTimer -= Time.deltaTime;
                if (bossHideTimer <= 0f)
                {
                    bossHealthContainer.SetActive(false);
                }
            }
        }

        // =================================================================================================
        // SISTEMA DE TRANSICIONES SUAVES (FADE + SLIDE / SCALE) PARA TODOS LOS MENSAJES Y PANELES
        // =================================================================================================

        private void AnimatePanelShow(GameObject panelRoot, RectTransform cardRect, Vector2 startOffsetPx, float duration = 0.36f)
        {
            if (panelRoot == null) return;
            if (activePanelTransitions.TryGetValue(panelRoot, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }
            Coroutine c = StartCoroutine(PanelShowRoutine(panelRoot, cardRect, startOffsetPx, duration));
            activePanelTransitions[panelRoot] = c;
        }

        private void AnimatePanelHide(GameObject panelRoot, RectTransform cardRect, Vector2 exitOffsetPx, float duration = 0.24f)
        {
            if (panelRoot == null || !panelRoot.activeSelf) return;
            if (activePanelTransitions.TryGetValue(panelRoot, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }
            Coroutine c = StartCoroutine(PanelHideRoutine(panelRoot, cardRect, exitOffsetPx, duration));
            activePanelTransitions[panelRoot] = c;
        }

        private IEnumerator PanelShowRoutine(GameObject panelRoot, RectTransform cardRect, Vector2 startOffsetPx, float duration)
        {
            panelRoot.SetActive(true);
            CanvasGroup rootCg = GetOrAddCanvasGroup(panelRoot);
            rootCg.alpha = 0f;

            Vector2 targetAnchoredPos = cardRect != null ? Vector2.zero : Vector2.zero;
            if (cardRect != null)
            {
                cardRect.anchoredPosition = startOffsetPx;
                cardRect.localScale = new Vector3(0.92f, 0.92f, 1f);
            }

            float elapsed = 0f;
            while (elapsed < duration && panelRoot != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, duration));
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);

                rootCg.alpha = easeOut;
                if (cardRect != null)
                {
                    cardRect.anchoredPosition = Vector2.Lerp(startOffsetPx, targetAnchoredPos, easeOut);
                    float sc = Mathf.Lerp(0.92f, 1.0f, easeOut);
                    cardRect.localScale = new Vector3(sc, sc, 1f);
                }
                yield return null;
            }

            if (panelRoot != null)
            {
                rootCg.alpha = 1f;
                if (cardRect != null)
                {
                    cardRect.anchoredPosition = targetAnchoredPos;
                    cardRect.localScale = Vector3.one;
                }
            }
        }

        private IEnumerator PanelHideRoutine(GameObject panelRoot, RectTransform cardRect, Vector2 exitOffsetPx, float duration)
        {
            CanvasGroup rootCg = GetOrAddCanvasGroup(panelRoot);
            float startAlpha = rootCg.alpha;
            Vector2 startPos = cardRect != null ? cardRect.anchoredPosition : Vector2.zero;

            float elapsed = 0f;
            while (elapsed < duration && panelRoot != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, duration));
                float easeIn = t * t;

                rootCg.alpha = Mathf.Lerp(startAlpha, 0f, easeIn);
                if (cardRect != null)
                {
                    cardRect.anchoredPosition = Vector2.Lerp(startPos, exitOffsetPx, easeIn);
                    float sc = Mathf.Lerp(1.0f, 0.94f, easeIn);
                    cardRect.localScale = new Vector3(sc, sc, 1f);
                }
                yield return null;
            }

            if (panelRoot != null)
            {
                rootCg.alpha = 0f;
                if (cardRect != null)
                {
                    cardRect.anchoredPosition = Vector2.zero;
                    cardRect.localScale = Vector3.one;
                }
                panelRoot.SetActive(false);
            }
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
        {
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = obj.AddComponent<CanvasGroup>();
            return cg;
        }

        // =================================================================================================
        // ISSUE #06: 4 ESTADOS DEL NIVEL (JUGANDO, PAUSADO, VICTORIA, GAME OVER) CON TRANSICIONES SUAVES
        // =================================================================================================

        public void TogglePauseState()
        {
            if (currentLevelState == LevelState.Playing)
            {
                GameEvents.TriggerLevelStateChanged(LevelState.Paused);
            }
            else if (currentLevelState == LevelState.Paused)
            {
                GameEvents.TriggerLevelStateChanged(LevelState.Playing);
            }
        }

        private void HandleLevelStateChanged(LevelState newState)
        {
            currentLevelState = newState;

            if (newState != LevelState.Paused && pausePanel != null && pausePanel.activeSelf)
            {
                CancelRebindListening();
                if (unifiedSettingsOverlay != null) unifiedSettingsOverlay.SetActive(false);
                AnimatePanelHide(pausePanel, pauseCardRect, new Vector2(0f, 55f), 0.22f);
            }
            if (newState != LevelState.Victory && victoryPanel != null && victoryPanel.activeSelf)
            {
                if (victoryExitConfirmPanel != null) victoryExitConfirmPanel.SetActive(false);
                AnimatePanelHide(victoryPanel, victoryCardRect, new Vector2(0f, -55f), 0.22f);
            }
            if (newState != LevelState.GameOver && gameOverPanel != null && gameOverPanel.activeSelf)
            {
                AnimatePanelHide(gameOverPanel, gameOverCardRect, new Vector2(0f, -55f), 0.22f);
            }

            switch (newState)
            {
                case LevelState.Playing:
                    Time.timeScale = 1f;
                    break;

                case LevelState.Paused:
                    Time.timeScale = 0f;
                    CancelRebindListening();
                    if (unifiedSettingsOverlay != null) unifiedSettingsOverlay.SetActive(false);
                    if (pauseSaveSlotsPanel != null) pauseSaveSlotsPanel.SetActive(false);
                    RefreshAudioLabels();
                    RefreshSettingsLabels();
                    if (pausePanel != null) AnimatePanelShow(pausePanel, pauseCardRect, new Vector2(0f, 65f), 0.34f);
                    break;

                case LevelState.Victory:
                    if (cutsceneDialoguePanel != null && cutsceneDialoguePanel.activeSelf)
                    {
                        AnimatePanelHide(cutsceneDialoguePanel, cutsceneDialogueCardRect, new Vector2(0f, -60f), 0.20f);
                    }
                    Time.timeScale = 0f;
                    isTransitioningFromVictory = false;
                    if (victoryNotificationText != null) victoryNotificationText.text = "";
                    if (victoryExitConfirmPanel != null) victoryExitConfirmPanel.SetActive(false);
                    if (victoryPanel != null)
                    {
                        victoryPanel.transform.SetAsLastSibling();
                        AnimatePanelShow(victoryPanel, victoryCardRect, new Vector2(0f, -70f), 0.42f);
                    }
                    break;

                case LevelState.GameOver:
                    Time.timeScale = 0f;
                    if (gameOverPanel != null) AnimatePanelShow(gameOverPanel, gameOverCardRect, new Vector2(0f, -70f), 0.42f);
                    break;
            }
        }

        // =================================================================================================
        // ISSUE #08: FLUJO DEL TUTORIAL INTERACTIVO (TRANSICIÓN SUAVE AL MOSTRAR Y OCULTAR CADA ESTACIÓN)
        // =================================================================================================

        private void UpdateInteractiveTutorialFlow()
        {
            if (tutorialCompletedAllStations || playerMovement == null) return;

            if (tutorialWaitingForKey)
            {
                if (CheckCurrentTutorialStationKeyRequirement(currentTutorialStepIndex))
                {
                    TutorialStationDef station = tutorialStations[currentTutorialStepIndex];
                    if (station.spawnPracticeDrone && tutorialPracticeDronePrefab != null)
                    {
                        Vector3 dronePos = playerMovement.transform.position + new Vector3(4.5f, 0.8f, 0f);
                        Instantiate(tutorialPracticeDronePrefab, dronePos, Quaternion.identity);
                    }

                    tutorialWaitingForKey = false;
                    AnimatePanelHide(tutorialOverlayPanel, tutorialCardRect, new Vector2(0f, 55f), 0.26f);
                    Time.timeScale = 1f;

                    currentTutorialStepIndex++;
                    if (currentTutorialStepIndex >= tutorialStations.Length)
                    {
                        tutorialCompletedAllStations = true;
                    }
                }
                return;
            }

            if (currentTutorialStepIndex < tutorialStations.Length)
            {
                float playerX = playerMovement.transform.position.x;
                TutorialStationDef nextStation = tutorialStations[currentTutorialStepIndex];
                if (playerX >= nextStation.triggerX)
                {
                    ShowTutorialStationPrompt(nextStation);
                }
            }
        }

        private void ShowTutorialStationPrompt(TutorialStationDef station)
        {
            if (tutorialOverlayPanel == null) EnsureTutorialOverlayUI();

            tutorialWaitingForKey = true;
            Time.timeScale = 0f;

            if (tutorialStepTitleText != null) tutorialStepTitleText.text = station.title;
            if (tutorialStepBodyText != null) tutorialStepBodyText.text = station.description;
            if (tutorialStepPromptText != null) tutorialStepPromptText.text = "▶ " + GetTutorialStationPrompt(currentTutorialStepIndex) + " ◀";

            AnimatePanelShow(tutorialOverlayPanel, tutorialCardRect, new Vector2(0f, 65f), 0.38f);
        }

        private string GetTutorialStationPrompt(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0: return $"Pulsa {GameInput.GetBindingDisplayName(InputActionId.MoveLeft)} o {GameInput.GetBindingDisplayName(InputActionId.MoveRight)} para moverte";
                case 1: return $"Pulsa {GameInput.GetBindingDisplayName(InputActionId.ClimbUp)} o {GameInput.GetBindingDisplayName(InputActionId.ClimbDown)} para usar la escalera";
                case 2: return $"Pulsa {GameInput.GetBindingDisplayName(InputActionId.Jump)} para saltar";
                case 3: return $"Pulsa {GameInput.GetBindingDisplayName(InputActionId.Dash)} para hacer Dash";
                case 4: return $"Pulsa {GameInput.GetBindingDisplayName(InputActionId.Melee)} para atacar";
                case 5: return $"Pulsa {GameInput.GetBindingDisplayName(InputActionId.Shoot)} para disparar";
                case 6: return $"Mantén {GameInput.GetBindingDisplayName(InputActionId.Shoot)} para cargar";
                default: return tutorialStations[Mathf.Clamp(stepIndex, 0, tutorialStations.Length - 1)].keyPrompt;
            }
        }

        private bool CheckCurrentTutorialStationKeyRequirement(int stepIndex)
        {
            switch (stepIndex)
            {
                case 0:
                    return Mathf.Abs(GameInput.GetHorizontal()) > 0.1f;
                case 1:
                    return Mathf.Abs(GameInput.GetVertical()) > 0.1f;
                case 2:
                    return GameInput.GetJumpDown() || GameInput.GetJumpHeld();
                case 3:
                    return GameInput.GetDashDown();
                case 4:
                    return GameInput.GetMeleeDown();
                case 5:
                    return GameInput.GetShootDown() || GameInput.GetShootHeld();
                case 6:
                    return GameInput.GetShootDown() || GameInput.GetShootHeld();
                default:
                    return GameInput.GetConfirmDialogueDown() || GameInput.GetJumpDown() || GameInput.GetShootDown();
            }
        }

        // =================================================================================================
        // PANTALLA DE CARGA PERSONALIZADA DE 3 SEGUNDOS ENTRE ESCENAS (ISSUE #07)
        // =================================================================================================

        public void LoadSceneWithAnimatedTransition(string targetSceneName)
        {
            Time.timeScale = 1f;
            StartCoroutine(AnimatedLoadingTransitionRoutine(targetSceneName));
        }

        private IEnumerator AnimatedLoadingTransitionRoutine(string targetSceneName)
        {
            if (loadingScreenPanel == null) EnsureLoadingScreenUI();

            CancelRebindListening();
            if (pausePanel != null) pausePanel.SetActive(false);
            if (unifiedSettingsOverlay != null) unifiedSettingsOverlay.SetActive(false);
            if (pauseSaveSlotsPanel != null) pauseSaveSlotsPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (victoryExitConfirmPanel != null) victoryExitConfirmPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (tutorialOverlayPanel != null) tutorialOverlayPanel.SetActive(false);
            if (checkpointToastPanel != null) checkpointToastPanel.SetActive(false);
            if (characterSelectSubPanel != null) characterSelectSubPanel.SetActive(false);
            if (levelSelectSubPanel != null) levelSelectSubPanel.SetActive(false);
            if (savedGamesSubPanel != null) savedGamesSubPanel.SetActive(false);
            if (deleteConfirmModal != null) deleteConfirmModal.SetActive(false);
            if (creditsSubPanel != null) creditsSubPanel.SetActive(false);
            if (mainMenuModalBackdrop != null) mainMenuModalBackdrop.SetActive(false);

            loadingScreenPanel.transform.SetAsLastSibling();
            loadingScreenPanel.SetActive(true);
            CanvasGroup lcg = GetOrAddCanvasGroup(loadingScreenPanel);
            lcg.alpha = 1f;

            PlaySfx(SfxType.ChargeReady);

            const float transitionDuration = 3.0f;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / transitionDuration);

                if (loadingProgressBarFill != null)
                {
                    loadingProgressBarFill.fillAmount = progress;
                }

                if (loadingStatusText != null)
                {
                    int pct = Mathf.RoundToInt(progress * 100f);
                    string phase = progress < 0.35f
                        ? "SINCRONIZANDO PULSO PEM..."
                        : (progress < 0.75f ? "CARGANDO LABORATORIO..." : "INICIANDO NIVEL...");
                    loadingStatusText.text = $"{phase}  {pct}%";
                }

                if (loadingSparkImage != null && electricSparkSprites != null && electricSparkSprites.Length > 0)
                {
                    int frameIdx = Mathf.FloorToInt(elapsed * 16f) % electricSparkSprites.Length;
                    if (electricSparkSprites[frameIdx] != null)
                    {
                        loadingSparkImage.sprite = electricSparkSprites[frameIdx];
                    }
                }

                yield return null;
            }

            SceneManager.LoadScene(targetSceneName);
        }

        // =================================================================================================
        // CONSTRUCCIÓN DE INTERFACES GLASSMORPHISM CON FONDO OSCURECIDO Y TRANSICIONES SUAVES
        // =================================================================================================

        private void EnsureMainMenuCanvasUI()
        {
            GameObject menuRoot = new GameObject("MainMenu_FullScreen_UI");
            menuRoot.transform.SetParent(transform, false);
            StretchFullScreen(menuRoot);

            // 1. Fondo de Suspenso + Velo Oscurecedor de Alto Contraste para que el Glassmorphism luzca nítido
            GameObject bgObj = new GameObject("Suspense_Background");
            bgObj.transform.SetParent(menuRoot.transform, false);
            StretchFullScreen(bgObj);
            Image imgBg = bgObj.AddComponent<Image>();
            if (mainMenuBackgroundSprite != null)
            {
                imgBg.sprite = mainMenuBackgroundSprite;
                imgBg.color = new Color(0.75f, 0.82f, 0.92f, 1f);
            }
            else
            {
                imgBg.color = new Color(0.03f, 0.05f, 0.10f, 1f);
            }

            // Capa oscurecedora sobre el fondo para resaltar todas las tarjetas Glassmorphism
            GameObject darkOverlay = new GameObject("MainMenu_Dark_Contrast_Backdrop");
            darkOverlay.transform.SetParent(menuRoot.transform, false);
            StretchFullScreen(darkOverlay);
            Image imgDark = darkOverlay.AddComponent<Image>();
            imgDark.color = new Color(0.01f, 0.02f, 0.06f, 0.72f);
            imgDark.raycastTarget = false;

            // 2. Barra Superior Reacomodada en la Parte Superior
            GameObject topBar = CreateGlassCard(menuRoot.transform, "Top_Telemetry_GlassBar",
                new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.975f),
                new Color(0.02f, 0.06f, 0.14f, 0.90f), new Color(0.25f, 0.88f, 1.0f, 0.85f));
            CreateDynamicText(topBar.transform, "Top_Left_Status", "⚡ CÓDIGO DE EMERGENCIA — UTNG", 15,
                new Vector2(0.02f, 0.0f), new Vector2(0.54f, 1.0f), new Color(0.55f, 0.95f, 1.0f, 1f), TextAnchor.MiddleLeft);
            CreateDynamicText(topBar.transform, "Top_Right_Alert", "ALERTA S.I.A. ACTIVA", 15,
                new Vector2(0.54f, 0.0f), new Vector2(0.98f, 1.0f), new Color(1.0f, 0.48f, 0.58f, 1f), TextAnchor.MiddleRight);

            // 3. Logo Neón del Juego Reacomodado en la Parte Superior Central
            GameObject logoContainer = new GameObject("Neon_Title_Container");
            logoContainer.transform.SetParent(menuRoot.transform, false);
            RectTransform rtLogo = logoContainer.AddComponent<RectTransform>();
            rtLogo.anchorMin = new Vector2(0.25f, 0.62f);
            rtLogo.anchorMax = new Vector2(0.75f, 0.905f);
            rtLogo.offsetMin = Vector2.zero;
            rtLogo.offsetMax = Vector2.zero;

            if (mainMenuNeonLogoSprite != null)
            {
                GameObject logoImgObj = new GameObject("Neon_Logo_Image");
                logoImgObj.transform.SetParent(logoContainer.transform, false);
                StretchFullScreen(logoImgObj);
                Image imgLogo = logoImgObj.AddComponent<Image>();
                imgLogo.sprite = mainMenuNeonLogoSprite;
                imgLogo.preserveAspect = true;
                imgLogo.color = Color.white;
                imgLogo.raycastTarget = false;
            }
            else
            {
                CreateDynamicText(logoContainer.transform, "Fallback_Title", "CÓDIGO DE EMERGENCIA", 44,
                    new Vector2(0f, 0.2f), new Vector2(1f, 0.9f), new Color(0.15f, 0.95f, 1.0f, 1f));
            }

            // 4. Tarjeta Izquierda: MENÚ PRINCIPAL ALINEADO A LA IZQUIERDA
            GameObject leftMenuGlass = CreateGlassCard(menuRoot.transform, "Left_MainMenu_Options_GlassCard",
                new Vector2(0.04f, 0.06f), new Vector2(0.46f, 0.60f),
                new Color(0.02f, 0.06f, 0.14f, 0.93f), new Color(0.28f, 0.95f, 1.0f, 0.92f));

            CreateDynamicText(leftMenuGlass.transform, "Left_Menu_Header", "MENÚ PRINCIPAL", 20,
                new Vector2(0.06f, 0.87f), new Vector2(0.94f, 0.97f), new Color(0.45f, 0.96f, 1.0f, 1f), TextAnchor.MiddleLeft, useHeaderFont: true);

            // Botón 1: NUEVO JUEGO
            CreateGlassButton(leftMenuGlass.transform, "Btn_New_Game", "▶  NUEVO JUEGO",
                new Vector2(0.06f, 0.71f), new Vector2(0.94f, 0.84f),
                new Color(0.06f, 0.48f, 0.68f, 0.94f), new Color(0.25f, 0.98f, 1.0f, 1f),
                () =>
                {
                    PlayerPrefs.SetInt("CDE_IsTutorialOnlyMode", 0);
                    PlayerPrefs.SetInt("CDE_PendingLoadSlot", -1);
                    PlayerPrefs.Save();
                    OpenMainMenuSubPanel(characterSelectSubPanel, characterSelectCardRect);
                });

            // Botón 2: CONTINUAR PARTIDA
            CreateGlassButton(leftMenuGlass.transform, "Btn_Continue_Saved", "💾  CONTINUAR PARTIDA",
                new Vector2(0.06f, 0.55f), new Vector2(0.94f, 0.68f),
                new Color(0.06f, 0.44f, 0.36f, 0.94f), new Color(0.25f, 1.0f, 0.72f, 1f),
                () =>
                {
                    RefreshSavedGamesSubPanel();
                    OpenMainMenuSubPanel(savedGamesSubPanel, savedGamesCardRect);
                });

            // Botón 3: SELECCIONAR NIVEL (Tutorial + 4 Niveles)
            CreateGlassButton(leftMenuGlass.transform, "Btn_Select_Level", "🗺  SELECCIONAR NIVEL",
                new Vector2(0.06f, 0.39f), new Vector2(0.94f, 0.52f),
                new Color(0.10f, 0.28f, 0.50f, 0.92f), new Color(0.45f, 0.85f, 1.0f, 0.96f),
                () => OpenMainMenuSubPanel(levelSelectSubPanel, levelSelectCardRect));

            // Botón 4: AJUSTES
            CreateGlassButton(leftMenuGlass.transform, "Btn_Settings", "⚙  AJUSTES",
                new Vector2(0.06f, 0.23f), new Vector2(0.94f, 0.36f),
                new Color(0.12f, 0.22f, 0.38f, 0.92f), new Color(0.55f, 0.85f, 1.0f, 0.92f),
                () => OpenUnifiedSettingsModal(fromMainMenu: true));

            // Botón 5: CRÉDITOS + SALIR
            CreateGlassButton(leftMenuGlass.transform, "Btn_Credits", "★  CRÉDITOS",
                new Vector2(0.06f, 0.07f), new Vector2(0.62f, 0.20f),
                new Color(0.22f, 0.14f, 0.40f, 0.92f), new Color(0.82f, 0.60f, 1.0f, 0.96f),
                () => OpenMainMenuSubPanel(creditsSubPanel, creditsCardRect));

            CreateGlassButton(leftMenuGlass.transform, "Btn_Quit", "✖  SALIR",
                new Vector2(0.65f, 0.07f), new Vector2(0.94f, 0.20f),
                new Color(0.42f, 0.08f, 0.16f, 0.92f), new Color(1.0f, 0.38f, 0.48f, 0.96f),
                () =>
                {
                    Application.Quit();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                });

            // 5. Tarjeta Derecha Adaptada al Espacio Restante: Controles directos y limpios
            GameObject rightGlass = CreateGlassCard(menuRoot.transform, "Right_Controls_Adapted_GlassCard",
                new Vector2(0.49f, 0.06f), new Vector2(0.96f, 0.60f),
                new Color(0.02f, 0.06f, 0.14f, 0.93f), new Color(0.22f, 0.88f, 1.0f, 0.85f));

            CreateDynamicText(rightGlass.transform, "Right_Card_Header", "CONTROLES", 20,
                new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.96f), new Color(0.35f, 0.96f, 1.0f, 1f), TextAnchor.MiddleLeft, useHeaderFont: true);

            CreateDynamicText(rightGlass.transform, "Right_Col1_Title", "MOVIMIENTO", 16,
                new Vector2(0.05f, 0.73f), new Vector2(0.48f, 0.83f), new Color(0.45f, 1.0f, 0.82f, 1f), TextAnchor.MiddleLeft);

            mainMenuControlsCol1Text = CreateDynamicText(rightGlass.transform, "Right_Col1_Body", "",
                15, new Vector2(0.05f, 0.10f), new Vector2(0.48f, 0.72f), new Color(0.92f, 0.97f, 1.0f, 1f), TextAnchor.UpperLeft);

            CreateDynamicText(rightGlass.transform, "Right_Col2_Title", "COMBATE Y SISTEMA", 16,
                new Vector2(0.52f, 0.73f), new Vector2(0.95f, 0.83f), new Color(1.0f, 0.85f, 0.35f, 1f), TextAnchor.MiddleLeft);

            mainMenuControlsCol2Text = CreateDynamicText(rightGlass.transform, "Right_Col2_Body", "",
                15, new Vector2(0.52f, 0.10f), new Vector2(0.95f, 0.72f), new Color(0.92f, 0.97f, 1.0f, 1f), TextAnchor.UpperLeft);

            RefreshDynamicControlsTexts();

            // Animación suave de entrada de las tarjetas del Menú Principal
            AnimatePanelShow(leftMenuGlass, leftMenuGlass.GetComponent<RectTransform>(), new Vector2(-45f, 0f), 0.42f);
            AnimatePanelShow(rightGlass, rightGlass.GetComponent<RectTransform>(), new Vector2(45f, 0f), 0.42f);

            // 6. Velo Oscurecedor Modal para Sub-paneles del Menú Principal
            mainMenuModalBackdrop = new GameObject("MainMenu_Modal_Dark_Backdrop");
            mainMenuModalBackdrop.transform.SetParent(menuRoot.transform, false);
            StretchFullScreen(mainMenuModalBackdrop);
            Image modalDarkImg = mainMenuModalBackdrop.AddComponent<Image>();
            modalDarkImg.color = new Color(0.01f, 0.02f, 0.05f, 0.90f);
            mainMenuModalBackdrop.SetActive(false);

            // 7. SUB-PANEL 1: SELECTOR DE PERSONAJE
            BuildCharacterSelectModal(menuRoot.transform);

            // 8. SUB-PANEL 2: SELECTOR DE NIVELES (TUTORIAL Y 4 NIVELES)
            BuildLevelSelectModal(menuRoot.transform);

            // 9. SUB-PANEL 3: PARTIDAS GUARDADAS
            BuildSavedGamesModal(menuRoot.transform);

            // 10. SUB-PANEL 4: CRÉDITOS (Ajustes usa unifiedSettingsOverlay compartido)
            BuildCreditsModal(menuRoot.transform);
        }

        private void BuildCharacterSelectModal(Transform menuRoot)
        {
            characterSelectSubPanel = CreateGlassCard(menuRoot, "CharacterSelect_GlassSubPanel",
                new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.91f),
                new Color(0.02f, 0.05f, 0.13f, 0.96f), new Color(0.25f, 0.95f, 1.0f, 0.96f));
            characterSelectCardRect = characterSelectSubPanel.GetComponent<RectTransform>();

            CreateDynamicText(characterSelectSubPanel.transform, "CS_Title", "SELECCIONAR PERSONAJE", 23,
                new Vector2(0.04f, 0.88f), new Vector2(0.96f, 0.97f), new Color(0.25f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            CreateDynamicText(characterSelectSubPanel.transform, "CS_Subtitle",
                "Escribe tu nickname obligatorio y elige a tu agente para comenzar.",
                15, new Vector2(0.04f, 0.815f), new Vector2(0.96f, 0.88f), new Color(0.85f, 0.95f, 1.0f, 0.96f));

            // Campo obligatorio de Nickname
            GameObject nickRow = CreateGlassCard(characterSelectSubPanel.transform, "CS_Nickname_Row",
                new Vector2(0.12f, 0.70f), new Vector2(0.88f, 0.805f),
                new Color(0.03f, 0.09f, 0.20f, 0.95f), new Color(0.30f, 0.95f, 1.0f, 0.85f));
            CreateDynamicText(nickRow.transform, "Nick_Label", "👤  TU NICKNAME (OBLIGATORIO):", 15,
                new Vector2(0.025f, 0.10f), new Vector2(0.41f, 0.90f), new Color(0.45f, 1.0f, 0.85f, 1f), TextAnchor.MiddleLeft);

            string storedNick = PlayerPrefs.GetString("CDE_PlayerNickname", "");
            nicknameInputField = CreateGlassInputField(nickRow.transform, "CS_Nickname_Input",
                "Escribe tu apodo (2 a 16 caracteres)...", storedNick, 16,
                new Vector2(0.42f, 0.14f), new Vector2(0.975f, 0.86f));

            nicknameValidationText = CreateDynamicText(characterSelectSubPanel.transform, "CS_Nick_Warn", "", 14,
                new Vector2(0.10f, 0.65f), new Vector2(0.90f, 0.698f), new Color(1.0f, 0.42f, 0.48f, 1f));

            Color[] cardAccents = new Color[]
            {
                new Color(0.25f, 0.95f, 1.0f, 0.95f),
                new Color(0.30f, 1.0f, 0.68f, 0.95f),
                new Color(0.20f, 0.88f, 1.0f, 0.95f),
                new Color(0.35f, 1.0f, 0.65f, 0.95f)
            };

            for (int i = 0; i < 2; i++)
            {
                int charIdx = i;
                float xMin = 0.12f + i * 0.40f;
                float xMax = xMin + 0.36f;

                GameObject charCard = CreateGlassCard(characterSelectSubPanel.transform, $"Char_Option_Card_{i}",
                    new Vector2(xMin, 0.15f), new Vector2(xMax, 0.64f),
                    new Color(0.03f, 0.08f, 0.18f, 0.94f), cardAccents[i]);

                CreateDynamicText(charCard.transform, "Char_Title", CharacterNames[i], 16,
                    new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.96f), cardAccents[i], TextAnchor.MiddleCenter, useHeaderFont: true);

                // Retrato del personaje
                GameObject portraitObj = new GameObject("Char_Portrait");
                portraitObj.transform.SetParent(charCard.transform, false);
                RectTransform rtPortrait = portraitObj.AddComponent<RectTransform>();
                rtPortrait.anchorMin = new Vector2(0.20f, 0.42f);
                rtPortrait.anchorMax = new Vector2(0.80f, 0.80f);
                rtPortrait.offsetMin = Vector2.zero;
                rtPortrait.offsetMax = Vector2.zero;
                Image imgPortrait = portraitObj.AddComponent<Image>();
                imgPortrait.preserveAspect = true;
                if (playableCharacterIdleSprites != null && charIdx < playableCharacterIdleSprites.Length && playableCharacterIdleSprites[charIdx] != null)
                {
                    imgPortrait.sprite = playableCharacterIdleSprites[charIdx];
                }
                imgPortrait.color = Color.white;

                CreateDynamicText(charCard.transform, "Char_Desc", CharacterRoles[i], 14,
                    new Vector2(0.06f, 0.21f), new Vector2(0.94f, 0.40f), new Color(0.92f, 0.97f, 1.0f, 0.98f));

                CreateGlassButton(charCard.transform, $"Btn_Select_Char_{i}", "✔ ELEGIR",
                    new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.18f),
                    new Color(0.06f, 0.44f, 0.58f, 0.95f), cardAccents[i],
                    () => StartNewGameWithCharacter(charIdx));
            }

            CreateGlassButton(characterSelectSubPanel.transform, "Btn_CS_Back", "↩  VOLVER",
                new Vector2(0.36f, 0.03f), new Vector2(0.64f, 0.12f),
                new Color(0.38f, 0.10f, 0.18f, 0.94f), new Color(1.0f, 0.45f, 0.55f, 0.95f),
                () => CloseMainMenuSubPanel(characterSelectSubPanel, characterSelectCardRect));

            characterSelectSubPanel.SetActive(false);
        }

        private void StartNewGameWithCharacter(int characterIndex)
        {
            string rawNick = nicknameInputField != null ? nicknameInputField.text : "";
            string cleanNick = GameSettings.SanitizeNickname(rawNick, fallback: "");
            if (string.IsNullOrEmpty(cleanNick))
            {
                if (nicknameValidationText != null)
                {
                    nicknameValidationText.text = "⚠ Debes escribir un nickname (mínimo 2 caracteres) antes de elegir personaje.";
                }
                PlaySfx(SfxType.PlayerHurt);
                if (nicknameInputField != null)
                {
                    nicknameInputField.Select();
                    nicknameInputField.ActivateInputField();
                }
                return;
            }

            GameSettings.PlayerNickname = cleanNick;
            PlayerPrefs.SetInt("CDE_SelectedCharacterIndex", characterIndex);
            PlayerPrefs.SetInt("CDE_IsTutorialOnlyMode", 0);
            PlayerPrefs.SetInt("CDE_PendingLoadSlot", -1);
            PlayerPrefs.Save();
            if (characterSelectSubPanel != null) characterSelectSubPanel.SetActive(false);
            if (mainMenuModalBackdrop != null) mainMenuModalBackdrop.SetActive(false);
            LoadSceneWithAnimatedTransition("IntroCutscene");
        }

        private void BuildLevelSelectModal(Transform menuRoot)
        {
            levelSelectSubPanel = CreateGlassCard(menuRoot, "LevelSelect_GlassSubPanel",
                new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.90f),
                new Color(0.02f, 0.05f, 0.13f, 0.96f), new Color(0.30f, 0.95f, 1.0f, 0.96f));
            levelSelectCardRect = levelSelectSubPanel.GetComponent<RectTransform>();

            CreateDynamicText(levelSelectSubPanel.transform, "LS_Title", "SELECCIONAR NIVEL", 23,
                new Vector2(0.04f, 0.87f), new Vector2(0.96f, 0.96f), new Color(0.30f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            CreateDynamicText(levelSelectSubPanel.transform, "LS_Subtitle",
                "Elige el tutorial o cualquiera de los cuatro niveles.",
                15, new Vector2(0.04f, 0.79f), new Vector2(0.96f, 0.87f), new Color(0.85f, 0.96f, 1.0f, 0.95f));

            string[] titles = new string[] { "🎓 TUTORIAL", "🔬 NIVEL 1", "⚡ NIVEL 2", "🖥 NIVEL 3", "👑 NIVEL 4" };
            string[] subtitles = new string[]
            {
                "Entrenamiento y Mini-Jefe",
                "Laboratorio de Cómputo",
                "Centro de Redes",
                "Torre de Servidores",
                "Núcleo Central S.I.A."
            };
            string[] scenes = new string[] { "TutorialScene", "SampleScene", "Level2Scene", "Level3Scene", "Level4Scene" };
            Color[] accents = new Color[]
            {
                new Color(0.30f, 1.0f, 0.75f, 0.96f),
                new Color(0.25f, 0.92f, 1.0f, 0.96f),
                new Color(1.0f, 0.75f, 0.25f, 0.96f),
                new Color(0.75f, 0.45f, 1.0f, 0.96f),
                new Color(1.0f, 0.35f, 0.52f, 0.96f)
            };

            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                bool topRow = i < 3;
                float xMin = topRow ? (0.04f + i * 0.312f) : (0.18f + (i - 3) * 0.34f);
                float width = topRow ? 0.296f : 0.30f;
                float yMax = topRow ? 0.76f : 0.46f;
                float yMin = topRow ? 0.49f : 0.19f;

                GameObject lvlCard = CreateGlassCard(levelSelectSubPanel.transform, $"Level_Card_{i}",
                    new Vector2(xMin, yMin), new Vector2(xMin + width, yMax),
                    new Color(0.03f, 0.08f, 0.18f, 0.94f), accents[i]);

                CreateDynamicText(lvlCard.transform, "Lvl_Title", titles[i], 18,
                    new Vector2(0.06f, 0.62f), new Vector2(0.94f, 0.92f), accents[i], TextAnchor.MiddleCenter, useHeaderFont: true);

                CreateDynamicText(lvlCard.transform, "Lvl_Desc", subtitles[i], 14,
                    new Vector2(0.06f, 0.36f), new Vector2(0.94f, 0.62f), new Color(0.92f, 0.97f, 1.0f, 0.96f));

                CreateGlassButton(lvlCard.transform, $"Btn_Play_Lvl_{i}", "▶ JUGAR",
                    new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.34f),
                    new Color(0.06f, 0.42f, 0.54f, 0.95f), accents[i],
                    () =>
                    {
                        PlayerPrefs.SetInt("CDE_PendingLoadSlot", -1);
                        PlayerPrefs.SetInt("CDE_IsTutorialOnlyMode", 0);
                        PlayerPrefs.Save();
                        CloseMainMenuSubPanel(levelSelectSubPanel, levelSelectCardRect);
                        LoadSceneWithAnimatedTransition(scenes[idx]);
                    });
            }

            CreateGlassButton(levelSelectSubPanel.transform, "Btn_LS_Back", "↩  VOLVER",
                new Vector2(0.36f, 0.04f), new Vector2(0.64f, 0.14f),
                new Color(0.38f, 0.10f, 0.18f, 0.94f), new Color(1.0f, 0.45f, 0.55f, 0.95f),
                () => CloseMainMenuSubPanel(levelSelectSubPanel, levelSelectCardRect));

            levelSelectSubPanel.SetActive(false);
        }

        private void BuildSavedGamesModal(Transform menuRoot)
        {
            savedGamesSubPanel = CreateGlassCard(menuRoot, "SavedGames_GlassSubPanel",
                new Vector2(0.10f, 0.08f), new Vector2(0.90f, 0.90f),
                new Color(0.02f, 0.05f, 0.13f, 0.96f), new Color(0.25f, 1.0f, 0.72f, 0.96f));
            savedGamesCardRect = savedGamesSubPanel.GetComponent<RectTransform>();

            CreateDynamicText(savedGamesSubPanel.transform, "SG_Title", "PARTIDAS GUARDADAS", 23,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), new Color(0.30f, 1.0f, 0.75f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            CreateDynamicText(savedGamesSubPanel.transform, "SG_Subtitle",
                "Retoma tu progreso desde el último checkpoint guardado.",
                15, new Vector2(0.05f, 0.81f), new Vector2(0.95f, 0.88f), new Color(0.85f, 0.96f, 1.0f, 0.95f));

            for (int i = 0; i < 4; i++)
            {
                int slotIdx = i;
                float yMax = 0.79f - i * 0.155f;
                float yMin = yMax - 0.140f;

                GameObject slotRow = CreateGlassCard(savedGamesSubPanel.transform, $"SaveSlot_Row_{i}",
                    new Vector2(0.04f, yMin), new Vector2(0.96f, yMax),
                    new Color(0.03f, 0.08f, 0.17f, 0.94f), new Color(0.25f, 0.90f, 1.0f, 0.78f));

                // Badge numérico lateral de ranura
                GameObject badgeObj = CreateGlassCard(slotRow.transform, "Slot_Badge",
                    new Vector2(0.015f, 0.14f), new Vector2(0.068f, 0.86f),
                    new Color(0.06f, 0.22f, 0.34f, 0.96f), new Color(0.25f, 1.0f, 0.75f, 0.95f));
                CreateDynamicText(badgeObj.transform, "Badge_Num", $"0{i + 1}", 17,
                    Vector2.zero, Vector2.one, new Color(0.35f, 1.0f, 0.82f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

                // Retrato del personaje guardado
                GameObject portraitFrame = CreateGlassCard(slotRow.transform, "Slot_Portrait_Frame",
                    new Vector2(0.08f, 0.12f), new Vector2(0.155f, 0.88f),
                    new Color(0.02f, 0.05f, 0.11f, 0.95f), new Color(0.25f, 0.88f, 1.0f, 0.75f));
                GameObject portraitImgObj = new GameObject("Portrait_Img");
                portraitImgObj.transform.SetParent(portraitFrame.transform, false);
                RectTransform rtPort = portraitImgObj.AddComponent<RectTransform>();
                rtPort.anchorMin = new Vector2(0.1f, 0.1f);
                rtPort.anchorMax = new Vector2(0.9f, 0.9f);
                rtPort.offsetMin = Vector2.zero;
                rtPort.offsetMax = Vector2.zero;
                saveSlotPortraitImages[i] = portraitImgObj.AddComponent<Image>();
                saveSlotPortraitImages[i].preserveAspect = true;
                saveSlotPortraitImages[i].raycastTarget = false;

                saveSlotTitleTexts[i] = CreateDynamicText(slotRow.transform, "Slot_Title", "", 16,
                    new Vector2(0.17f, 0.52f), new Vector2(0.65f, 0.92f), new Color(0.35f, 1.0f, 0.85f, 1f), TextAnchor.MiddleLeft);

                saveSlotMetaTexts[i] = CreateDynamicText(slotRow.transform, "Slot_Meta", "", 14,
                    new Vector2(0.17f, 0.10f), new Vector2(0.65f, 0.52f), new Color(0.88f, 0.95f, 1.0f, 0.94f), TextAnchor.MiddleLeft);

                GameObject loadBtnObj = CreateGlassButton(slotRow.transform, $"Btn_Load_Slot_{i}", "▶ CARGAR",
                    new Vector2(0.66f, 0.16f), new Vector2(0.82f, 0.84f),
                    new Color(0.06f, 0.46f, 0.38f, 0.95f), new Color(0.25f, 1.0f, 0.72f, 1f),
                    () => LoadSavedGameFromSlot(slotIdx));
                saveSlotLoadButtons[i] = loadBtnObj.GetComponent<Button>();

                saveSlotDeleteButtons[i] = CreateGlassButton(slotRow.transform, $"Btn_Delete_Slot_{i}", "🗑 BORRAR",
                    new Vector2(0.84f, 0.16f), new Vector2(0.985f, 0.84f),
                    new Color(0.42f, 0.10f, 0.18f, 0.92f), new Color(1.0f, 0.42f, 0.52f, 0.95f),
                    () => RequestDeleteSavedGameSlot(slotIdx));
            }

            CreateGlassButton(savedGamesSubPanel.transform, "Btn_SG_Back", "↩  VOLVER",
                new Vector2(0.36f, 0.04f), new Vector2(0.64f, 0.14f),
                new Color(0.38f, 0.10f, 0.18f, 0.94f), new Color(1.0f, 0.45f, 0.55f, 0.95f),
                () => CloseMainMenuSubPanel(savedGamesSubPanel, savedGamesCardRect));

            // Sub-modal de confirmación antes de borrar una partida guardada
            BuildDeleteConfirmModal(savedGamesSubPanel.transform);

            RefreshSavedGamesSubPanel();
            savedGamesSubPanel.SetActive(false);
        }

        private void BuildDeleteConfirmModal(Transform parent)
        {
            deleteConfirmModal = new GameObject("Delete_Save_Confirm_Backdrop");
            deleteConfirmModal.transform.SetParent(parent, false);
            StretchFullScreen(deleteConfirmModal);
            Image bg = deleteConfirmModal.AddComponent<Image>();
            bg.color = new Color(0.01f, 0.01f, 0.03f, 0.86f);

            GameObject card = CreateGlassCard(deleteConfirmModal.transform, "Delete_Confirm_GlassCard",
                new Vector2(0.18f, 0.22f), new Vector2(0.82f, 0.78f),
                new Color(0.07f, 0.02f, 0.05f, 0.98f), new Color(1.0f, 0.30f, 0.42f, 0.98f));
            deleteConfirmCardRect = card.GetComponent<RectTransform>();

            CreateDynamicText(card.transform, "Del_Title", "⚠  CONFIRMAR ELIMINACIÓN", 21,
                new Vector2(0.06f, 0.74f), new Vector2(0.94f, 0.93f), new Color(1.0f, 0.38f, 0.48f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            deleteConfirmBodyText = CreateDynamicText(card.transform, "Del_Body", "", 15,
                new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.72f), new Color(0.96f, 0.94f, 0.96f, 1f));

            CreateGlassButton(card.transform, "Btn_Confirm_Delete", "🗑  SÍ, ELIMINAR",
                new Vector2(0.08f, 0.08f), new Vector2(0.48f, 0.25f),
                new Color(0.55f, 0.10f, 0.18f, 0.96f), new Color(1.0f, 0.35f, 0.45f, 1f),
                ConfirmDeleteSavedGameSlot);

            CreateGlassButton(card.transform, "Btn_Cancel_Delete", "↩  CANCELAR",
                new Vector2(0.52f, 0.08f), new Vector2(0.92f, 0.25f),
                new Color(0.10f, 0.28f, 0.44f, 0.95f), new Color(0.35f, 0.95f, 1.0f, 1f),
                () =>
                {
                    pendingDeleteSlotIndex = -1;
                    AnimatePanelHide(deleteConfirmModal, deleteConfirmCardRect, new Vector2(0f, -35f), 0.18f);
                });

            deleteConfirmModal.SetActive(false);
        }

        private static string FormatReadableSceneName(string sceneName)
        {
            switch (sceneName)
            {
                case "TutorialScene": return "Tutorial";
                case "SampleScene": return "Nivel 1";
                case "Level2Scene": return "Nivel 2";
                case "Level3Scene": return "Nivel 3";
                case "Level4Scene": return "Nivel 4";
                case "EndingCutscene": return "Campaña Final";
                default: return string.IsNullOrEmpty(sceneName) ? "Nivel 1" : sceneName;
            }
        }

        private string FormatSaveSlotTitle(int slotIndex)
        {
            string prefix = $"CDE_SaveSlot_{slotIndex}_";
            if (PlayerPrefs.GetInt(prefix + "HasData", 0) == 0)
            {
                return $"RANURA {slotIndex + 1}  —  VACÍA";
            }

            string nick = GameSettings.SanitizeNickname(PlayerPrefs.GetString(prefix + "Nickname", ""), fallback: "Agente");
            bool completed = PlayerPrefs.GetInt(prefix + "Completed", 0) == 1;
            string readableScene = FormatReadableSceneName(PlayerPrefs.GetString(prefix + "Scene", "SampleScene"));
            int charIdx = Mathf.Clamp(PlayerPrefs.GetInt(prefix + "CharIndex", 0), 0, CharacterNames.Length - 1);
            string charShort = charIdx == 0 ? "Kunoichi" : "Shinobi";
            return completed
                ? $"🏆 CAMPAÑA COMPLETADA  •  {nick} ({charShort})"
                : $"{readableScene.ToUpper()}  •  {nick} ({charShort})";
        }

        private string FormatSaveSlotMeta(int slotIndex)
        {
            string prefix = $"CDE_SaveSlot_{slotIndex}_";
            if (PlayerPrefs.GetInt(prefix + "HasData", 0) == 0)
            {
                return "Sin partida guardada en esta ranura";
            }

            int hp = PlayerPrefs.GetInt(prefix + "Health", 100);
            int lives = PlayerPrefs.GetInt(prefix + "Lives", 3);
            int cp = PlayerPrefs.GetInt(prefix + "Checkpoint", 0);
            string date = PlayerPrefs.GetString(prefix + "Timestamp", "Reciente");
            bool completed = PlayerPrefs.GetInt(prefix + "Completed", 0) == 1;
            string cpText = completed ? "Medalla UTNG" : (cp > 0 ? $"Checkpoint {cp}" : "Inicio del nivel");
            return $"♥ {lives} Vidas   |   ✚ {hp}/100 HP   |   🚩 {cpText}   |   🕒 {date}";
        }

        private void RefreshSavedGamesSubPanel()
        {
            for (int i = 0; i < 4; i++)
            {
                string prefix = $"CDE_SaveSlot_{i}_";
                bool hasData = PlayerPrefs.GetInt(prefix + "HasData", 0) == 1;
                int charIdx = Mathf.Clamp(PlayerPrefs.GetInt(prefix + "CharIndex", 0), 0, CharacterNames.Length - 1);
                Sprite portSprite = (playableCharacterIdleSprites != null && charIdx < playableCharacterIdleSprites.Length)
                    ? playableCharacterIdleSprites[charIdx]
                    : null;

                if (saveSlotTitleTexts[i] != null)
                {
                    saveSlotTitleTexts[i].text = FormatSaveSlotTitle(i);
                    saveSlotTitleTexts[i].color = hasData ? new Color(0.35f, 1.0f, 0.85f, 1f) : new Color(0.65f, 0.72f, 0.82f, 0.85f);
                }
                if (saveSlotMetaTexts[i] != null)
                {
                    saveSlotMetaTexts[i].text = FormatSaveSlotMeta(i);
                    saveSlotMetaTexts[i].color = hasData ? new Color(0.90f, 0.96f, 1.0f, 0.95f) : new Color(0.55f, 0.62f, 0.72f, 0.78f);
                }
                if (saveSlotPortraitImages[i] != null)
                {
                    saveSlotPortraitImages[i].sprite = portSprite;
                    saveSlotPortraitImages[i].enabled = (portSprite != null);
                    saveSlotPortraitImages[i].color = hasData ? Color.white : new Color(0.45f, 0.55f, 0.68f, 0.28f);
                }
                if (saveSlotLoadButtons[i] != null)
                {
                    saveSlotLoadButtons[i].interactable = hasData;
                }
                if (saveSlotDeleteButtons[i] != null)
                {
                    saveSlotDeleteButtons[i].SetActive(hasData);
                }

                if (pauseSaveSlotTitleTexts[i] != null)
                {
                    pauseSaveSlotTitleTexts[i].text = FormatSaveSlotTitle(i);
                    pauseSaveSlotTitleTexts[i].color = hasData ? new Color(0.35f, 1.0f, 0.85f, 1f) : new Color(0.65f, 0.72f, 0.82f, 0.85f);
                }
                if (pauseSaveSlotMetaTexts[i] != null)
                {
                    pauseSaveSlotMetaTexts[i].text = FormatSaveSlotMeta(i);
                    pauseSaveSlotMetaTexts[i].color = hasData ? new Color(0.90f, 0.96f, 1.0f, 0.95f) : new Color(0.55f, 0.62f, 0.72f, 0.78f);
                }
                if (pauseSaveSlotPortraits[i] != null)
                {
                    pauseSaveSlotPortraits[i].sprite = portSprite;
                    pauseSaveSlotPortraits[i].enabled = (portSprite != null);
                    pauseSaveSlotPortraits[i].color = hasData ? Color.white : new Color(0.45f, 0.55f, 0.68f, 0.28f);
                }
            }
        }

        private void LoadSavedGameFromSlot(int slotIndex)
        {
            string prefix = $"CDE_SaveSlot_{slotIndex}_";
            if (PlayerPrefs.GetInt(prefix + "HasData", 0) == 0)
            {
                PlaySfx(SfxType.PlayerHurt);
                return;
            }

            string targetScene = PlayerPrefs.GetString(prefix + "Scene", "SampleScene");
            int savedChar = PlayerPrefs.GetInt(prefix + "CharIndex", 0);
            string savedNick = GameSettings.SanitizeNickname(PlayerPrefs.GetString(prefix + "Nickname", ""), fallback: "Agente");

            GameSettings.PlayerNickname = savedNick;
            PlayerPrefs.SetInt("CDE_SelectedCharacterIndex", savedChar);
            PlayerPrefs.SetInt("CDE_ActiveSaveSlot", slotIndex);
            PlayerPrefs.SetInt("CDE_PendingLoadSlot", slotIndex);
            PlayerPrefs.SetInt("CDE_IsTutorialOnlyMode", 0);
            PlayerPrefs.Save();

            LoadSceneWithAnimatedTransition(targetScene);
        }

        private void RequestDeleteSavedGameSlot(int slotIndex)
        {
            string prefix = $"CDE_SaveSlot_{slotIndex}_";
            if (PlayerPrefs.GetInt(prefix + "HasData", 0) == 0 || deleteConfirmModal == null) return;

            pendingDeleteSlotIndex = slotIndex;
            if (deleteConfirmBodyText != null)
            {
                deleteConfirmBodyText.text =
                    $"¿Seguro que deseas eliminar la Ranura {slotIndex + 1}?\n\n" +
                    $"<color=#40E0D0><b>{FormatSaveSlotTitle(slotIndex)}</b></color>\n" +
                    $"{FormatSaveSlotMeta(slotIndex)}\n\n" +
                    "Esta acción es permanente y no se puede deshacer.";
            }

            deleteConfirmModal.transform.SetAsLastSibling();
            AnimatePanelShow(deleteConfirmModal, deleteConfirmCardRect, new Vector2(0f, -30f), 0.22f);
        }

        private void ConfirmDeleteSavedGameSlot()
        {
            if (pendingDeleteSlotIndex < 0 || pendingDeleteSlotIndex >= 4) return;

            string prefix = $"CDE_SaveSlot_{pendingDeleteSlotIndex}_";
            PlayerPrefs.SetInt(prefix + "HasData", 0);
            PlayerPrefs.Save();
            pendingDeleteSlotIndex = -1;

            PlaySfx(SfxType.EnemyHit);
            RefreshSavedGamesSubPanel();
            if (deleteConfirmModal != null)
            {
                AnimatePanelHide(deleteConfirmModal, deleteConfirmCardRect, new Vector2(0f, -30f), 0.18f);
            }
        }

        private void SaveCurrentGameToSlotAndExit(int slotIndex)
        {
            if (isSavingFromEnding)
            {
                isSavingFromEnding = false;
                string endPrefix = $"CDE_SaveSlot_{slotIndex}_";
                int endCharIdx = PlayerPrefs.GetInt("CDE_SelectedCharacterIndex", 0);
                int endLives = PlayerPrefs.GetInt("CDE_EndingLastLives", GameSettings.StartingLives);
                string endTimestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                PlayerPrefs.SetInt(endPrefix + "HasData", 1);
                PlayerPrefs.SetString(endPrefix + "Scene", "SampleScene");
                PlayerPrefs.SetInt(endPrefix + "CharIndex", endCharIdx);
                PlayerPrefs.SetString(endPrefix + "Nickname", GameSettings.PlayerNickname);
                PlayerPrefs.SetInt(endPrefix + "Completed", 1);
                PlayerPrefs.SetInt(endPrefix + "Health", 100);
                PlayerPrefs.SetInt(endPrefix + "Lives", Mathf.Max(1, endLives));
                PlayerPrefs.SetInt(endPrefix + "Checkpoint", 0);
                PlayerPrefs.SetFloat(endPrefix + "PosX", -24f);
                PlayerPrefs.SetFloat(endPrefix + "PosY", -1.75f);
                PlayerPrefs.SetString(endPrefix + "Timestamp", endTimestamp);
                PlayerPrefs.SetInt("CDE_ActiveSaveSlot", slotIndex);
                PlayerPrefs.Save();

                RefreshSavedGamesSubPanel();
                if (pauseSaveSlotsPanel != null) AnimatePanelHide(pauseSaveSlotsPanel, pauseSaveSlotsCardRect, new Vector2(0f, -45f), 0.22f);
                if (endingSaveNotificationText != null)
                {
                    endingSaveNotificationText.text = $"💾 Partida completada guardada en Ranura {slotIndex + 1} ({GameSettings.PlayerNickname}).";
                    endingSaveNotificationText.color = new Color(0.25f, 1.0f, 0.70f, 1f);
                }
                return;
            }

            if (isSavingFromVictory)
            {
                isSavingFromVictory = false;
                SaveNextLevelProgressToSlot(slotIndex);
                RefreshSavedGamesSubPanel();
                LoadSceneWithAnimatedTransition("MainMenu");
                return;
            }

            string prefix = $"CDE_SaveSlot_{slotIndex}_";
            string activeScene = SceneManager.GetActiveScene().name;
            int charIdx = PlayerPrefs.GetInt("CDE_SelectedCharacterIndex", 0);
            int hp = playerHealth != null ? playerHealth.CurrentHealth : 100;
            int lives = playerHealth != null ? playerHealth.CurrentLives : GameSettings.StartingLives;
            int cp = playerHealth != null ? playerHealth.ActiveCheckpointIndex : 0;
            // Guardar siempre la posición exacta del último checkpoint alcanzado (nunca en medio del aire/nivel sin checkpoint)
            Vector3 pos = playerHealth != null ? playerHealth.LastCheckpointPosition : new Vector3(-21f, -1.75f, 0f);
            string timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            PlayerPrefs.SetInt(prefix + "HasData", 1);
            PlayerPrefs.SetString(prefix + "Scene", activeScene);
            PlayerPrefs.SetInt(prefix + "CharIndex", charIdx);
            PlayerPrefs.SetString(prefix + "Nickname", GameSettings.PlayerNickname);
            PlayerPrefs.SetInt(prefix + "Completed", 0);
            PlayerPrefs.SetInt(prefix + "Health", hp);
            PlayerPrefs.SetInt(prefix + "Lives", lives);
            PlayerPrefs.SetInt(prefix + "Checkpoint", cp);
            PlayerPrefs.SetFloat(prefix + "PosX", pos.x);
            PlayerPrefs.SetFloat(prefix + "PosY", pos.y);
            PlayerPrefs.SetString(prefix + "Timestamp", timestamp);
            PlayerPrefs.SetInt("CDE_ActiveSaveSlot", slotIndex);
            PlayerPrefs.Save();

            RefreshSavedGamesSubPanel();
            LoadSceneWithAnimatedTransition("MainMenu");
        }

        private void SaveNextLevelProgressToSlot(int slotIndex)
        {
            string prefix = $"CDE_SaveSlot_{slotIndex}_";
            string targetNext = string.IsNullOrEmpty(nextSceneName) ? "SampleScene" : nextSceneName;
            if (targetNext == "MainMenu" || targetNext == "EndingCutscene")
            {
                targetNext = SceneManager.GetActiveScene().name;
            }

            int charIdx = PlayerPrefs.GetInt("CDE_SelectedCharacterIndex", 0);
            float defaultStartX = (targetNext == "TutorialScene") ? -21f : -24f;
            string timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            PlayerPrefs.SetInt(prefix + "HasData", 1);
            PlayerPrefs.SetString(prefix + "Scene", targetNext);
            PlayerPrefs.SetInt(prefix + "CharIndex", charIdx);
            PlayerPrefs.SetString(prefix + "Nickname", GameSettings.PlayerNickname);
            PlayerPrefs.SetInt(prefix + "Completed", 0);
            PlayerPrefs.SetInt(prefix + "Health", 100);
            PlayerPrefs.SetInt(prefix + "Lives", GameSettings.StartingLives);
            PlayerPrefs.SetInt(prefix + "Checkpoint", 0);
            PlayerPrefs.SetFloat(prefix + "PosX", defaultStartX);
            PlayerPrefs.SetFloat(prefix + "PosY", -1.75f);
            PlayerPrefs.SetString(prefix + "Timestamp", timestamp);
            PlayerPrefs.SetInt("CDE_ActiveSaveSlot", slotIndex);
            PlayerPrefs.Save();
            Debug.Log($"<color=green><b>[PARTIDA GUARDADA]</b> Progreso del siguiente nivel ({targetNext}) guardado en Ranura {slotIndex + 1}.</color>");
        }

        private void EnsureUnifiedSettingsUI()
        {
            if (unifiedSettingsOverlay != null) return;

            unifiedSettingsOverlay = new GameObject("Unified_Settings_Modal_Overlay");
            unifiedSettingsOverlay.transform.SetParent(transform, false);
            StretchFullScreen(unifiedSettingsOverlay);
            Image bgDim = unifiedSettingsOverlay.AddComponent<Image>();
            bgDim.color = new Color(0.01f, 0.02f, 0.05f, 0.88f);

            GameObject card = CreateGlassCard(unifiedSettingsOverlay.transform, "Unified_Settings_Card",
                new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.95f),
                new Color(0.02f, 0.05f, 0.13f, 0.97f), new Color(0.25f, 0.95f, 1.0f, 0.96f));
            unifiedSettingsCardRect = card.GetComponent<RectTransform>();

            CreateDynamicText(card.transform, "Set_Title", "⚙  CONFIGURACIÓN", 23,
                new Vector2(0.04f, 0.90f), new Vector2(0.96f, 0.98f), new Color(0.25f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            // Barra superior de 3 pestañas reales
            GameObject btnAudioObj = CreateGlassButton(card.transform, "Tab_Btn_Audio", "🔊  AUDIO",
                new Vector2(0.05f, 0.81f), new Vector2(0.34f, 0.89f),
                new Color(0.08f, 0.42f, 0.56f, 0.96f), new Color(0.30f, 0.96f, 1.0f, 1f),
                () => SwitchSettingsTab(0));
            settingsTabBtnAudioBg = btnAudioObj.GetComponent<Image>();

            GameObject btnVideoObj = CreateGlassButton(card.transform, "Tab_Btn_Video", "🖥  VIDEO Y JUEGO",
                new Vector2(0.355f, 0.81f), new Vector2(0.645f, 0.89f),
                new Color(0.05f, 0.14f, 0.28f, 0.92f), new Color(0.30f, 0.96f, 1.0f, 1f),
                () => SwitchSettingsTab(1));
            settingsTabBtnVideoBg = btnVideoObj.GetComponent<Image>();

            GameObject btnCtrlObj = CreateGlassButton(card.transform, "Tab_Btn_Controls", "⌨  CONTROLES",
                new Vector2(0.66f, 0.81f), new Vector2(0.95f, 0.89f),
                new Color(0.05f, 0.14f, 0.28f, 0.92f), new Color(0.30f, 0.96f, 1.0f, 1f),
                () => SwitchSettingsTab(2));
            settingsTabBtnControlsBg = btnCtrlObj.GetComponent<Image>();

            // =========================================================================
            // PESTAÑA 1: AUDIO COMPLETO
            // =========================================================================
            settingsTabAudio = new GameObject("Settings_Tab_Audio");
            settingsTabAudio.transform.SetParent(card.transform, false);
            StretchFullScreen(settingsTabAudio);

            CreateDynamicText(settingsTabAudio.transform, "Audio_Header", "🔊 CONTROL INDEPENDIENTE DE AUDIO", 16,
                new Vector2(0.06f, 0.71f), new Vector2(0.58f, 0.78f), new Color(0.45f, 0.96f, 1.0f, 1f), TextAnchor.MiddleLeft);

            GameObject muteBtn = CreateGlassButton(settingsTabAudio.transform, "Btn_Set_Mute", "",
                new Vector2(0.60f, 0.70f), new Vector2(0.94f, 0.785f),
                new Color(0.10f, 0.28f, 0.44f, 0.94f), new Color(0.35f, 0.95f, 1.0f, 1f),
                () =>
                {
                    AudioListener.pause = !AudioListener.pause;
                    RefreshSettingsLabels();
                });
            settingsAudioMuteLabel = muteBtn.GetComponentInChildren<Text>();

            settingsVolumeLabel = CreateDynamicText(settingsTabAudio.transform, "Set_Vol_Label", "", 15,
                new Vector2(0.06f, 0.60f), new Vector2(0.48f, 0.67f), new Color(0.45f, 1.0f, 0.85f, 1f), TextAnchor.MiddleLeft);
            settingsVolumeSlider = CreateGlassVolumeSlider(settingsTabAudio.transform, "Set_Vol_Slider",
                new Vector2(0.06f, 0.52f), new Vector2(0.48f, 0.60f), masterVolume, OnMasterVolumeSliderChanged);

            settingsMusicLabel = CreateDynamicText(settingsTabAudio.transform, "Set_Music_Label", "", 15,
                new Vector2(0.52f, 0.60f), new Vector2(0.94f, 0.67f), new Color(0.45f, 1.0f, 0.85f, 1f), TextAnchor.MiddleLeft);
            settingsMusicSlider = CreateGlassVolumeSlider(settingsTabAudio.transform, "Set_Music_Slider",
                new Vector2(0.52f, 0.52f), new Vector2(0.94f, 0.60f), musicVolume, OnMusicVolumeSliderChanged);

            settingsSfxLabel = CreateDynamicText(settingsTabAudio.transform, "Set_Sfx_Label", "", 15,
                new Vector2(0.06f, 0.41f), new Vector2(0.48f, 0.48f), new Color(0.45f, 1.0f, 0.85f, 1f), TextAnchor.MiddleLeft);
            settingsSfxSlider = CreateGlassVolumeSlider(settingsTabAudio.transform, "Set_Sfx_Slider",
                new Vector2(0.06f, 0.33f), new Vector2(0.48f, 0.41f), sfxVolume, OnSfxVolumeSliderChanged);

            settingsEnvLabel = CreateDynamicText(settingsTabAudio.transform, "Set_Env_Label", "", 15,
                new Vector2(0.52f, 0.41f), new Vector2(0.94f, 0.48f), new Color(0.45f, 1.0f, 0.85f, 1f), TextAnchor.MiddleLeft);
            settingsEnvSlider = CreateGlassVolumeSlider(settingsTabAudio.transform, "Set_Env_Slider",
                new Vector2(0.52f, 0.33f), new Vector2(0.94f, 0.41f), envVolume, OnEnvVolumeSliderChanged);

            GameObject jumpSfxBtn = CreateGlassButton(settingsTabAudio.transform, "Btn_Set_JumpSfx", "",
                new Vector2(0.06f, 0.17f), new Vector2(0.58f, 0.27f),
                new Color(0.10f, 0.28f, 0.44f, 0.94f), new Color(0.35f, 0.95f, 1.0f, 1f),
                ToggleJumpSfxEnabled);
            settingsJumpSfxLabel = jumpSfxBtn.GetComponentInChildren<Text>();

            CreateGlassButton(settingsTabAudio.transform, "Btn_Test_Sfx", "🔊 PROBAR SONIDO",
                new Vector2(0.61f, 0.17f), new Vector2(0.94f, 0.27f),
                new Color(0.06f, 0.42f, 0.48f, 0.94f), new Color(0.25f, 1.0f, 0.78f, 1f),
                () => PlaySfx(SfxType.ShootNormal));

            // =========================================================================
            // PESTAÑA 2: VIDEO, JUEGO Y DIFICULTAD REALES
            // =========================================================================
            settingsTabVideo = new GameObject("Settings_Tab_Video");
            settingsTabVideo.transform.SetParent(card.transform, false);
            StretchFullScreen(settingsTabVideo);

            settingsFullscreenLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_Fullscreen", "",
                new Vector2(0.05f, 0.70f), new Vector2(0.485f, 0.785f),
                new Color(0.08f, 0.28f, 0.46f, 0.94f), new Color(0.30f, 0.92f, 1.0f, 1f),
                () =>
                {
                    GameSettings.ApplyResolution(!Screen.fullScreen);
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsResolutionLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_Resolution", "",
                new Vector2(0.05f, 0.595f), new Vector2(0.485f, 0.68f),
                new Color(0.08f, 0.28f, 0.46f, 0.94f), new Color(0.30f, 0.92f, 1.0f, 1f),
                () =>
                {
                    GameSettings.ResolutionIndex = (GameSettings.ResolutionIndex + 1) % 4;
                    GameSettings.ApplyResolution(Screen.fullScreen);
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsVsyncLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_Vsync", "",
                new Vector2(0.05f, 0.49f), new Vector2(0.485f, 0.575f),
                new Color(0.08f, 0.28f, 0.46f, 0.94f), new Color(0.30f, 0.92f, 1.0f, 1f),
                () =>
                {
                    GameSettings.VSyncEnabled = !GameSettings.VSyncEnabled;
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsFpsLimitLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_FpsLimit", "",
                new Vector2(0.05f, 0.385f), new Vector2(0.485f, 0.47f),
                new Color(0.08f, 0.28f, 0.46f, 0.94f), new Color(0.30f, 0.92f, 1.0f, 1f),
                () =>
                {
                    GameSettings.FpsLimitIndex = (GameSettings.FpsLimitIndex + 1) % 4;
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsShowFpsLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_ShowFps", "",
                new Vector2(0.05f, 0.28f), new Vector2(0.485f, 0.365f),
                new Color(0.08f, 0.28f, 0.46f, 0.94f), new Color(0.30f, 0.92f, 1.0f, 1f),
                () =>
                {
                    GameSettings.ShowFpsCounter = !GameSettings.ShowFpsCounter;
                    EnsureFpsCounterUI();
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsBrightnessLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_Brightness", "",
                new Vector2(0.515f, 0.70f), new Vector2(0.95f, 0.785f),
                new Color(0.10f, 0.26f, 0.46f, 0.94f), new Color(1.0f, 0.85f, 0.35f, 1f),
                () =>
                {
                    GameSettings.BrightnessLevel = (GameSettings.BrightnessLevel + 1) % 3;
                    ApplyBrightnessLive();
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsZoomLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_Zoom", "",
                new Vector2(0.515f, 0.595f), new Vector2(0.95f, 0.68f),
                new Color(0.10f, 0.26f, 0.46f, 0.94f), new Color(0.35f, 1.0f, 0.80f, 1f),
                () =>
                {
                    GameSettings.CameraZoomLevel = (GameSettings.CameraZoomLevel + 1) % 3;
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsEffectsLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_Effects", "",
                new Vector2(0.515f, 0.49f), new Vector2(0.95f, 0.575f),
                new Color(0.10f, 0.26f, 0.46f, 0.94f), new Color(0.35f, 1.0f, 0.80f, 1f),
                () =>
                {
                    GameSettings.ScreenEffectsLevel = (GameSettings.ScreenEffectsLevel + 1) % 3;
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsEnemyBarsLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_EnemyBars", "",
                new Vector2(0.515f, 0.385f), new Vector2(0.95f, 0.47f),
                new Color(0.10f, 0.26f, 0.46f, 0.94f), new Color(0.35f, 1.0f, 0.80f, 1f),
                () =>
                {
                    GameSettings.ShowEnemyHealthBars = !GameSettings.ShowEnemyHealthBars;
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsHudScaleLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_HudScale", "",
                new Vector2(0.515f, 0.28f), new Vector2(0.95f, 0.365f),
                new Color(0.10f, 0.26f, 0.46f, 0.94f), new Color(0.35f, 1.0f, 0.80f, 1f),
                () =>
                {
                    GameSettings.HudScaleLevel = (GameSettings.HudScaleLevel + 1) % 3;
                    ApplyHudScaleLive();
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsDifficultyLabel = CreateGlassButton(settingsTabVideo.transform, "Btn_Set_Difficulty", "",
                new Vector2(0.05f, 0.175f), new Vector2(0.95f, 0.26f),
                new Color(0.24f, 0.16f, 0.38f, 0.95f), new Color(1.0f, 0.72f, 0.30f, 1f),
                () =>
                {
                    GameSettings.Difficulty = (GameSettings.Difficulty + 1) % 3;
                    RefreshSettingsLabels();
                }).GetComponentInChildren<Text>();

            settingsDifficultySummaryText = CreateDynamicText(settingsTabVideo.transform, "Set_Diff_Summary", "", 13,
                new Vector2(0.05f, 0.115f), new Vector2(0.95f, 0.172f), new Color(0.90f, 0.95f, 1.0f, 0.94f));

            // =========================================================================
            // PESTAÑA 3: CONTROLES PERSONALIZABLES (1 TECLA O BOTÓN DE MOUSE, SIN REPETIDOS)
            // =========================================================================
            settingsTabControls = new GameObject("Settings_Tab_Controls");
            settingsTabControls.transform.SetParent(card.transform, false);
            StretchFullScreen(settingsTabControls);

            CreateDynamicText(settingsTabControls.transform, "Ctrl_Hint",
                "Haz clic en cualquier botón y pulsa una tecla o botón de mouse (ESC cancela • No permite teclas repetidas).",
                14, new Vector2(0.05f, 0.735f), new Vector2(0.95f, 0.79f), new Color(0.75f, 0.94f, 1.0f, 0.95f));

            for (int i = 0; i < GameInput.ActionCount; i++)
            {
                InputActionId act = (InputActionId)i;
                bool rightCol = i >= 5;
                int row = rightCol ? (i - 5) : i;
                float xMin = rightCol ? 0.512f : 0.045f;
                float xMax = rightCol ? 0.955f : 0.488f;
                float yTop = 0.73f - row * 0.104f;
                float yBot = yTop - 0.092f;

                GameObject bindRow = CreateGlassCard(settingsTabControls.transform, $"Bind_Row_{act}",
                    new Vector2(xMin, yBot), new Vector2(xMax, yTop),
                    new Color(0.03f, 0.08f, 0.18f, 0.92f), new Color(0.25f, 0.85f, 1.0f, 0.62f));

                CreateDynamicText(bindRow.transform, "Action_Name", GameInput.GetActionDisplayName(act), 16,
                    new Vector2(0.035f, 0.06f), new Vector2(0.515f, 0.94f), new Color(0.94f, 0.98f, 1.0f, 1f), TextAnchor.MiddleLeft);

                GameObject bindBtn = CreateGlassButton(bindRow.transform, $"Btn_Bind_{act}", "",
                    new Vector2(0.525f, 0.08f), new Vector2(0.975f, 0.92f),
                    new Color(0.07f, 0.36f, 0.56f, 0.98f), new Color(0.35f, 1.0f, 0.95f, 1f),
                    () => StartListeningForRebind(act));
                Text btnLabel = bindBtn.GetComponentInChildren<Text>();
                keybindButtonLabels[i] = btnLabel;
                if (btnLabel != null)
                {
                    // HD_Text_2x usa escala 0.5x para super-muestreo HD: 42 => 21 px nítidos en negrita
                    btnLabel.fontSize = 42;
                    btnLabel.fontStyle = FontStyle.Bold;
                    btnLabel.resizeTextForBestFit = true;
                    btnLabel.resizeTextMinSize = 26;
                    btnLabel.resizeTextMaxSize = 42;
                    btnLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
                    btnLabel.verticalOverflow = VerticalWrapMode.Truncate;
                    btnLabel.alignment = TextAnchor.MiddleCenter;
                    Outline crispOutline = btnLabel.gameObject.GetComponent<Outline>();
                    if (crispOutline == null) crispOutline = btnLabel.gameObject.AddComponent<Outline>();
                    crispOutline.effectColor = new Color(0f, 0.04f, 0.10f, 0.95f);
                    crispOutline.effectDistance = new Vector2(1.8f, -1.8f);
                }
            }

            keybindStatusText = CreateDynamicText(settingsTabControls.transform, "Ctrl_Status", "", 14,
                new Vector2(0.05f, 0.145f), new Vector2(0.62f, 0.21f), new Color(1.0f, 0.88f, 0.35f, 1f), TextAnchor.MiddleLeft);

            CreateGlassButton(settingsTabControls.transform, "Btn_Ctrl_Reset", "↺  RESTAURAR PREDETERMINADOS",
                new Vector2(0.63f, 0.135f), new Vector2(0.95f, 0.215f),
                new Color(0.36f, 0.14f, 0.22f, 0.94f), new Color(1.0f, 0.55f, 0.65f, 1f),
                () =>
                {
                    CancelRebindListening();
                    GameInput.ResetDefaultBindings();
                    RefreshSettingsLabels();
                    RefreshDynamicControlsTexts();
                    if (keybindStatusText != null)
                    {
                        keybindStatusText.text = "✔ Controles restaurados a sus valores predeterminados.";
                        keybindStatusText.color = new Color(0.35f, 1.0f, 0.80f, 1f);
                    }
                });

            // Botón inferior común para cerrar el panel unificado
            CreateGlassButton(card.transform, "Btn_Set_Close", "✔  APLICAR Y VOLVER",
                new Vector2(0.32f, 0.025f), new Vector2(0.68f, 0.105f),
                new Color(0.06f, 0.48f, 0.58f, 0.96f), new Color(0.25f, 0.98f, 1.0f, 1f),
                CloseUnifiedSettingsModal);

            SwitchSettingsTab(0);
            RefreshSettingsLabels();
            RefreshAudioLabels();
            unifiedSettingsOverlay.SetActive(false);
        }

        private void OpenUnifiedSettingsModal(bool fromMainMenu, int initialTab = 0)
        {
            if (unifiedSettingsOverlay == null) EnsureUnifiedSettingsUI();
            settingsOpenedFromMainMenu = fromMainMenu;
            CancelRebindListening();

            if (fromMainMenu && mainMenuModalBackdrop != null)
            {
                mainMenuModalBackdrop.SetActive(true);
                GetOrAddCanvasGroup(mainMenuModalBackdrop).alpha = 1f;
            }

            SwitchSettingsTab(initialTab);
            RefreshAudioLabels();
            RefreshSettingsLabels();

            if (unifiedSettingsOverlay != null)
            {
                unifiedSettingsOverlay.transform.SetAsLastSibling();
                AnimatePanelShow(unifiedSettingsOverlay, unifiedSettingsCardRect, new Vector2(0f, -45f), 0.28f);
            }
        }

        private void CloseUnifiedSettingsModal()
        {
            CancelRebindListening();
            PlayerPrefs.Save();
            if (settingsOpenedFromMainMenu && mainMenuModalBackdrop != null)
            {
                mainMenuModalBackdrop.SetActive(false);
            }
            if (unifiedSettingsOverlay != null)
            {
                AnimatePanelHide(unifiedSettingsOverlay, unifiedSettingsCardRect, new Vector2(0f, -45f), 0.22f);
            }
        }

        private void SwitchSettingsTab(int tabIndex)
        {
            CancelRebindListening();
            if (settingsTabAudio != null) settingsTabAudio.SetActive(tabIndex == 0);
            if (settingsTabVideo != null) settingsTabVideo.SetActive(tabIndex == 1);
            if (settingsTabControls != null) settingsTabControls.SetActive(tabIndex == 2);

            Color activeBg = new Color(0.08f, 0.44f, 0.60f, 0.96f);
            Color idleBg = new Color(0.04f, 0.12f, 0.24f, 0.90f);
            if (settingsTabBtnAudioBg != null) settingsTabBtnAudioBg.color = tabIndex == 0 ? activeBg : idleBg;
            if (settingsTabBtnVideoBg != null) settingsTabBtnVideoBg.color = tabIndex == 1 ? activeBg : idleBg;
            if (settingsTabBtnControlsBg != null) settingsTabBtnControlsBg.color = tabIndex == 2 ? activeBg : idleBg;
        }

        private void StartListeningForRebind(InputActionId action)
        {
            isListeningForRebind = true;
            listeningActionId = action;
            rebindArmTimer = 0.14f;
            RefreshSettingsLabels();

            if (keybindStatusText != null)
            {
                keybindStatusText.text = $"⌨ Pulsa una tecla o clic para '{GameInput.GetActionDisplayName(action)}' (ESC cancela)...";
                keybindStatusText.color = new Color(1.0f, 0.88f, 0.30f, 1f);
            }
        }

        private void CancelRebindListening()
        {
            if (!isListeningForRebind) return;
            isListeningForRebind = false;
            RefreshSettingsLabels();
            if (keybindStatusText != null)
            {
                keybindStatusText.text = "Asignación cancelada.";
                keybindStatusText.color = new Color(0.75f, 0.85f, 0.95f, 0.9f);
            }
        }

        private void ToggleJumpSfxEnabled()
        {
            jumpSfxEnabled = !jumpSfxEnabled;
            PlayerPrefs.SetInt("CDE_JumpSfxEnabled", jumpSfxEnabled ? 1 : 0);
            PlayerPrefs.Save();
            RefreshSettingsLabels();
        }

        private void RefreshSettingsLabels()
        {
            if (settingsAudioMuteLabel != null)
                settingsAudioMuteLabel.text = AudioListener.pause ? "🔇 AUDIO: SILENCIADO" : "🔊 AUDIO: ACTIVO";

            if (settingsJumpSfxLabel != null)
                settingsJumpSfxLabel.text = jumpSfxEnabled ? "🔊 SONIDO DE SALTO: ACTIVADO" : "🔇 SONIDO DE SALTO: DESACTIVADO";

            if (settingsFullscreenLabel != null)
                settingsFullscreenLabel.text = Screen.fullScreen ? "🖥 PANTALLA: COMPLETA" : "🪟 PANTALLA: VENTANA";

            if (settingsResolutionLabel != null)
                settingsResolutionLabel.text = $"📐 RESOLUCIÓN: {GameSettings.ResolutionDisplayName}";

            if (settingsVsyncLabel != null)
                settingsVsyncLabel.text = GameSettings.VSyncEnabled ? "⚡ V-SYNC: ACTIVADO" : "⚡ V-SYNC: DESACTIVADO";

            if (settingsFpsLimitLabel != null)
                settingsFpsLimitLabel.text = $"🎞 LÍMITE FPS: {GameSettings.FpsLimitDisplayName}";

            if (settingsShowFpsLabel != null)
                settingsShowFpsLabel.text = GameSettings.ShowFpsCounter ? "📊 MOSTRAR FPS: ACTIVADO" : "📊 MOSTRAR FPS: DESACTIVADO";

            if (settingsBrightnessLabel != null)
                settingsBrightnessLabel.text = $"☀ BRILLO ENTORNO: {GameSettings.BrightnessDisplayName}";

            if (settingsZoomLabel != null)
                settingsZoomLabel.text = $"🔍 ZOOM CÁMARA: {GameSettings.CameraZoomDisplayName}";

            if (settingsEffectsLabel != null)
                settingsEffectsLabel.text = $"💥 SACUDIDA Y DESTELLOS: {GameSettings.ScreenEffectsDisplayName}";

            if (settingsEnemyBarsLabel != null)
                settingsEnemyBarsLabel.text = GameSettings.ShowEnemyHealthBars ? "💚 BARRAS DE ENEMIGOS: VISIBLES" : "💚 BARRAS DE ENEMIGOS: OCULTAS";

            if (settingsHudScaleLabel != null)
                settingsHudScaleLabel.text = $"📏 TAMAÑO DEL HUD: {GameSettings.HudScaleDisplayName}";

            if (settingsDifficultyLabel != null)
                settingsDifficultyLabel.text = $"⚔ DIFICULTAD GENERAL: {GameSettings.DifficultyDisplayName}";

            if (settingsDifficultySummaryText != null)
            {
                string prefix = (!isMainMenuScene && !isIntroCutsceneScene && !isEndingCutsceneScene)
                    ? "(Las vidas iniciales aplican al iniciar/reiniciar nivel)  •  "
                    : "";
                settingsDifficultySummaryText.text = prefix + GameSettings.DifficultySummary;
            }

            for (int i = 0; i < GameInput.ActionCount; i++)
            {
                if (keybindButtonLabels[i] == null) continue;
                InputActionId act = (InputActionId)i;
                if (isListeningForRebind && listeningActionId == act)
                {
                    keybindButtonLabels[i].text = "... PULSA ...";
                    keybindButtonLabels[i].color = new Color(1.0f, 0.88f, 0.25f, 1f);
                }
                else
                {
                    keybindButtonLabels[i].text = GameInput.GetBindingDisplayName(act);
                    keybindButtonLabels[i].color = Color.white;
                }
            }
        }

        private void RefreshDynamicControlsTexts()
        {
            if (mainMenuControlsCol1Text != null)
            {
                mainMenuControlsCol1Text.text =
                    $"• {GameInput.GetBindingDisplayName(InputActionId.MoveLeft)} / {GameInput.GetBindingDisplayName(InputActionId.MoveRight)} : Correr\n\n" +
                    $"• {GameInput.GetBindingDisplayName(InputActionId.ClimbUp)} / {GameInput.GetBindingDisplayName(InputActionId.ClimbDown)} : Escaleras\n\n" +
                    $"• {GameInput.GetBindingDisplayName(InputActionId.Jump)} : Saltar\n\n" +
                    $"• MANTENER {GameInput.GetBindingDisplayName(InputActionId.Jump)} : Caída suave";
            }

            if (mainMenuControlsCol2Text != null)
            {
                mainMenuControlsCol2Text.text =
                    $"• {GameInput.GetBindingDisplayName(InputActionId.Dash)} : Esquive Dash\n\n" +
                    $"• {GameInput.GetBindingDisplayName(InputActionId.Melee)} : Ataque Katana\n\n" +
                    $"• {GameInput.GetBindingDisplayName(InputActionId.Shoot)} : Disparo PEM\n\n" +
                    $"• MANTENER {GameInput.GetBindingDisplayName(InputActionId.Shoot)} : Pulso cargado\n\n" +
                    $"• {GameInput.GetBindingDisplayName(InputActionId.Pause)} / ESC : Pausa";
            }
        }

        private void CacheSceneBrightnessLight()
        {
            cachedGlobalLight2D = null;
            cachedLightIntensityProp = null;
            baseGlobalLightIntensity = 0.72f;

            MonoBehaviour[] allScripts = FindObjectsByType<MonoBehaviour>();
            for (int i = 0; i < allScripts.Length; i++)
            {
                MonoBehaviour mb = allScripts[i];
                if (mb == null || mb.GetType().Name != "Light2D") continue;

                PropertyInfo ltProp = mb.GetType().GetProperty("lightType");
                PropertyInfo intProp = mb.GetType().GetProperty("intensity");
                if (ltProp == null || intProp == null) continue;

                object ltVal = ltProp.GetValue(mb);
                if (ltVal != null && ltVal.ToString().Contains("Global"))
                {
                    cachedGlobalLight2D = mb;
                    cachedLightIntensityProp = intProp;
                    object curInt = intProp.GetValue(mb);
                    if (curInt is float f) baseGlobalLightIntensity = Mathf.Max(0.2f, f);
                    break;
                }
            }
        }

        private void ApplyBrightnessLive()
        {
            if (cachedGlobalLight2D != null && cachedLightIntensityProp != null)
            {
                cachedLightIntensityProp.SetValue(cachedGlobalLight2D, baseGlobalLightIntensity * GameSettings.BrightnessMultiplier);
            }
        }

        private void ApplyHudScaleLive()
        {
            float scale = GameSettings.HudScaleFactor;
            Vector3 s = new Vector3(scale, scale, 1f);
            Transform hp = transform.Find("Player_Health_Panel");
            Transform ab = transform.Find("Player_Abilities_Panel");
            if (hp != null) hp.localScale = s;
            if (ab != null) ab.localScale = s;
            if (levelProgressContainer != null) levelProgressContainer.transform.localScale = s;
            else
            {
                Transform lp = transform.Find("Level_Progress_Panel");
                if (lp != null) lp.localScale = s;
            }
            if (bossHealthContainer != null) bossHealthContainer.transform.localScale = s;
        }

        private void EnsureFpsCounterUI()
        {
            if (fpsCounterPanel == null)
            {
                fpsCounterPanel = new GameObject("Live_FPS_Counter");
                fpsCounterPanel.transform.SetParent(transform, false);
                RectTransform rt = fpsCounterPanel.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.012f, 0.92f);
                rt.anchorMax = new Vector2(0.16f, 0.985f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                GameObject card = CreateGlassCard(fpsCounterPanel.transform, "FPS_Card", Vector2.zero, Vector2.one,
                    new Color(0.01f, 0.04f, 0.10f, 0.85f), new Color(0.25f, 0.96f, 1.0f, 0.80f));
                fpsCounterText = CreateDynamicText(card.transform, "FPS_Text", "60 FPS", 13,
                    new Vector2(0.06f, 0.05f), new Vector2(0.94f, 0.95f), new Color(0.45f, 1.0f, 0.78f, 1f));
            }
            fpsCounterPanel.SetActive(GameSettings.ShowFpsCounter);
        }

        private void UpdateFpsCounterLive()
        {
            if (fpsCounterPanel == null) return;
            bool shouldShow = GameSettings.ShowFpsCounter;
            if (fpsCounterPanel.activeSelf != shouldShow)
            {
                fpsCounterPanel.SetActive(shouldShow);
            }
            if (!shouldShow || fpsCounterText == null) return;

            fpsFrames++;
            fpsTimer += Time.unscaledDeltaTime;
            if (fpsTimer >= 0.4f)
            {
                float fps = fpsFrames / fpsTimer;
                float ms = (fps > 0.1f) ? (1000f / fps) : 0f;
                fpsCounterText.text = $"{Mathf.RoundToInt(fps)} FPS | {ms:F0} ms";
                fpsFrames = 0;
                fpsTimer = 0f;
            }
        }

        private void EnsureCheckpointToastUI()
        {
            if (checkpointToastPanel != null) return;

            // Contenedor estirado a pantalla completa para que el aviso discreto nunca colapse en texto vertical
            checkpointToastPanel = new GameObject("Checkpoint_Toast_Banner");
            checkpointToastPanel.transform.SetParent(transform, false);
            StretchFullScreen(checkpointToastPanel);

            GameObject card = CreateGlassCard(checkpointToastPanel.transform, "Checkpoint_Toast_Card",
                new Vector2(0.755f, 0.795f), new Vector2(0.985f, 0.855f),
                new Color(0.02f, 0.10f, 0.11f, 0.82f), new Color(0.28f, 0.96f, 0.68f, 0.85f));
            checkpointToastText = CreateDynamicText(card.transform, "Toast_Text", "", 13,
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.92f), new Color(0.62f, 1.0f, 0.84f, 0.96f), TextAnchor.MiddleCenter);
            checkpointToastText.resizeTextForBestFit = true;
            checkpointToastText.resizeTextMinSize = 18;
            checkpointToastText.resizeTextMaxSize = 30;
            checkpointToastText.verticalOverflow = VerticalWrapMode.Truncate;
            checkpointToastPanel.SetActive(false);
        }

        private IEnumerator ShowCheckpointToastRoutine(int index)
        {
            if (checkpointToastPanel == null) EnsureCheckpointToastUI();
            if (checkpointToastPanel == null) yield break;

            if (checkpointToastText != null)
            {
                string reward = (playerHealth != null && !string.IsNullOrEmpty(playerHealth.LastCheckpointRewardNote))
                    ? playerHealth.LastCheckpointRewardNote
                    : "Guardado";
                checkpointToastText.text = $"Checkpoint {index}/8  •  {reward}";
            }

            checkpointToastPanel.SetActive(true);
            checkpointToastPanel.transform.SetAsLastSibling();
            CanvasGroup cg = GetOrAddCanvasGroup(checkpointToastPanel);
            cg.blocksRaycasts = false;
            cg.interactable = false;

            float fadeIn = 0f;
            while (fadeIn < 0.22f)
            {
                fadeIn += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Clamp01(fadeIn / 0.22f) * 0.92f;
                yield return null;
            }
            cg.alpha = 0.92f;

            yield return new WaitForSecondsRealtime(1.65f);

            float fadeOut = 0f;
            while (fadeOut < 0.35f)
            {
                fadeOut += Time.unscaledDeltaTime;
                cg.alpha = (1f - Mathf.Clamp01(fadeOut / 0.35f)) * 0.92f;
                yield return null;
            }
            checkpointToastPanel.SetActive(false);
            checkpointToastCoroutine = null;
        }

        private InputField CreateGlassInputField(Transform parent, string name, string placeholderText, string initialText, int charLimit, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject root = CreateGlassCard(parent, name, anchorMin, anchorMax,
                new Color(0.01f, 0.04f, 0.10f, 0.96f), new Color(0.35f, 0.96f, 1.0f, 0.90f));

            Text textComp = CreateDynamicText(root.transform, "Text_Value", initialText ?? "", 16,
                new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.92f), Color.white, TextAnchor.MiddleLeft);
            textComp.supportRichText = false;
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;

            Text placeholderComp = CreateDynamicText(root.transform, "Placeholder_Text", placeholderText, 15,
                new Vector2(0.03f, 0.08f), new Vector2(0.97f, 0.92f), new Color(0.55f, 0.72f, 0.86f, 0.75f), TextAnchor.MiddleLeft);
            placeholderComp.fontStyle = FontStyle.Italic;

            InputField input = root.AddComponent<InputField>();
            input.targetGraphic = root.GetComponent<Image>();
            input.textComponent = textComp;
            input.placeholder = placeholderComp;
            input.characterLimit = charLimit;
            input.contentType = InputField.ContentType.Standard;
            input.lineType = InputField.LineType.SingleLine;
            input.selectionColor = new Color(0.25f, 0.92f, 1.0f, 0.35f);
            input.caretColor = new Color(0.35f, 1.0f, 0.85f, 1f);
            input.caretWidth = 2;
            input.customCaretColor = true;
            input.text = initialText ?? "";
            return input;
        }

        private static string BuildFullCreditsBody(string endingHeaderPrefix = null)
        {
            string header = string.IsNullOrEmpty(endingHeaderPrefix) ? "" : endingHeaderPrefix + "\n\n";
            return header +
                "UNIVERSIDAD TECNOLÓGICA DEL NORTE DE GUANAJUATO\n\n" +
                "DESARROLLADORES:\n" +
                "• Cynthia Janeth Granados Estrada\n" +
                "• Uriel Yáñez Aguayo\n" +
                "• Antigravity\n\n" +
                "MOTOR DE JUEGO Y TECNOLOGÍAS:\n" +
                "• Unity 6 (Unity Technologies) — Universal Render Pipeline (URP 2D) y Unity Input System\n\n" +
                "AUDIO Y CONTENIDO GENERADO CON IA:\n" +
                "• Efectos de combate sintetizados proceduralmente en formato WAV (PCM 16-bit / 44.1 kHz)\n" +
                "  mediante síntesis digital en C# con asistencia de IA (Antigravity):\n" +
                "  SFX_EMP_Pulse_Normal, SFX_EMP_Pulse_Charged, SFX_Charge_Ready, SFX_Katana_Slash,\n" +
                "  SFX_Dash_Slide, SFX_Player_Hurt, SFX_Enemy_Hit y SFX_Boss_Cast.\n" +
                "• Ilustración de fondo y logotipo de portada del Menú Principal generados con IA (Antigravity).\n\n" +
                "CRÉDITOS DE ASSETS Y RECURSOS DE TERCEROS:\n" +
                "• itch.io — https://itch.io/\n" +
                "• CraftPix (Música y SFX Cyberpunk Pack 1, fondos, íconos y sprites) — https://craftpix.net/\n" +
                "• GameArt2D (Sprites de personajes Ninja y robots) — http://gameart2d.com/\n" +
                "• Kenney (UI Pack: Sci-Fi 2.0 y Platformer Pack: Industrial — CC0 1.0) — https://kenney.nl/assets\n" +
                "• SunGraphica (Interfaz y elementos HUD 2D) — https://sungraphica.itch.io/\n" +
                "• Wenrexa (Sci-Fi Interface, barras de progreso e íconos) — https://wenrexa.itch.io/\n" +
                "• Binbun3D (Recursos gráficos 2D) — https://binbun3d.itch.io/\n" +
                "• Frostwindz (Efectos visuales de rayos y habilidades PEM) — https://frostwindz.itch.io/\n" +
                "• Unity Technologies — Audio de salto (SFX_Jump.wav) del proyecto de muestra 'Lost Crypt'\n" +
                "  (Licencia Unity Companion License).\n" +
                "• Tipografía: Segoe UI (Microsoft Corporation) y referencias tipográficas incluidas en paquetes.\n\n" +
                "DERECHOS DE AUTOR Y LICENCIAS:\n" +
                "Unity y los logotipos relacionados son marcas registradas de Unity Technologies.\n" +
                "Todos los assets, pistas musicales, efectos de sonido y tipografías pertenecen a sus respectivos\n" +
                "autores y se utilizan respetando sus licencias (CC0, Free Assets y Unity Companion License)\n" +
                "con fines estrictamente educativos y no comerciales.";
        }

        private void BuildCreditsModal(Transform menuRoot)
        {
            creditsSubPanel = CreateGlassCard(menuRoot, "Credits_GlassSubPanel",
                new Vector2(0.11f, 0.06f), new Vector2(0.89f, 0.94f),
                new Color(0.02f, 0.05f, 0.13f, 0.97f), new Color(0.30f, 0.92f, 1.0f, 0.96f));
            creditsCardRect = creditsSubPanel.GetComponent<RectTransform>();

            CreateDynamicText(creditsSubPanel.transform, "Cr_Title", "CRÉDITOS Y LICENCIAS", 23,
                new Vector2(0.05f, 0.89f), new Vector2(0.95f, 0.975f), new Color(0.25f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            // Área desplazable con rueda del mouse o arrastre para mostrar todos los créditos completos sin recortes
            GameObject scrollObj = new GameObject("Credits_ScrollRect");
            scrollObj.transform.SetParent(creditsSubPanel.transform, false);
            RectTransform rtScrollObj = scrollObj.AddComponent<RectTransform>();
            rtScrollObj.anchorMin = new Vector2(0.04f, 0.16f);
            rtScrollObj.anchorMax = new Vector2(0.96f, 0.875f);
            rtScrollObj.offsetMin = Vector2.zero;
            rtScrollObj.offsetMax = Vector2.zero;
            scrollObj.AddComponent<Image>().color = new Color(0.01f, 0.03f, 0.08f, 0.72f);
            scrollObj.AddComponent<RectMask2D>();

            GameObject contentObj = new GameObject("Credits_ScrollContent");
            contentObj.transform.SetParent(scrollObj.transform, false);
            RectTransform rtContent = contentObj.AddComponent<RectTransform>();
            rtContent.anchorMin = new Vector2(0f, 1f);
            rtContent.anchorMax = new Vector2(1f, 1f);
            rtContent.pivot = new Vector2(0.5f, 1f);
            rtContent.anchoredPosition = Vector2.zero;
            rtContent.sizeDelta = new Vector2(0f, 1280f);

            Text creditsBody = CreateDynamicText(contentObj.transform, "Cr_Body", BuildFullCreditsBody(), 14,
                new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.98f), new Color(0.94f, 0.98f, 1.0f, 1f), TextAnchor.UpperLeft);
            creditsBody.lineSpacing = 1.18f;

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.content = rtContent;
            sr.viewport = rtScrollObj;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 36f;

            CreateDynamicText(creditsSubPanel.transform, "Cr_Scroll_Hint", "↕ Usa la rueda del mouse o arrastra para leer los créditos completos", 12,
                new Vector2(0.05f, 0.112f), new Vector2(0.95f, 0.155f), new Color(0.58f, 0.88f, 0.98f, 0.85f));

            CreateGlassButton(creditsSubPanel.transform, "Btn_Cr_Close", "↩  VOLVER",
                new Vector2(0.34f, 0.025f), new Vector2(0.66f, 0.105f),
                new Color(0.38f, 0.10f, 0.18f, 0.94f), new Color(1.0f, 0.45f, 0.55f, 0.95f),
                () => CloseMainMenuSubPanel(creditsSubPanel, creditsCardRect));

            creditsSubPanel.SetActive(false);
        }

        private void OpenMainMenuSubPanel(GameObject subPanel, RectTransform cardRect)
        {
            if (mainMenuModalBackdrop != null)
            {
                mainMenuModalBackdrop.SetActive(true);
                GetOrAddCanvasGroup(mainMenuModalBackdrop).alpha = 1f;
            }
            if (characterSelectSubPanel != null && characterSelectSubPanel != subPanel) characterSelectSubPanel.SetActive(false);
            if (levelSelectSubPanel != null && levelSelectSubPanel != subPanel) levelSelectSubPanel.SetActive(false);
            if (savedGamesSubPanel != null && savedGamesSubPanel != subPanel) savedGamesSubPanel.SetActive(false);
            if (deleteConfirmModal != null) deleteConfirmModal.SetActive(false);
            if (unifiedSettingsOverlay != null && unifiedSettingsOverlay != subPanel) unifiedSettingsOverlay.SetActive(false);
            if (creditsSubPanel != null && creditsSubPanel != subPanel) creditsSubPanel.SetActive(false);

            if (subPanel == characterSelectSubPanel && nicknameInputField != null)
            {
                if (nicknameValidationText != null) nicknameValidationText.text = "";
                if (string.IsNullOrEmpty(nicknameInputField.text))
                {
                    nicknameInputField.text = PlayerPrefs.GetString("CDE_PlayerNickname", "");
                }
                nicknameInputField.Select();
                nicknameInputField.ActivateInputField();
            }

            AnimatePanelShow(subPanel, cardRect, new Vector2(0f, -55f), 0.34f);
        }

        private void CloseMainMenuSubPanel(GameObject subPanel, RectTransform cardRect)
        {
            if (deleteConfirmModal != null) deleteConfirmModal.SetActive(false);
            if (mainMenuModalBackdrop != null) mainMenuModalBackdrop.SetActive(false);
            AnimatePanelHide(subPanel, cardRect, new Vector2(0f, -55f), 0.22f);
        }

        private void EnsureStatePanelsUI()
        {
            // 1. MENÚ DE PAUSA GLASSMORPHISM
            pausePanel = new GameObject("Pause_State_Overlay");
            pausePanel.transform.SetParent(transform, false);
            StretchFullScreen(pausePanel);
            Image pauseDim = pausePanel.AddComponent<Image>();
            pauseDim.color = new Color(0.01f, 0.02f, 0.06f, 0.88f);

            GameObject pauseCard = CreateGlassCard(pausePanel.transform, "Pause_Glass_Card",
                new Vector2(0.24f, 0.12f), new Vector2(0.76f, 0.88f),
                new Color(0.02f, 0.05f, 0.13f, 0.95f), new Color(0.28f, 0.95f, 1.0f, 0.95f));
            pauseCardRect = pauseCard.GetComponent<RectTransform>();

            CreateDynamicText(pauseCard.transform, "Pause_Title", "⏸  PAUSA", 27,
                new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.95f), new Color(0.25f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            CreateDynamicText(pauseCard.transform, "Pause_Subtitle", "MENÚ DE JUEGO", 15,
                new Vector2(0.06f, 0.76f), new Vector2(0.94f, 0.83f), new Color(0.78f, 0.94f, 1.0f, 0.95f));

            CreateGlassButton(pauseCard.transform, "Btn_Resume", "▶  REANUDAR",
                new Vector2(0.12f, 0.62f), new Vector2(0.88f, 0.73f),
                new Color(0.06f, 0.48f, 0.56f, 0.94f), new Color(0.25f, 0.98f, 1.0f, 1f),
                () => GameEvents.TriggerLevelStateChanged(LevelState.Playing));

            CreateGlassButton(pauseCard.transform, "Btn_Config_Pause", "⚙  CONFIGURACIÓN",
                new Vector2(0.12f, 0.48f), new Vector2(0.88f, 0.59f),
                new Color(0.10f, 0.28f, 0.48f, 0.94f), new Color(0.35f, 0.92f, 1.0f, 1f),
                () => OpenUnifiedSettingsModal(fromMainMenu: false));

            CreateGlassButton(pauseCard.transform, "Btn_SaveAndExit_Pause", "💾  GUARDAR Y SALIR",
                new Vector2(0.12f, 0.34f), new Vector2(0.88f, 0.45f),
                new Color(0.06f, 0.46f, 0.36f, 0.95f), new Color(0.25f, 1.0f, 0.72f, 1f),
                () =>
                {
                    isSavingFromVictory = false;
                    isSavingFromEnding = false;
                    RefreshSavedGamesSubPanel();
                    if (pauseSaveSlotsPanel != null)
                    {
                        pauseSaveSlotsPanel.transform.SetAsLastSibling();
                        AnimatePanelShow(pauseSaveSlotsPanel, pauseSaveSlotsCardRect, new Vector2(0f, -45f), 0.30f);
                    }
                });

            CreateGlassButton(pauseCard.transform, "Btn_Restart_Pause", "↻  REINICIAR NIVEL",
                new Vector2(0.12f, 0.20f), new Vector2(0.88f, 0.31f),
                new Color(0.12f, 0.26f, 0.46f, 0.92f), new Color(0.45f, 0.82f, 1.0f, 0.95f),
                () => LoadSceneWithAnimatedTransition(SceneManager.GetActiveScene().name));

            CreateGlassButton(pauseCard.transform, "Btn_MainMenu_Pause", "🏠  SALIR SIN GUARDAR",
                new Vector2(0.12f, 0.06f), new Vector2(0.88f, 0.17f),
                new Color(0.42f, 0.10f, 0.18f, 0.92f), new Color(1.0f, 0.42f, 0.52f, 0.95f),
                () => LoadSceneWithAnimatedTransition("MainMenu"));

            pausePanel.SetActive(false);

            // Sub-panel independiente de 4 ranuras para Guardar Partida (compartido por Pausa y Victoria)
            pauseSaveSlotsPanel = new GameObject("SaveSlots_Modal_Overlay");
            pauseSaveSlotsPanel.transform.SetParent(transform, false);
            StretchFullScreen(pauseSaveSlotsPanel);
            Image ssDim = pauseSaveSlotsPanel.AddComponent<Image>();
            ssDim.color = new Color(0.01f, 0.02f, 0.06f, 0.88f);

            GameObject ssCard = CreateGlassCard(pauseSaveSlotsPanel.transform, "Pause_SaveSlots_SubModal",
                new Vector2(0.10f, 0.08f), new Vector2(0.90f, 0.92f),
                new Color(0.02f, 0.05f, 0.13f, 0.98f), new Color(0.25f, 1.0f, 0.72f, 0.98f));
            pauseSaveSlotsCardRect = ssCard.GetComponent<RectTransform>();

            CreateDynamicText(ssCard.transform, "PSS_Title", "💾 GUARDAR PARTIDA", 22,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), new Color(0.30f, 1.0f, 0.75f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            CreateDynamicText(ssCard.transform, "PSS_Subtitle",
                "Selecciona una ranura para guardar tu progreso desde el último checkpoint.", 14,
                new Vector2(0.05f, 0.81f), new Vector2(0.95f, 0.88f), new Color(0.85f, 0.96f, 1.0f, 0.94f));

            for (int i = 0; i < 4; i++)
            {
                int slotIdx = i;
                float yMax = 0.79f - i * 0.155f;
                float yMin = yMax - 0.140f;

                GameObject row = CreateGlassCard(ssCard.transform, $"PauseSave_Row_{i}",
                    new Vector2(0.04f, yMin), new Vector2(0.96f, yMax),
                    new Color(0.03f, 0.08f, 0.17f, 0.94f), new Color(0.25f, 0.92f, 1.0f, 0.78f));

                GameObject pBadge = CreateGlassCard(row.transform, "Slot_Badge",
                    new Vector2(0.015f, 0.14f), new Vector2(0.068f, 0.86f),
                    new Color(0.06f, 0.22f, 0.34f, 0.96f), new Color(0.25f, 1.0f, 0.75f, 0.95f));
                CreateDynamicText(pBadge.transform, "Badge_Num", $"0{i + 1}", 17,
                    Vector2.zero, Vector2.one, new Color(0.35f, 1.0f, 0.82f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

                GameObject pPortFrame = CreateGlassCard(row.transform, "Slot_Portrait_Frame",
                    new Vector2(0.08f, 0.12f), new Vector2(0.155f, 0.88f),
                    new Color(0.02f, 0.05f, 0.11f, 0.95f), new Color(0.25f, 0.88f, 1.0f, 0.75f));
                GameObject pPortImgObj = new GameObject("Portrait_Img");
                pPortImgObj.transform.SetParent(pPortFrame.transform, false);
                RectTransform pRtPort = pPortImgObj.AddComponent<RectTransform>();
                pRtPort.anchorMin = new Vector2(0.1f, 0.1f);
                pRtPort.anchorMax = new Vector2(0.9f, 0.9f);
                pRtPort.offsetMin = Vector2.zero;
                pRtPort.offsetMax = Vector2.zero;
                pauseSaveSlotPortraits[i] = pPortImgObj.AddComponent<Image>();
                pauseSaveSlotPortraits[i].preserveAspect = true;
                pauseSaveSlotPortraits[i].raycastTarget = false;

                pauseSaveSlotTitleTexts[i] = CreateDynamicText(row.transform, "Slot_Title", FormatSaveSlotTitle(i), 16,
                    new Vector2(0.17f, 0.52f), new Vector2(0.69f, 0.92f), new Color(0.35f, 1.0f, 0.85f, 1f), TextAnchor.MiddleLeft);

                pauseSaveSlotMetaTexts[i] = CreateDynamicText(row.transform, "Slot_Meta", FormatSaveSlotMeta(i), 14,
                    new Vector2(0.17f, 0.10f), new Vector2(0.69f, 0.52f), new Color(0.88f, 0.95f, 1.0f, 0.94f), TextAnchor.MiddleLeft);

                CreateGlassButton(row.transform, $"Btn_Save_Slot_{i}", "💾 GUARDAR Y SALIR",
                    new Vector2(0.71f, 0.15f), new Vector2(0.985f, 0.85f),
                    new Color(0.06f, 0.46f, 0.38f, 0.95f), new Color(0.25f, 1.0f, 0.72f, 1f),
                    () => SaveCurrentGameToSlotAndExit(slotIdx));
            }

            CreateGlassButton(ssCard.transform, "Btn_PSS_Cancel", "↩  VOLVER",
                new Vector2(0.34f, 0.04f), new Vector2(0.66f, 0.14f),
                new Color(0.38f, 0.10f, 0.18f, 0.94f), new Color(1.0f, 0.45f, 0.55f, 0.95f),
                () =>
                {
                    isSavingFromVictory = false;
                    AnimatePanelHide(pauseSaveSlotsPanel, pauseSaveSlotsCardRect, new Vector2(0f, -45f), 0.22f);
                });

            pauseSaveSlotsPanel.SetActive(false);

            // 2. PANEL DE VICTORIA / NIVEL COMPLETADO GLASSMORPHISM (SOLO 2 OPCIONES)
            victoryPanel = new GameObject("Victory_State_Overlay");
            victoryPanel.transform.SetParent(transform, false);
            StretchFullScreen(victoryPanel);
            Image vicDim = victoryPanel.AddComponent<Image>();
            vicDim.color = new Color(0.01f, 0.03f, 0.07f, 0.88f);

            GameObject vicCard = CreateGlassCard(victoryPanel.transform, "Victory_Glass_Card",
                new Vector2(0.20f, 0.16f), new Vector2(0.80f, 0.84f),
                new Color(0.02f, 0.07f, 0.14f, 0.95f), new Color(0.25f, 1.0f, 0.72f, 0.96f));
            victoryCardRect = vicCard.GetComponent<RectTransform>();

            CreateDynamicText(vicCard.transform, "Victory_Badge", "★ NIVEL COMPLETADO ★", 16,
                new Vector2(0.06f, 0.85f), new Vector2(0.94f, 0.93f), new Color(0.45f, 1.0f, 0.82f, 1f));

            CreateDynamicText(vicCard.transform, "Victory_Title", "¡SECTOR LIBERADO!", 30,
                new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.85f), new Color(0.25f, 1.0f, 0.68f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            CreateDynamicText(vicCard.transform, "Victory_Desc",
                "Sobrecargaste la pulsera S.I.A. y el docente fue puesto a salvo.", 16,
                new Vector2(0.08f, 0.46f), new Vector2(0.92f, 0.68f), new Color(0.92f, 0.98f, 1.0f, 1f));

            victoryNotificationText = CreateDynamicText(vicCard.transform, "Victory_AutoSave_Notify", "", 15,
                new Vector2(0.08f, 0.31f), new Vector2(0.92f, 0.44f), new Color(0.25f, 1.0f, 0.82f, 1f));

            CreateGlassButton(vicCard.transform, "Btn_Next_Level", "▶  SIGUIENTE NIVEL",
                new Vector2(0.08f, 0.11f), new Vector2(0.48f, 0.27f),
                new Color(0.06f, 0.50f, 0.42f, 0.95f), new Color(0.25f, 1.0f, 0.75f, 1f),
                () => StartCoroutine(AutoSaveAndAdvanceNextLevelRoutine()));

            CreateGlassButton(vicCard.transform, "Btn_MainMenu_Victory", "🏠  MENÚ PRINCIPAL",
                new Vector2(0.52f, 0.11f), new Vector2(0.92f, 0.27f),
                new Color(0.12f, 0.26f, 0.46f, 0.94f), new Color(0.45f, 0.85f, 1.0f, 0.95f),
                () =>
                {
                    if (isTransitioningFromVictory) return;
                    if (victoryExitConfirmPanel != null)
                    {
                        victoryExitConfirmPanel.transform.SetAsLastSibling();
                        AnimatePanelShow(victoryExitConfirmPanel, victoryExitConfirmCardRect, new Vector2(0f, -35f), 0.24f);
                    }
                });

            // Modal de confirmación desde Victoria para salir guardando o sin guardar
            victoryExitConfirmPanel = new GameObject("Victory_ExitConfirm_Backdrop");
            victoryExitConfirmPanel.transform.SetParent(victoryPanel.transform, false);
            StretchFullScreen(victoryExitConfirmPanel);
            Image vxDim = victoryExitConfirmPanel.AddComponent<Image>();
            vxDim.color = new Color(0.01f, 0.02f, 0.05f, 0.86f);

            GameObject vxCard = CreateGlassCard(victoryExitConfirmPanel.transform, "Victory_ExitConfirm_GlassCard",
                new Vector2(0.18f, 0.22f), new Vector2(0.82f, 0.78f),
                new Color(0.02f, 0.06f, 0.14f, 0.98f), new Color(0.35f, 0.95f, 1.0f, 0.98f));
            victoryExitConfirmCardRect = vxCard.GetComponent<RectTransform>();

            CreateDynamicText(vxCard.transform, "VX_Title", "⚠  SALIR AL MENÚ PRINCIPAL", 21,
                new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.92f), new Color(0.35f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            CreateDynamicText(vxCard.transform, "VX_Body",
                "¿Deseas guardar tu progreso antes de salir al Menú Principal?", 16,
                new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.70f), new Color(0.94f, 0.98f, 1.0f, 1f));

            CreateGlassButton(vxCard.transform, "Btn_VX_SaveExit", "💾  SALIR Y GUARDAR",
                new Vector2(0.05f, 0.10f), new Vector2(0.37f, 0.28f),
                new Color(0.06f, 0.48f, 0.38f, 0.96f), new Color(0.25f, 1.0f, 0.72f, 1f),
                () =>
                {
                    victoryExitConfirmPanel.SetActive(false);
                    isSavingFromVictory = true;
                    RefreshSavedGamesSubPanel();
                    if (pauseSaveSlotsPanel != null)
                    {
                        pauseSaveSlotsPanel.transform.SetAsLastSibling();
                        AnimatePanelShow(pauseSaveSlotsPanel, pauseSaveSlotsCardRect, new Vector2(0f, -45f), 0.28f);
                    }
                });

            CreateGlassButton(vxCard.transform, "Btn_VX_NoSaveExit", "🚪  SALIR SIN GUARDAR",
                new Vector2(0.39f, 0.10f), new Vector2(0.71f, 0.28f),
                new Color(0.45f, 0.12f, 0.20f, 0.96f), new Color(1.0f, 0.42f, 0.52f, 1f),
                () => LoadSceneWithAnimatedTransition("MainMenu"));

            CreateGlassButton(vxCard.transform, "Btn_VX_Cancel", "↩  CANCELAR",
                new Vector2(0.73f, 0.10f), new Vector2(0.95f, 0.28f),
                new Color(0.12f, 0.26f, 0.46f, 0.94f), new Color(0.45f, 0.85f, 1.0f, 1f),
                () => AnimatePanelHide(victoryExitConfirmPanel, victoryExitConfirmCardRect, new Vector2(0f, -35f), 0.18f));

            victoryExitConfirmPanel.SetActive(false);
            victoryPanel.SetActive(false);

            RefreshSettingsLabels();
            RefreshAudioLabels();

            // 3. PANEL DE GAME OVER GLASSMORPHISM
            gameOverPanel = new GameObject("GameOver_State_Overlay");
            gameOverPanel.transform.SetParent(transform, false);
            StretchFullScreen(gameOverPanel);
            Image goDim = gameOverPanel.AddComponent<Image>();
            goDim.color = new Color(0.05f, 0.01f, 0.02f, 0.88f);

            GameObject goCard = CreateGlassCard(gameOverPanel.transform, "GameOver_Glass_Card",
                new Vector2(0.22f, 0.18f), new Vector2(0.78f, 0.82f),
                new Color(0.08f, 0.02f, 0.05f, 0.95f), new Color(1.0f, 0.28f, 0.40f, 0.96f));
            gameOverCardRect = goCard.GetComponent<RectTransform>();

            CreateDynamicText(goCard.transform, "GO_Title", "⚠  GAME OVER", 28,
                new Vector2(0.05f, 0.73f), new Vector2(0.95f, 0.90f), new Color(1.0f, 0.32f, 0.42f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            CreateDynamicText(goCard.transform, "GO_Desc",
                "Agotaste todas tus vidas. Reinicia el nivel para intentarlo de nuevo.", 17,
                new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.70f), new Color(0.96f, 0.92f, 0.94f, 1f));

            CreateGlassButton(goCard.transform, "Btn_Retry_Level", "↻  REINICIAR NIVEL",
                new Vector2(0.08f, 0.14f), new Vector2(0.54f, 0.30f),
                new Color(0.50f, 0.12f, 0.20f, 0.95f), new Color(1.0f, 0.42f, 0.52f, 1f),
                () => LoadSceneWithAnimatedTransition(SceneManager.GetActiveScene().name));

            CreateGlassButton(goCard.transform, "Btn_MainMenu_GO", "🏠  MENÚ PRINCIPAL",
                new Vector2(0.57f, 0.14f), new Vector2(0.92f, 0.30f),
                new Color(0.14f, 0.22f, 0.38f, 0.94f), new Color(0.50f, 0.82f, 1.0f, 0.95f),
                () => LoadSceneWithAnimatedTransition("MainMenu"));

            gameOverPanel.SetActive(false);
        }

        private void OnVolumeSliderChanged(float value)
        {
            OnMasterVolumeSliderChanged(value);
        }

        private void OnMasterVolumeSliderChanged(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            AudioListener.volume = masterVolume;
            PlayerPrefs.SetFloat("CDE_MasterVolume", masterVolume);
            RefreshAudioLabels();
        }

        private void OnMusicVolumeSliderChanged(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            if (bgmSource != null) bgmSource.volume = musicVolume * 0.42f;
            PlayerPrefs.SetFloat("CDE_MusicVolume", musicVolume);
            RefreshAudioLabels();
        }

        private void OnSfxVolumeSliderChanged(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            PlayerPrefs.SetFloat("CDE_SfxVolume", sfxVolume);
            RefreshAudioLabels();
        }

        private void OnEnvVolumeSliderChanged(float value)
        {
            envVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("CDE_EnvVolume", envVolume);
            RefreshAudioLabels();
        }

        private void RefreshAudioLabels()
        {
            int mPct = Mathf.RoundToInt(masterVolume * 100f);
            int musPct = Mathf.RoundToInt(musicVolume * 100f);
            int sfxPct = Mathf.RoundToInt(sfxVolume * 100f);
            int envPct = Mathf.RoundToInt(envVolume * 100f);

            if (settingsVolumeLabel != null) settingsVolumeLabel.text = $"🔊 VOLUMEN GENERAL: {mPct}%";
            if (settingsMusicLabel != null) settingsMusicLabel.text = $"🎵 MÚSICA BGM: {musPct}%";
            if (settingsSfxLabel != null) settingsSfxLabel.text = $"⚔ EFECTOS SFX: {sfxPct}%";
            if (settingsEnvLabel != null) settingsEnvLabel.text = $"🏃 ENTORNO Y SALTO: {envPct}%";

            if (settingsVolumeSlider != null && Mathf.Abs(settingsVolumeSlider.value - masterVolume) > 0.005f)
                settingsVolumeSlider.SetValueWithoutNotify(masterVolume);
            if (settingsMusicSlider != null && Mathf.Abs(settingsMusicSlider.value - musicVolume) > 0.005f)
                settingsMusicSlider.SetValueWithoutNotify(musicVolume);
            if (settingsSfxSlider != null && Mathf.Abs(settingsSfxSlider.value - sfxVolume) > 0.005f)
                settingsSfxSlider.SetValueWithoutNotify(sfxVolume);
            if (settingsEnvSlider != null && Mathf.Abs(settingsEnvSlider.value - envVolume) > 0.005f)
                settingsEnvSlider.SetValueWithoutNotify(envVolume);
        }

        private IEnumerator AutoSaveAndAdvanceNextLevelRoutine()
        {
            int activeSlot = PlayerPrefs.GetInt("CDE_ActiveSaveSlot", -1);
            if (activeSlot < 0 || activeSlot >= 4)
                activeSlot = PlayerPrefs.GetInt("CDE_PendingLoadSlot", -1);
            if (activeSlot < 0 || activeSlot >= 4)
            {
                activeSlot = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (PlayerPrefs.GetInt($"CDE_SaveSlot_{i}_HasData", 0) == 0)
                    {
                        activeSlot = i;
                        break;
                    }
                }
            }

            SaveNextLevelProgressToSlot(activeSlot);
            PlayerPrefs.SetInt("CDE_PendingLoadSlot", -1);
            PlayerPrefs.SetInt("CDE_ActiveSaveSlot", activeSlot);
            PlayerPrefs.Save();

            if (victoryExitConfirmPanel != null)
                victoryExitConfirmPanel.SetActive(false);

            if (victoryNotificationText != null)
            {
                victoryNotificationText.text = $"💾 Partida guardada automáticamente en Ranura {activeSlot + 1}. Cargando siguiente nivel...";
                victoryNotificationText.color = new Color(0.25f, 1.0f, 0.65f, 1.0f);
            }

            yield return new WaitForSecondsRealtime(1.1f);

            string targetNext = string.IsNullOrEmpty(nextSceneName) ? "SampleScene" : nextSceneName;
            LoadSceneWithAnimatedTransition(targetNext);
        }

        private void EnsureTutorialOverlayUI()
        {
            if (tutorialOverlayPanel != null) return;

            tutorialOverlayPanel = new GameObject("Tutorial_Interactive_Spotlight_Overlay");
            tutorialOverlayPanel.transform.SetParent(transform, false);
            StretchFullScreen(tutorialOverlayPanel);

            // Oscurece el nivel al 80% cuando aparece la indicación Glassmorphism del tutorial
            Image dimImg = tutorialOverlayPanel.AddComponent<Image>();
            dimImg.color = new Color(0.01f, 0.02f, 0.06f, 0.80f);

            GameObject card = CreateGlassCard(tutorialOverlayPanel.transform, "Tutorial_Instruction_GlassCard",
                new Vector2(0.16f, 0.54f), new Vector2(0.84f, 0.89f),
                new Color(0.02f, 0.05f, 0.13f, 0.95f), new Color(0.25f, 0.96f, 1.0f, 0.96f));
            tutorialCardRect = card.GetComponent<RectTransform>();

            tutorialStepTitleText = CreateDynamicText(card.transform, "Tut_Title", "ENTRENAMIENTO DE CAMPO", 23,
                new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.93f), new Color(0.25f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            tutorialStepBodyText = CreateDynamicText(card.transform, "Tut_Body", "", 17,
                new Vector2(0.06f, 0.28f), new Vector2(0.94f, 0.70f), new Color(0.94f, 0.98f, 1.0f, 1f));

            tutorialStepPromptText = CreateDynamicText(card.transform, "Tut_KeyPrompt", "", 18,
                new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.25f), new Color(1.0f, 0.88f, 0.28f, 1f));

            tutorialOverlayPanel.SetActive(false);
        }

        private void EnsureCutsceneDialogueUI()
        {
            if (cutsceneDialoguePanel != null) return;

            // Contenedor con fondo oscurecido parcial para que el cuadro Glassmorphism de agradecimiento se lea con total claridad
            cutsceneDialoguePanel = new GameObject("Cutscene_Dialogue_Overlay");
            cutsceneDialoguePanel.transform.SetParent(transform, false);
            StretchFullScreen(cutsceneDialoguePanel);

            Image backdrop = cutsceneDialoguePanel.AddComponent<Image>();
            backdrop.color = new Color(0.01f, 0.02f, 0.06f, 0.65f);
            backdrop.raycastTarget = false;

            GameObject card = CreateGlassCard(cutsceneDialoguePanel.transform, "Cutscene_Dialogue_GlassCard",
                new Vector2(0.16f, 0.06f), new Vector2(0.84f, 0.27f),
                new Color(0.02f, 0.05f, 0.13f, 0.95f), new Color(0.25f, 1.0f, 0.78f, 0.96f));
            cutsceneDialogueCardRect = card.GetComponent<RectTransform>();

            cutsceneSpeakerText = CreateDynamicText(card.transform, "Speaker_Name", "DOCENTE LIBERADO", 20,
                new Vector2(0.04f, 0.68f), new Vector2(0.96f, 0.93f), new Color(0.30f, 1.0f, 0.75f, 1f), TextAnchor.MiddleLeft, useHeaderFont: true);

            cutsceneMessageText = CreateDynamicText(card.transform, "Speaker_Message", "", 17,
                new Vector2(0.04f, 0.10f), new Vector2(0.96f, 0.66f), Color.white, TextAnchor.MiddleLeft);

            cutsceneDialoguePanel.SetActive(false);
        }

        private void HandleCutsceneDialogueRequested(string speaker, string message, float duration)
        {
            EnsureCutsceneDialogueUI();
            if (activeDialogueCoroutine != null) StopCoroutine(activeDialogueCoroutine);

            if (string.IsNullOrEmpty(message) || duration <= 0f)
            {
                if (cutsceneDialoguePanel != null && cutsceneDialoguePanel.activeSelf)
                {
                    AnimatePanelHide(cutsceneDialoguePanel, cutsceneDialogueCardRect, new Vector2(0f, -65f), 0.20f);
                }
                return;
            }

            activeDialogueCoroutine = StartCoroutine(ShowCutsceneDialogueRoutine(speaker, message, duration));
        }

        private IEnumerator ShowCutsceneDialogueRoutine(string speaker, string message, float duration)
        {
            if (cutsceneSpeakerText != null) cutsceneSpeakerText.text = "💬  " + speaker;
            if (cutsceneMessageText != null) cutsceneMessageText.text = message;

            // Deslizar suavemente desde el borde inferior
            AnimatePanelShow(cutsceneDialoguePanel, cutsceneDialogueCardRect, new Vector2(0f, -65f), 0.35f);
            yield return new WaitForSecondsRealtime(Mathf.Max(0.5f, duration - 0.30f));
            AnimatePanelHide(cutsceneDialoguePanel, cutsceneDialogueCardRect, new Vector2(0f, -65f), 0.28f);
        }

        // =================================================================================================
        // CINEMÁTICA INTERACTIVA POR ESCENAS ANIMADAS (IntroCutscene.unity — 5 ESCENAS CON AVANCE [ESPACIO])
        // =================================================================================================

        private IEnumerator RunAnimatedMovieCutsceneDirector()
        {
            int selectedChar = Mathf.Clamp(PlayerPrefs.GetInt("CDE_SelectedCharacterIndex", 0), 0, CharacterNames.Length - 1);
            string nick = GameSettings.PlayerNickname;

            GameObject cinemaRoot = new GameObject("IntroCutscene_Movie_UI");
            cinemaRoot.transform.SetParent(transform, false);
            StretchFullScreen(cinemaRoot);

            // Barra superior cinematográfica
            GameObject topLetterbox = CreateGlassCard(cinemaRoot.transform, "Cinema_Top_Bar",
                new Vector2(0.02f, 0.90f), new Vector2(0.98f, 0.985f),
                new Color(0.01f, 0.03f, 0.08f, 0.94f), new Color(0.25f, 0.92f, 1.0f, 0.85f));

            Text topSceneCounter = CreateDynamicText(topLetterbox.transform, "Rec_Badge",
                $"🔴 ESCENA 1 DE 5  —  {nick} ({CharacterNames[selectedChar]})", 15,
                new Vector2(0.03f, 0.08f), new Vector2(0.72f, 0.92f), new Color(0.45f, 0.96f, 1.0f, 1f), TextAnchor.MiddleLeft);

            CreateGlassButton(topLetterbox.transform, "Btn_Skip_Movie", "⏭  OMITIR CINEMÁTICA",
                new Vector2(0.75f, 0.14f), new Vector2(0.98f, 0.86f),
                new Color(0.32f, 0.10f, 0.18f, 0.92f), new Color(1.0f, 0.45f, 0.55f, 0.95f),
                () => LoadSceneWithAnimatedTransition("TutorialScene"));

            // Panel inferior Glassmorphism para narrativa e indicador de avance con ESPACIO
            GameObject bottomCard = CreateGlassCard(cinemaRoot.transform, "Cinema_Bottom_Narrative_Card",
                new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.27f),
                new Color(0.01f, 0.04f, 0.11f, 0.95f), new Color(0.25f, 0.95f, 1.0f, 0.92f));
            RectTransform bottomCardRect = bottomCard.GetComponent<RectTransform>();

            Text subtitleSpeaker = CreateDynamicText(bottomCard.transform, "Movie_Subtitle_Speaker", "", 18,
                new Vector2(0.03f, 0.72f), new Vector2(0.68f, 0.95f), new Color(0.30f, 0.96f, 1.0f, 1f), TextAnchor.MiddleLeft, useHeaderFont: true);

            Text subtitlePrompt = CreateDynamicText(bottomCard.transform, "Movie_Space_Indicator",
                "▶ PULSA ESPACIO PARA AVANZAR A LA SIGUIENTE ESCENA ◀", 15,
                new Vector2(0.03f, 0.05f), new Vector2(0.68f, 0.24f), new Color(1.0f, 0.88f, 0.30f, 1f), TextAnchor.MiddleLeft);

            Text subtitleText = CreateDynamicText(bottomCard.transform, "Movie_Subtitle_Text", "", 16,
                new Vector2(0.03f, 0.25f), new Vector2(0.70f, 0.71f), new Color(0.95f, 0.98f, 1.0f, 1f), TextAnchor.MiddleLeft);

            CreateGlassButton(bottomCard.transform, "Btn_Next_Cutscene_Step", "SIGUIENTE ▶",
                new Vector2(0.72f, 0.18f), new Vector2(0.97f, 0.82f),
                new Color(0.06f, 0.48f, 0.62f, 0.95f), new Color(0.25f, 1.0f, 0.80f, 1f),
                () => { cutsceneAdvanceRequested = true; });

            // Flash de pantalla completa para impactos de la película
            GameObject flashObj = new GameObject("Cinema_Screen_Flash");
            flashObj.transform.SetParent(cinemaRoot.transform, false);
            StretchFullScreen(flashObj);
            Image flashImg = flashObj.AddComponent<Image>();
            flashImg.color = Color.clear;
            flashImg.raycastTarget = false;

            // Referencias a los actores del escenario 2D
            Camera cam = Camera.main;
            GameObject hero = GameObject.Find("Cutscene_Hero_Ninja");
            GameObject prof1 = GameObject.Find("Cutscene_Professor");
            GameObject prof2 = GameObject.Find("Cutscene_Professor_2");
            GameObject wraith = GameObject.Find("Cutscene_SIA_Wraith");
            GameObject robot1 = GameObject.Find("Cutscene_Robot_1");

            Animator heroAnim = hero != null ? hero.GetComponent<Animator>() : null;
            SpriteRenderer heroSr = hero != null ? hero.GetComponent<SpriteRenderer>() : null;

            // Aplicar el personaje seleccionado por el usuario al protagonista de la cinemática
            if (playableCharacterControllers != null && selectedChar < playableCharacterControllers.Length && playableCharacterControllers[selectedChar] != null && heroAnim != null)
            {
                heroAnim.runtimeAnimatorController = playableCharacterControllers[selectedChar];
                heroAnim.SetBool("IsGrounded", true);
                heroAnim.SetFloat("Speed", 0f);
            }
            if (playableCharacterIdleSprites != null && selectedChar < playableCharacterIdleSprites.Length && playableCharacterIdleSprites[selectedChar] != null && heroSr != null)
            {
                heroSr.sprite = playableCharacterIdleSprites[selectedChar];
            }

            SpriteRenderer prof1Sr = prof1 != null ? prof1.GetComponent<SpriteRenderer>() : null;
            SpriteRenderer prof2Sr = prof2 != null ? prof2.GetComponent<SpriteRenderer>() : null;
            SpriteRenderer wraithSr = wraith != null ? wraith.GetComponent<SpriteRenderer>() : null;
            Sprite wraithFormSprite = wraithSr != null ? wraithSr.sprite : null;

            // Posiciones iniciales de la película
            if (hero != null) hero.transform.position = new Vector3(-11.5f, -2.05f, 0f);
            if (prof1 != null)
            {
                prof1.transform.position = new Vector3(-2.5f, -2.05f, 0f);
                prof1.transform.localScale = new Vector3(1.22f, 1.22f, 1f);
            }
            if (prof2 != null)
            {
                prof2.transform.position = new Vector3(3.8f, -2.05f, 0f);
                prof2.transform.localScale = new Vector3(-1.22f, 1.22f, 1f);
            }
            if (wraith != null) wraith.SetActive(false);
            if (robot1 != null) robot1.SetActive(false);

            // =========================================================================================
            // ESCENA 1 DE 5: LOS PROFESORES Y LAS PULSERAS INTELIGENTES S.I.A.
            // =========================================================================================
            cutsceneAdvanceRequested = false;
            yield return null;
            topSceneCounter.text = $"🔴 ESCENA 1 DE 5  —  {CharacterNames[selectedChar]}";
            subtitleSpeaker.text = "ESCENA 1 DE 5 — SISTEMA DE INTELIGENCIA ACADÉMICA";
            subtitleText.text =
                "En el Laboratorio de Cómputo de la UTNG, los profesores operaban los servidores centrales con las pulseras inteligentes S.I.A.\n" +
                "Estas pulseras sincronizaban la energía, las redes y la seguridad automatizada del campus.";
            AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -45f), 0.32f);

            float t = 0f;
            while (!(t > 0.35f && CheckCutsceneNextSceneInput()))
            {
                if (GameInput.GetPauseDown()) { LoadSceneWithAnimatedTransition("TutorialScene"); yield break; }
                t += Time.deltaTime;
                float camP = Mathf.Clamp01(t / 3.5f);

                if (cam != null)
                {
                    cam.transform.position = new Vector3(Mathf.Lerp(-2.2f, 0.8f, Mathf.SmoothStep(0f, 1f, camP)), 0.15f, -10f);
                    cam.orthographicSize = Mathf.Lerp(5.3f, 4.7f, camP);
                }

                if (prof1 != null)
                {
                    if (t < 2.6f)
                    {
                        prof1.transform.position += Vector3.right * (1.15f * Time.deltaTime);
                        AnimateSpriteFrames(prof1Sr, humanRunFrames, t, 10f);
                    }
                    else
                    {
                        AnimateSpriteFrames(prof1Sr, humanIdleFrames, t, 6f);
                    }
                }
                if (prof2 != null)
                {
                    AnimateSpriteFrames(prof2Sr, humanIdleFrames, t + 0.5f, 6f);
                }
                subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                yield return null;
            }
            PlaySfx(SfxType.ChargeReady);

            // =========================================================================================
            // ESCENA 2 DE 5: SOBRECARGA EN LOS SERVIDORES Y DESPERTAR DE S.I.A.
            // =========================================================================================
            cutsceneAdvanceRequested = false;
            yield return null;
            PlaySfx(SfxType.BossCast);
            topSceneCounter.text = $"🔴 ESCENA 2 DE 5  —  {CharacterNames[selectedChar]}";
            subtitleSpeaker.text = "ESCENA 2 DE 5 — SOBRECARGA EN LOS SERVIDORES";
            subtitleText.text =
                "Una descarga de alto voltaje dañó los servidores principales del laboratorio y corrompió la red neuronal de S.I.A.\n" +
                "Al perder sus protocolos de seguridad, la inteligencia artificial bloqueó todas las instalaciones.";
            AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

            if (wraith != null)
            {
                wraith.SetActive(true);
                wraith.transform.position = new Vector3(1.5f, 5.0f, 0f);
            }

            t = 0f;
            while (!(t > 0.35f && CheckCutsceneNextSceneInput()))
            {
                if (GameInput.GetPauseDown()) { LoadSceneWithAnimatedTransition("TutorialScene"); yield break; }
                t += Time.deltaTime;

                float alarmPulse = Mathf.Abs(Mathf.Sin(t * 6.5f)) * 0.20f;
                flashImg.color = new Color(1.0f, 0.10f, 0.25f, alarmPulse);

                if (cam != null)
                {
                    Vector2 shake = UnityEngine.Random.insideUnitCircle * (t < 2.5f ? 0.07f : 0.025f);
                    cam.transform.position = new Vector3(1.1f + shake.x, 0.35f + shake.y, -10f);
                    cam.orthographicSize = Mathf.Lerp(4.7f, 5.1f, Mathf.Clamp01(t / 2.5f));
                }

                if (wraith != null)
                {
                    float wy = Mathf.Lerp(5.0f, 1.65f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 1.8f)));
                    wraith.transform.position = new Vector3(1.5f + Mathf.Sin(t * 2.8f) * 0.35f, wy, 0f);
                }

                if (prof1 != null) AnimateSpriteFrames(prof1Sr, humanIdleFrames, t, 8f);
                if (prof2 != null) AnimateSpriteFrames(prof2Sr, humanIdleFrames, t + 0.3f, 8f);
                subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                yield return null;
            }
            PlaySfx(SfxType.ShootCharged);

            // =========================================================================================
            // ESCENA 3 DE 5: LAS PULSERAS TRANSFORMAN A LOS DOCENTES EN ESPECTROS GIGANTES
            // =========================================================================================
            cutsceneAdvanceRequested = false;
            yield return null;
            topSceneCounter.text = $"🔴 ESCENA 3 DE 5  —  {CharacterNames[selectedChar]}";
            subtitleSpeaker.text = "ESCENA 3 DE 5 — TRANSFORMACIÓN DE LOS DOCENTES";
            subtitleText.text =
                "S.I.A. envió una señal corruptora a las pulseras inteligentes de los profesores, tomando el control de sus mentes.\n" +
                "La sobrecarga los transformó en espectros gigantes escoltados por drones de vigilancia.";
            AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

            if (robot1 != null)
            {
                robot1.SetActive(true);
                robot1.transform.position = new Vector3(7.5f, -1.35f, 0f);
            }

            t = 0f;
            while (!(t > 0.35f && CheckCutsceneNextSceneInput()))
            {
                if (GameInput.GetPauseDown()) { LoadSceneWithAnimatedTransition("TutorialScene"); yield break; }
                t += Time.deltaTime;

                float ct = Mathf.Clamp01(t / 1.6f);
                flashImg.color = new Color(0.95f, 0.15f, 0.55f, Mathf.Abs(Mathf.Sin(t * 5f)) * 0.14f);

                if (prof1 != null && prof1Sr != null)
                {
                    if (ct < 0.75f)
                    {
                        float giantSc = Mathf.Lerp(1.22f, 2.75f, Mathf.SmoothStep(0f, 1f, ct / 0.75f));
                        prof1.transform.localScale = new Vector3(giantSc, giantSc, 1f);
                        prof1Sr.color = Color.Lerp(Color.white, new Color(1.0f, 0.30f, 0.65f, 1f), ct / 0.75f);
                    }
                    else if (wraithFormSprite != null)
                    {
                        prof1Sr.sprite = wraithFormSprite;
                        prof1Sr.color = Color.white;
                        prof1.transform.localScale = new Vector3(0.88f, 0.88f, 1f);
                    }
                }

                if (robot1 != null)
                {
                    robot1.transform.position = Vector3.MoveTowards(robot1.transform.position, new Vector3(-0.6f, -1.35f, 0f), 3.4f * Time.deltaTime);
                }
                subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                yield return null;
            }
            flashImg.color = Color.clear;

            // =========================================================================================
            // ESCENA 4 DE 5: EL PERSONAJE SELECCIONADO IRRUMPE EN ACCIÓN (KATANA + DISPARO PEM)
            // =========================================================================================
            cutsceneAdvanceRequested = false;
            yield return null;
            PlaySfx(SfxType.Dash);
            topSceneCounter.text = $"🔴 ESCENA 4 DE 5  —  {nick} ({CharacterNames[selectedChar]})";
            subtitleSpeaker.text = $"ESCENA 4 DE 5 — INTERVENCIÓN DE {nick.ToUpper()}";
            subtitleText.text =
                $"{nick}, entras en acción con tu Katana de alta frecuencia para destruir los drones y tu emisor de pulsos electromagnéticos.\n" +
                "Tu misión es sobrecargar las pulseras para liberar a los profesores sin hacerles daño.";
            AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

            t = 0f;
            bool slashedRobot = false;
            bool firedEmpShot = false;
            GameObject cutscenePulseObj = null;

            while (!(t > 0.35f && CheckCutsceneNextSceneInput()))
            {
                if (GameInput.GetPauseDown()) { LoadSceneWithAnimatedTransition("TutorialScene"); yield break; }
                t += Time.deltaTime;

                if (cam != null && hero != null)
                {
                    float targetCamX = Mathf.Clamp(hero.transform.position.x + 2.0f, -4.5f, 1.2f);
                    cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(targetCamX, 0.1f, -10f), Time.deltaTime * 5f);
                    cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 4.7f, Time.deltaTime * 3f);
                }

                if (hero != null && t < 1.8f)
                {
                    hero.transform.position = Vector3.MoveTowards(hero.transform.position, new Vector3(-2.0f, -2.05f, 0f), 5.6f * Time.deltaTime);
                    if (heroAnim != null)
                    {
                        heroAnim.SetBool("IsGrounded", true);
                        heroAnim.SetFloat("Speed", 8f);
                    }
                }
                else if (heroAnim != null)
                {
                    heroAnim.SetBool("IsGrounded", true);
                    heroAnim.SetFloat("Speed", 0f);
                }

                if (t >= 1.9f && !slashedRobot)
                {
                    slashedRobot = true;
                    PlaySfx(SfxType.MeleeSlash);
                    if (heroAnim != null) heroAnim.SetTrigger("Attack");
                    flashImg.color = new Color(0.35f, 0.98f, 1.0f, 0.35f);
                    if (robot1 != null)
                    {
                        PlaySfx(SfxType.EnemyDestroyed);
                        robot1.SetActive(false);
                    }
                }
                else
                {
                    flashImg.color = Color.Lerp(flashImg.color, Color.clear, Time.deltaTime * 8f);
                }

                if (t >= 3.0f && !firedEmpShot && hero != null)
                {
                    firedEmpShot = true;
                    PlaySfx(SfxType.ShootCharged);
                    if (heroAnim != null) heroAnim.SetTrigger("Shoot");

                    cutscenePulseObj = new GameObject("Cutscene_EMP_Bolt");
                    cutscenePulseObj.transform.position = hero.transform.position + new Vector3(0.9f, 1.05f, 0f);
                    cutscenePulseObj.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
                    SpriteRenderer srBolt = cutscenePulseObj.AddComponent<SpriteRenderer>();
                    if (electricSparkSprites != null && electricSparkSprites.Length > 0)
                        srBolt.sprite = electricSparkSprites[0];
                    srBolt.color = new Color(0.25f, 0.98f, 1.0f, 1f);
                    srBolt.sortingOrder = 10;
                }

                if (cutscenePulseObj != null && prof1 != null)
                {
                    Vector3 targetPos = prof1.transform.position + Vector3.up * 1.2f;
                    cutscenePulseObj.transform.position = Vector3.MoveTowards(cutscenePulseObj.transform.position, targetPos, 11f * Time.deltaTime);
                    if (Vector3.Distance(cutscenePulseObj.transform.position, targetPos) < 0.25f)
                    {
                        Destroy(cutscenePulseObj);
                        PlaySfx(SfxType.EnemyHit);
                        flashImg.color = new Color(0.35f, 1.0f, 0.85f, 0.45f);
                    }
                }
                subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                yield return null;
            }
            if (cutscenePulseObj != null) Destroy(cutscenePulseObj);

            // =========================================================================================
            // ESCENA 5 DE 5: DESTRANSFORMACIÓN DEL DOCENTE Y COMIENZO DEL TUTORIAL
            // =========================================================================================
            cutsceneAdvanceRequested = false;
            yield return null;
            topSceneCounter.text = $"🔴 ESCENA 5 DE 5  —  {CharacterNames[selectedChar]}";
            subtitleSpeaker.text = "ESCENA 5 DE 5 — LIBERACIÓN Y TUTORIAL";
            subtitleText.text =
                "Al sobrecargar la pulsera, el profesor recupera su forma humana, vuelve a su tamaño normal y se pone a salvo.\n" +
                "Pulsa ESPACIO para comenzar el Tutorial.";
            subtitlePrompt.text = "▶ PULSA ESPACIO PARA INICIAR EL TUTORIAL ◀";
            AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

            if (wraith != null) wraith.SetActive(false);

            if (prof1 != null && prof1Sr != null && humanIdleFrames != null && humanIdleFrames.Length > 0)
            {
                prof1Sr.sprite = humanIdleFrames[0];
                prof1Sr.color = Color.white;
                prof1.transform.localScale = new Vector3(-2.85f, 2.85f, 1f);
            }

            t = 0f;
            while (!(t > 0.35f && CheckCutsceneNextSceneInput()))
            {
                if (GameInput.GetPauseDown()) { LoadSceneWithAnimatedTransition("TutorialScene"); yield break; }
                t += Time.deltaTime;
                flashImg.color = Color.Lerp(flashImg.color, Color.clear, Time.deltaTime * 6f);

                if (prof1 != null)
                {
                    if (t < 0.9f)
                    {
                        prof1.transform.localScale = new Vector3(-2.85f, 2.85f, 1f);
                        AnimateSpriteFrames(prof1Sr, humanIdleFrames, t, 6f);
                    }
                    else if (t < 2.3f)
                    {
                        float shrinkT = Mathf.SmoothStep(0f, 1f, (t - 0.9f) / 1.4f);
                        float sc = Mathf.Lerp(2.85f, 1.22f, shrinkT);
                        prof1.transform.localScale = new Vector3(-sc, sc, 1f);
                        AnimateSpriteFrames(prof1Sr, humanIdleFrames, t, 7f);
                    }
                    else if (prof1.transform.position.x > -14f)
                    {
                        prof1.transform.localScale = new Vector3(-1.22f, 1.22f, 1f);
                        prof1.transform.position += Vector3.left * (6.2f * Time.deltaTime);
                        AnimateSpriteFrames(prof1Sr, humanRunFrames, t, 14f);
                    }
                }

                subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                yield return null;
            }

            LoadSceneWithAnimatedTransition("TutorialScene");
        }

        private void AnimateSpriteFrames(SpriteRenderer sr, Sprite[] frames, float time, float fps)
        {
            if (sr == null || frames == null || frames.Length == 0) return;
            int idx = Mathf.FloorToInt(time * fps) % frames.Length;
            if (frames[idx] != null) sr.sprite = frames[idx];
        }

        private bool CheckCutsceneNextSceneInput()
        {
            if (cutsceneAdvanceRequested)
            {
                cutsceneAdvanceRequested = false;
                return true;
            }
            return GameInput.GetJumpSpaceOnlyDown();
        }

        // =================================================================================================
        // CINEMÁTICA INTERACTIVA DE LIBERACIÓN DE DOCENTE JEFE (TUTORIAL Y NIVEL 1)
        // =================================================================================================

        private void HandleBossLiberationCutsceneRequested(Transform bossTransform)
        {
            if (isPlayingBossCutscene) return;
            StartCoroutine(RunBossLiberationCutsceneRoutine(bossTransform));
        }

        /// <summary>
        /// Texto de la cinemática de liberación del jefe según el nivel actual (SampleScene = Nivel 1 ... Level4Scene = Nivel 4).
        /// part 1 = purificación, 2 = agradecimiento, 3 = evacuación.
        /// </summary>
        private string GetLevelLiberationText(int part)
        {
            string scene = SceneManager.GetActiveScene().name;
            string nick = GameSettings.PlayerNickname;
            string sector;
            string thanks;
            string closing;

            switch (scene)
            {
                case "Level2Scene":
                    sector = "El jefe de redes recupera su forma humana tras apagarse el pulso hostil que saturaba todo el Centro de Redes.";
                    thanks = $"\"¡Gracias por restaurar el Centro de Redes, {nick}! Con los enlaces de fibra de nuevo en línea rastreamos a S.I.A. hasta la Torre de Servidores.\"";
                    closing = "El docente se pone a salvo mientras el tráfico de la red vuelve a la normalidad.";
                    break;
                case "Level3Scene":
                    sector = "La pulsera del jefe de servidores se desintegra y el docente cae al suelo de la Torre libre de la corrupción de S.I.A.";
                    thanks = $"\"¡La Torre de Servidores es libre gracias a ti, {nick}! Solo queda el Núcleo Central S.I.A.: ahí está el origen de todo. ¡Mucho éxito!\"";
                    closing = "El docente evacúa por la escalera de emergencia mientras la Torre recupera su energía estable.";
                    break;
                case "Level4Scene":
                    sector = "El Núcleo Central S.I.A. entra en sobrecarga irreversible al romperse la pulsera maestra del último jefe.";
                    thanks = $"\"¡Lo lograste, {nick}! S.I.A. está cayendo en todo el campus...\"";
                    closing = "Todos los sistemas centrales comienzan a colapsar.";
                    break;
                default:
                    sector = "El docente jefe de cómputo recupera su forma humana tras disiparse la energía hostil.";
                    thanks = $"\"¡Excelente trabajo rescatándome y despejando el Laboratorio de Cómputo, {nick}!\nAún quedan docentes atrapados en los siguientes edificios del campus. ¡Todo nuestro apoyo va contigo!\"";
                    closing = "El docente se pone a salvo con el equipo de emergencia del campus.";
                    break;
            }

            switch (part)
            {
                case 1:
                    return "¡La sobrecarga PEM destruye la pulsera maestra del jefe!\n" + sector;
                case 2:
                    return thanks;
                default:
                    return closing + "\nPulsa ESPACIO para terminar la cinemática y continuar.";
            }
        }

        private bool CheckBossCutsceneAdvanceInput()
        {
            if (bossCutsceneSkipAll) return true;
            if (cutsceneAdvanceRequested)
            {
                cutsceneAdvanceRequested = false;
                return true;
            }
            return GameInput.GetJumpSpaceOnlyDown();
        }

        private float FindSolidFloorYForCutscene(Vector3 refPos, Transform ignoreRoot)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(refPos.x, refPos.y + 1.5f), Vector2.down, 40f);
            float bestY = refPos.y;
            bool found = false;
            float topHitY = float.MinValue;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D h = hits[i];
                if (h.collider == null || h.collider.isTrigger) continue;
                if (ignoreRoot != null && (h.collider.transform == ignoreRoot || h.collider.transform.IsChildOf(ignoreRoot))) continue;
                if (h.collider.CompareTag("Player") || h.collider.CompareTag("Enemy") || h.collider.CompareTag("Projectile")) continue;
                if (h.normal.y < 0.55f) continue;

                if (h.point.y > topHitY)
                {
                    topHitY = h.point.y;
                    bestY = h.point.y;
                    found = true;
                }
            }

            return found ? bestY : -1.75f;
        }

        private IEnumerator RunBossLiberationCutsceneRoutine(Transform bossTransform)
        {
            isPlayingBossCutscene = true;
            bossCutsceneSkipAll = false;
            cutsceneAdvanceRequested = false;
            Time.timeScale = 1f;

            int selectedChar = Mathf.Clamp(PlayerPrefs.GetInt("CDE_SelectedCharacterIndex", 0), 0, CharacterNames.Length - 1);

            string nick = GameSettings.PlayerNickname;
            if (playerHealth != null)
            {
                PlayerPrefs.SetInt("CDE_EndingLastLives", playerHealth.CurrentLives);
                PlayerPrefs.Save();
            }
            string activeScene = SceneManager.GetActiveScene().name;
            bool leadsToEnding = activeScene == "Level4Scene";

            SetGameplayHudVisible(false);
            if (tutorialOverlayPanel != null) tutorialOverlayPanel.SetActive(false);
            if (cutsceneDialoguePanel != null) cutsceneDialoguePanel.SetActive(false);
            if (checkpointToastPanel != null) checkpointToastPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (unifiedSettingsOverlay != null) unifiedSettingsOverlay.SetActive(false);
            if (pauseSaveSlotsPanel != null) pauseSaveSlotsPanel.SetActive(false);

            if (playerMovement != null)
            {
                playerMovement.SetMovementEnabled(false);
                Rigidbody2D pRb = playerMovement.GetComponent<Rigidbody2D>();
                if (pRb != null) pRb.linearVelocity = Vector2.zero;
                Animator pAnim = playerMovement.GetComponent<Animator>();
                if (pAnim != null)
                {
                    pAnim.SetFloat("Speed", 0f);
                    pAnim.SetBool("IsGrounded", true);
                }
            }
            if (playerShooting != null) playerShooting.SetShootingEnabled(false);

            Camera cam = Camera.main;
            CameraFollow2D camFollow = cam != null ? cam.GetComponent<CameraFollow2D>() : null;
            Transform origCamTarget = playerMovement != null ? playerMovement.transform : null;
            GameObject focusAnchor = new GameObject("BossCutscene_CamFocus");

            SpriteRenderer bossSr = bossTransform != null ? bossTransform.GetComponent<SpriteRenderer>() : null;
            Rigidbody2D bossRb = bossTransform != null ? bossTransform.GetComponent<Rigidbody2D>() : null;
            if (bossRb != null)
            {
                bossRb.gravityScale = 0f;
                bossRb.linearVelocity = Vector2.zero;
            }
            if (bossTransform != null)
            {
                Collider2D[] cols = bossTransform.GetComponentsInChildren<Collider2D>(true);
                for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;
                Animator bAnim = bossTransform.GetComponent<Animator>();
                if (bAnim != null) bAnim.enabled = false;
            }

            Vector3 bossStartPos = bossTransform != null ? bossTransform.position : (origCamTarget != null ? origCamTarget.position + Vector3.right * 4f : Vector3.zero);
            Vector3 heroPos = origCamTarget != null ? origCamTarget.position : bossStartPos + Vector3.left * 4.5f;
            float floorY = FindSolidFloorYForCutscene(bossStartPos, bossTransform);
            Vector3 bossLandPos = new Vector3(bossStartPos.x, floorY, 0f);

            if (playerMovement != null)
                playerMovement.FaceDirection(bossStartPos.x >= heroPos.x);

            focusAnchor.transform.position = Vector3.Lerp(heroPos, bossStartPos, 0.58f);
            if (camFollow != null) camFollow.SetTarget(focusAnchor.transform);

            // Construcción de UI Cinematográfica idéntica al estilo de IntroCutscene
            GameObject cinemaRoot = new GameObject("BossLiberation_Movie_UI");
            cinemaRoot.transform.SetParent(transform, false);
            StretchFullScreen(cinemaRoot);

            GameObject topLetterbox = CreateGlassCard(cinemaRoot.transform, "Cinema_Top_Bar",
                new Vector2(0.02f, 0.90f), new Vector2(0.98f, 0.985f),
                new Color(0.01f, 0.03f, 0.08f, 0.94f), new Color(0.25f, 0.92f, 1.0f, 0.85f));

            Text topSceneCounter = CreateDynamicText(topLetterbox.transform, "Rec_Badge",
                $"🔴 LIBERACIÓN — ESCENA 1 DE 3  —  {CharacterNames[selectedChar]}", 15,
                new Vector2(0.03f, 0.08f), new Vector2(0.72f, 0.92f), new Color(0.45f, 0.96f, 1.0f, 1f), TextAnchor.MiddleLeft);

            CreateGlassButton(topLetterbox.transform, "Btn_Skip_Movie", "⏭  OMITIR CINEMÁTICA",
                new Vector2(0.75f, 0.14f), new Vector2(0.98f, 0.86f),
                new Color(0.32f, 0.10f, 0.18f, 0.92f), new Color(1.0f, 0.45f, 0.55f, 0.95f),
                () => { bossCutsceneSkipAll = true; cutsceneAdvanceRequested = true; });

            GameObject bottomCard = CreateGlassCard(cinemaRoot.transform, "Cinema_Bottom_Narrative_Card",
                new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.27f),
                new Color(0.01f, 0.04f, 0.11f, 0.95f), new Color(0.25f, 0.95f, 1.0f, 0.92f));
            RectTransform bottomCardRect = bottomCard.GetComponent<RectTransform>();

            Text subtitleSpeaker = CreateDynamicText(bottomCard.transform, "Movie_Subtitle_Speaker", "", 18,
                new Vector2(0.03f, 0.72f), new Vector2(0.68f, 0.95f), new Color(0.30f, 0.96f, 1.0f, 1f), TextAnchor.MiddleLeft, useHeaderFont: true);

            Text subtitlePrompt = CreateDynamicText(bottomCard.transform, "Movie_Space_Indicator",
                "▶ PULSA ESPACIO PARA CONTINUAR ◀", 15,
                new Vector2(0.03f, 0.05f), new Vector2(0.68f, 0.24f), new Color(1.0f, 0.88f, 0.30f, 1f), TextAnchor.MiddleLeft);

            Text subtitleText = CreateDynamicText(bottomCard.transform, "Movie_Subtitle_Text", "", 16,
                new Vector2(0.03f, 0.25f), new Vector2(0.70f, 0.71f), new Color(0.95f, 0.98f, 1.0f, 1f), TextAnchor.MiddleLeft);

            GameObject nextStepBtn = CreateGlassButton(bottomCard.transform, "Btn_Next_Cutscene_Step", "SIGUIENTE ▶",
                new Vector2(0.72f, 0.18f), new Vector2(0.97f, 0.82f),
                new Color(0.06f, 0.48f, 0.62f, 0.95f), new Color(0.25f, 1.0f, 0.80f, 1f),
                () => { cutsceneAdvanceRequested = true; });
            Text nextStepBtnText = nextStepBtn.GetComponentInChildren<Text>();

            GameObject flashObj = new GameObject("Cinema_Screen_Flash");
            flashObj.transform.SetParent(cinemaRoot.transform, false);
            StretchFullScreen(flashObj);
            Image flashImg = flashObj.AddComponent<Image>();
            flashImg.color = Color.clear;
            flashImg.raycastTarget = false;

            float faceHeroSign = (heroPos.x < bossLandPos.x) ? -1f : 1f;

            // =========================================================================================
            // ESCENA 1 DE 3: SOBRECARGA DE PULSERA Y PURIFICACIÓN HUMANA (ATERIZAJE FIRME AL SUELO)
            // =========================================================================================
            PlaySfx(SfxType.EnemyDestroyed);
            float flashScale = GameSettings.ScreenFlashScale;
            flashImg.color = new Color(0.35f, 1.0f, 0.85f, 0.72f * flashScale);
            CameraFollow2D.RequestShake(leadsToEnding ? 0.45f : 0.25f, leadsToEnding ? 0.55f : 0.30f);

            topSceneCounter.text = $"🔴 LIBERACIÓN — ESCENA 1 DE 3  —  {nick}";
            subtitleSpeaker.text = "ESCENA 1 DE 3 — PURIFICACIÓN DEL DOCENTE";
            subtitleText.text = isTutorialScene
                ? $"¡La pulsera de control S.I.A. ha sido neutralizada, {nick}!\nEl instructor de campo se libera de la distorsión electromagnética y desciende al suelo."
                : GetLevelLiberationText(1);
            subtitlePrompt.text = "▶ PULSA ESPACIO PARA CONTINUAR ◀";
            AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -45f), 0.30f);

            bool transformedToHuman = false;
            float t = 0f;
            while (!bossCutsceneSkipAll && !(t > 0.35f && CheckBossCutsceneAdvanceInput()))
            {
                if (GameInput.GetPauseDown()) { bossCutsceneSkipAll = true; break; }
                t += Time.deltaTime;
                flashImg.color = Color.Lerp(flashImg.color, Color.clear, Time.deltaTime * 5f);

                if (bossTransform != null)
                {
                    float landProgress = Mathf.Clamp01(t / 1.15f);
                    bossTransform.position = Vector3.Lerp(bossStartPos, bossLandPos, Mathf.SmoothStep(0f, 1f, landProgress));

                    if (!transformedToHuman && landProgress >= 0.45f)
                    {
                        transformedToHuman = true;
                        flashImg.color = new Color(0.75f, 1.0f, 0.95f, 0.45f * flashScale);
                        SpriteRenderer[] extraRenderers = bossTransform.GetComponentsInChildren<SpriteRenderer>(true);
                        for (int i = 0; i < extraRenderers.Length; i++)
                        {
                            if (extraRenderers[i] != bossSr) extraRenderers[i].enabled = false;
                        }
                        if (bossSr != null && humanIdleFrames != null && humanIdleFrames.Length > 0)
                        {
                            bossSr.sprite = humanIdleFrames[0];
                            bossSr.color = Color.white;
                        }
                    }

                    if (transformedToHuman)
                    {
                        float settleScale = Mathf.Lerp(1.65f, 1.25f, Mathf.Clamp01((landProgress - 0.45f) / 0.55f));
                        bossTransform.localScale = new Vector3(faceHeroSign * settleScale, settleScale, 1f);
                        AnimateSpriteFrames(bossSr, humanIdleFrames, t, 6f);
                    }
                    else if (bossSr != null)
                    {
                        bossSr.color = Color.Lerp(new Color(1f, 0.45f, 0.75f, 1f), new Color(0.45f, 1f, 0.9f, 1f), Mathf.PingPong(t * 7f, 1f));
                    }

                    focusAnchor.transform.position = Vector3.Lerp(heroPos, bossTransform.position, 0.58f);
                }

                subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                yield return null;
            }

            if (bossTransform != null)
            {
                bossTransform.position = bossLandPos;
                bossTransform.localScale = new Vector3(faceHeroSign * 1.25f, 1.25f, 1f);
                SpriteRenderer[] extraRenderers = bossTransform.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < extraRenderers.Length; i++)
                {
                    if (extraRenderers[i] != bossSr) extraRenderers[i].enabled = false;
                }
                if (bossSr != null && humanIdleFrames != null && humanIdleFrames.Length > 0)
                {
                    bossSr.sprite = humanIdleFrames[0];
                    bossSr.color = Color.white;
                }
            }

            // Si es el jefe final del Nivel 4, tras caer al suelo y estallar el Núcleo entra directo a la Cinemática Final
            if (leadsToEnding)
            {
                if (camFollow != null && origCamTarget != null) camFollow.SetTarget(origCamTarget);
                if (focusAnchor != null) Destroy(focusAnchor);
                if (bossTransform != null) Destroy(bossTransform.gameObject);
                if (cinemaRoot != null) Destroy(cinemaRoot);
                isPlayingBossCutscene = false;
                LoadSceneWithAnimatedTransition("EndingCutscene");
                yield break;
            }

            // =========================================================================================
            // ESCENA 2 DE 3: AGRADECIMIENTO DEL DOCENTE LIBERADO
            // =========================================================================================
            if (!bossCutsceneSkipAll)
            {
                cutsceneAdvanceRequested = false;
                yield return null;
                topSceneCounter.text = $"🔴 LIBERACIÓN — ESCENA 2 DE 3  —  {nick}";
                subtitleSpeaker.text = isTutorialScene
                    ? "ESCENA 2 DE 3 — INSTRUCTOR DE CAMPO LIBERADO"
                    : "ESCENA 2 DE 3 — DOCENTE JEFE LIBERADO";
                subtitleText.text = isTutorialScene
                    ? $"\"¡Muchísimas gracias por liberarme, {nick}! La red S.I.A. alteró mi pulsera para sellar esta zona.\nDemostraste gran dominio del pulso PEM y la katana. Estás listo para infiltrarte en el Nivel 1.\""
                    : GetLevelLiberationText(2);
                subtitlePrompt.text = "▶ PULSA ESPACIO PARA CONTINUAR ◀";
                AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

                t = 0f;
                while (!bossCutsceneSkipAll && !(t > 0.35f && CheckBossCutsceneAdvanceInput()))
                {
                    if (GameInput.GetPauseDown()) { bossCutsceneSkipAll = true; break; }
                    t += Time.deltaTime;
                    flashImg.color = Color.Lerp(flashImg.color, Color.clear, Time.deltaTime * 6f);

                    if (bossTransform != null)
                    {
                        AnimateSpriteFrames(bossSr, humanIdleFrames, t, 7f);
                        focusAnchor.transform.position = Vector3.Lerp(heroPos, bossTransform.position, 0.52f);
                    }

                    subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                    yield return null;
                }
            }

            // =========================================================================================
            // ESCENA 3 DE 3: EVACUACIÓN SEGURA Y CIERRE CON [ESPACIO]
            // =========================================================================================
            if (!bossCutsceneSkipAll)
            {
                cutsceneAdvanceRequested = false;
                yield return null;
                topSceneCounter.text = $"🔴 LIBERACIÓN — ESCENA 3 DE 3  —  {nick}";
                subtitleSpeaker.text = "ESCENA 3 DE 3 — SECTOR ASEGURADO";
                subtitleText.text = isTutorialScene
                    ? $"El instructor evacúa hacia la zona segura mientras se desbloquean los accesos, {nick}.\nPulsa ESPACIO para terminar la cinemática y continuar al siguiente nivel."
                    : GetLevelLiberationText(3);
                subtitlePrompt.text = "▶ PULSA ESPACIO PARA TERMINAR LA CINEMÁTICA ◀";
                if (nextStepBtnText != null) nextStepBtnText.text = "FINALIZAR ▶";
                AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

                bool evacuatedOutOfFrame = false;
                t = 0f;
                while (!bossCutsceneSkipAll && !(t > 0.35f && CheckBossCutsceneAdvanceInput()))
                {
                    if (GameInput.GetPauseDown()) { bossCutsceneSkipAll = true; break; }
                    t += Time.deltaTime;
                    flashImg.color = Color.Lerp(flashImg.color, Color.clear, Time.deltaTime * 6f);

                    if (bossTransform != null && !evacuatedOutOfFrame)
                    {
                        if (t < 0.45f)
                        {
                            bossTransform.localScale = new Vector3(faceHeroSign * 1.25f, 1.25f, 1f);
                            AnimateSpriteFrames(bossSr, humanIdleFrames, t, 6f);
                        }
                        else
                        {
                            bossTransform.localScale = new Vector3(1.25f, 1.25f, 1f);
                            bossTransform.position += Vector3.right * (6.2f * Time.deltaTime);
                            AnimateSpriteFrames(bossSr, humanRunFrames, t, 14f);

                            if (cam != null)
                            {
                                Vector3 vp = cam.WorldToViewportPoint(bossTransform.position);
                                if (vp.x > 1.08f)
                                {
                                    evacuatedOutOfFrame = true;
                                    if (bossSr != null) bossSr.enabled = false;
                                }
                            }
                        }
                    }

                    subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                    yield return null;
                }
            }

            if (camFollow != null && origCamTarget != null)
                camFollow.SetTarget(origCamTarget);
            if (focusAnchor != null)
                Destroy(focusAnchor);
            if (bossTransform != null)
                Destroy(bossTransform.gameObject);
            if (cinemaRoot != null)
                Destroy(cinemaRoot);

            isPlayingBossCutscene = false;
            GameEvents.TriggerLevelStateChanged(LevelState.Victory);
        }

        // =================================================================================================
        // CINEMÁTICA FINAL EXTENSA (EndingCutscene.unity — 8 ESCENAS CON CONDECORACIÓN Y CRÉDITOS)
        // =================================================================================================

        private IEnumerator RunEndingCutsceneDirector()
        {
            Time.timeScale = 1f;
            int selectedChar = Mathf.Clamp(PlayerPrefs.GetInt("CDE_SelectedCharacterIndex", 0), 0, CharacterNames.Length - 1);
            int partnerChar = (selectedChar == 0) ? 1 : 0;
            string nick = GameSettings.PlayerNickname;
            int savedLives = PlayerPrefs.GetInt("CDE_EndingLastLives", GameSettings.StartingLives);
            float flashScale = GameSettings.ScreenFlashScale;
            bool skipToFinalSummary = false;

            GameObject cinemaRoot = new GameObject("EndingCutscene_Movie_UI");
            cinemaRoot.transform.SetParent(transform, false);
            StretchFullScreen(cinemaRoot);

            GameObject topLetterbox = CreateGlassCard(cinemaRoot.transform, "Cinema_Top_Bar",
                new Vector2(0.02f, 0.90f), new Vector2(0.98f, 0.985f),
                new Color(0.01f, 0.03f, 0.08f, 0.94f), new Color(0.30f, 0.96f, 0.85f, 0.90f));

            Text topSceneCounter = CreateDynamicText(topLetterbox.transform, "Rec_Badge",
                $"🏆 FINAL — ESCENA 1 DE 8  —  {nick}", 15,
                new Vector2(0.03f, 0.08f), new Vector2(0.72f, 0.92f), new Color(0.45f, 1.0f, 0.88f, 1f), TextAnchor.MiddleLeft);

            CreateGlassButton(topLetterbox.transform, "Btn_Skip_Ending", "⏭  IR AL REPORTE",
                new Vector2(0.75f, 0.14f), new Vector2(0.98f, 0.86f),
                new Color(0.28f, 0.14f, 0.24f, 0.92f), new Color(1.0f, 0.72f, 0.35f, 0.95f),
                () => { skipToFinalSummary = true; cutsceneAdvanceRequested = true; });

            GameObject bottomCard = CreateGlassCard(cinemaRoot.transform, "Cinema_Bottom_Narrative_Card",
                new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.265f),
                new Color(0.01f, 0.04f, 0.11f, 0.95f), new Color(0.25f, 0.96f, 0.88f, 0.94f));
            RectTransform bottomCardRect = bottomCard.GetComponent<RectTransform>();

            Text subtitleSpeaker = CreateDynamicText(bottomCard.transform, "Movie_Subtitle_Speaker", "", 18,
                new Vector2(0.03f, 0.72f), new Vector2(0.68f, 0.95f), new Color(0.35f, 1.0f, 0.85f, 1f), TextAnchor.MiddleLeft, useHeaderFont: true);

            Text subtitlePrompt = CreateDynamicText(bottomCard.transform, "Movie_Space_Indicator",
                "▶ PULSA ESPACIO PARA CONTINUAR ◀", 15,
                new Vector2(0.03f, 0.05f), new Vector2(0.68f, 0.24f), new Color(1.0f, 0.88f, 0.30f, 1f), TextAnchor.MiddleLeft);

            Text subtitleText = CreateDynamicText(bottomCard.transform, "Movie_Subtitle_Text", "", 16,
                new Vector2(0.03f, 0.25f), new Vector2(0.70f, 0.71f), new Color(0.95f, 0.98f, 1.0f, 1f), TextAnchor.MiddleLeft);

            GameObject nextStepBtn = CreateGlassButton(bottomCard.transform, "Btn_Next_Ending_Step", "SIGUIENTE ▶",
                new Vector2(0.72f, 0.18f), new Vector2(0.97f, 0.82f),
                new Color(0.06f, 0.48f, 0.56f, 0.95f), new Color(0.25f, 1.0f, 0.80f, 1f),
                () => { cutsceneAdvanceRequested = true; });
            Text nextStepBtnText = nextStepBtn.GetComponentInChildren<Text>();

            GameObject flashObj = new GameObject("Cinema_Screen_Flash");
            flashObj.transform.SetParent(cinemaRoot.transform, false);
            StretchFullScreen(flashObj);
            Image flashImg = flashObj.AddComponent<Image>();
            flashImg.color = Color.clear;
            flashImg.raycastTarget = false;

            // Referencias de escenario 5 zonas
            Camera cam = Camera.main;
            GameObject wraithBoss = GameObject.Find("Ending_Wraith_Boss");
            GameObject bossHuman = GameObject.Find("Ending_Boss_Human");
            SpriteRenderer wraithSr = wraithBoss != null ? wraithBoss.GetComponent<SpriteRenderer>() : null;
            SpriteRenderer bossHumanSr = bossHuman != null ? bossHuman.GetComponent<SpriteRenderer>() : null;

            GameObject vfxBurstObj = new GameObject("Ending_VFX_Actor");
            vfxBurstObj.transform.position = new Vector3(3.2f, 0.6f, 0f);
            vfxBurstObj.transform.localScale = new Vector3(2.8f, 2.8f, 1f);
            SpriteRenderer vfxBurstSr = vfxBurstObj.AddComponent<SpriteRenderer>();
            vfxBurstSr.sortingOrder = 14;
            vfxBurstObj.SetActive(false);

            GameObject[] robots = new GameObject[3];
            SpriteRenderer[] robotSrs = new SpriteRenderer[3];
            SpriteRenderer[] alertScreens = new SpriteRenderer[3];
            SpriteRenderer[] alarmLights = new SpriteRenderer[3];
            for (int i = 0; i < 3; i++)
            {
                robots[i] = GameObject.Find($"Ending_Robot_{i}");
                if (robots[i] != null) robotSrs[i] = robots[i].GetComponent<SpriteRenderer>();
                GameObject scr = GameObject.Find($"Ending_Alert_Screen_{i}");
                if (scr != null) alertScreens[i] = scr.GetComponent<SpriteRenderer>();
                GameObject al = GameObject.Find($"Ending_Alarm_Light_{i}");
                if (al != null) alarmLights[i] = al.GetComponent<SpriteRenderer>();
            }

            GameObject[] freeProfs = new GameObject[4];
            SpriteRenderer[] freeProfSrs = new SpriteRenderer[4];
            for (int i = 0; i < 4; i++)
            {
                freeProfs[i] = GameObject.Find($"Ending_Free_Prof_{i}");
                if (freeProfs[i] != null) freeProfSrs[i] = freeProfs[i].GetComponent<SpriteRenderer>();
            }

            GameObject hero = GameObject.Find("Ending_Hero");
            GameObject partner = GameObject.Find("Ending_Partner");
            Animator heroAnim = hero != null ? hero.GetComponent<Animator>() : null;
            SpriteRenderer heroSr = hero != null ? hero.GetComponent<SpriteRenderer>() : null;
            Animator partnerAnim = partner != null ? partner.GetComponent<Animator>() : null;
            SpriteRenderer partnerSr = partner != null ? partner.GetComponent<SpriteRenderer>() : null;

            if (heroAnim != null && playableCharacterControllers != null && selectedChar < playableCharacterControllers.Length && playableCharacterControllers[selectedChar] != null)
            {
                heroAnim.runtimeAnimatorController = playableCharacterControllers[selectedChar];
                heroAnim.SetBool("IsGrounded", true);
                heroAnim.SetFloat("Speed", 0f);
            }
            if (heroSr != null && playableCharacterIdleSprites != null && selectedChar < playableCharacterIdleSprites.Length && playableCharacterIdleSprites[selectedChar] != null)
            {
                heroSr.sprite = playableCharacterIdleSprites[selectedChar];
            }

            if (partnerAnim != null && playableCharacterControllers != null && partnerChar < playableCharacterControllers.Length && playableCharacterControllers[partnerChar] != null)
            {
                partnerAnim.runtimeAnimatorController = playableCharacterControllers[partnerChar];
                partnerAnim.SetBool("IsGrounded", true);
                partnerAnim.SetFloat("Speed", 0f);
            }
            if (partnerSr != null && playableCharacterIdleSprites != null && partnerChar < playableCharacterIdleSprites.Length && playableCharacterIdleSprites[partnerChar] != null)
            {
                partnerSr.sprite = playableCharacterIdleSprites[partnerChar];
            }

            GameObject[] faculty = new GameObject[4];
            SpriteRenderer[] facultySrs = new SpriteRenderer[4];
            for (int i = 0; i < 4; i++)
            {
                faculty[i] = GameObject.Find($"Ending_Faculty_{i}");
                if (faculty[i] != null) facultySrs[i] = faculty[i].GetComponent<SpriteRenderer>();
            }

            GameObject awardGlow = GameObject.Find("Ending_Award_Glow");
            GameObject medalObj = GameObject.Find("Ending_Medal_Icon");
            SpriteRenderer medalSr = medalObj != null ? medalObj.GetComponent<SpriteRenderer>() : null;
            if (medalSr != null && endingMedalSprite != null) medalSr.sprite = endingMedalSprite;
            if (medalObj != null) medalObj.SetActive(false);
            if (awardGlow != null) awardGlow.SetActive(false);

            // =========================================================================================
            // ESCENA 1 DE 8: COLAPSO FINAL DEL NÚCLEO S.I.A. Y PURIFICACIÓN DEL ÚLTIMO JEFE
            // =========================================================================================
            PlaySfx(SfxType.ShootCharged);
            flashImg.color = new Color(1.0f, 0.45f, 0.55f, 0.65f * flashScale);
            topSceneCounter.text = $"🏆 FINAL — ESCENA 1 DE 8  —  {nick}";
            subtitleSpeaker.text = "ESCENA 1 DE 8 — COLAPSO DEL NÚCLEO CENTRAL S.I.A.";
            subtitleText.text =
                $"Con el golpe decisivo de {nick}, la pulsera maestra del Núcleo Central estalla en pedazos.\n" +
                "La manifestación digital de S.I.A. se desintegra y el último docente jefe cae a salvo al suelo.";
            AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -45f), 0.30f);

            if (bossHuman != null) bossHuman.SetActive(false);
            if (cam != null) { cam.transform.position = new Vector3(1.5f, 0.3f, -10f); cam.orthographicSize = 5.4f; }

            float t = 0f;
            bool boomDone = false;
            bool landedHuman = false;
            while (!skipToFinalSummary && !(t > 0.35f && (CheckCutsceneNextSceneInput() || GameInput.GetPauseDown())))
            {
                t += Time.deltaTime;
                flashImg.color = Color.Lerp(flashImg.color, Color.clear, Time.deltaTime * 4.5f);

                if (cam != null && t < 1.3f)
                {
                    Vector2 sk = UnityEngine.Random.insideUnitCircle * (0.14f * GameSettings.ScreenShakeScale * (1f - t / 1.3f));
                    cam.transform.position = new Vector3(1.5f + sk.x, 0.3f + sk.y, -10f);
                }

                if (t < 1.1f)
                {
                    vfxBurstObj.SetActive(true);
                    if (endingExplosionFrames != null && endingExplosionFrames.Length > 0)
                    {
                        int idx = Mathf.Clamp(Mathf.FloorToInt(t / 1.1f * endingExplosionFrames.Length), 0, endingExplosionFrames.Length - 1);
                        vfxBurstSr.sprite = endingExplosionFrames[idx];
                    }
                    if (wraithBoss != null && wraithSr != null && endingWraithDyingFrames != null && endingWraithDyingFrames.Length > 0)
                    {
                        int dIdx = Mathf.Clamp(Mathf.FloorToInt(t / 1.1f * endingWraithDyingFrames.Length), 0, endingWraithDyingFrames.Length - 1);
                        wraithSr.sprite = endingWraithDyingFrames[dIdx];
                        wraithSr.color = Color.Lerp(Color.white, new Color(0.4f, 1f, 0.9f, 0f), t / 1.1f);
                    }
                }
                else
                {
                    if (!boomDone)
                    {
                        boomDone = true;
                        PlaySfx(SfxType.EnemyDestroyed);
                        vfxBurstObj.SetActive(false);
                        if (wraithBoss != null) wraithBoss.SetActive(false);
                        if (bossHuman != null) { bossHuman.SetActive(true); bossHuman.transform.position = new Vector3(3.2f, 2.0f, 0f); }
                        flashImg.color = new Color(0.40f, 1.0f, 0.85f, 0.50f * flashScale);
                    }
                    if (bossHuman != null)
                    {
                        float fallT = Mathf.Clamp01((t - 1.1f) / 0.85f);
                        bossHuman.transform.position = Vector3.Lerp(new Vector3(3.2f, 2.0f, 0f), new Vector3(3.2f, -2.05f, 0f), Mathf.SmoothStep(0f, 1f, fallT));
                        if (!landedHuman && fallT >= 0.95f)
                        {
                            landedHuman = true;
                        }
                        AnimateSpriteFrames(bossHumanSr, humanIdleFrames, t, 6f);
                    }
                }

                subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                yield return null;
            }
            vfxBurstObj.SetActive(false);
            flashImg.color = Color.clear;

            // =========================================================================================
            // ESCENA 2 DE 8: APAGÓN EN CADENA DE DRONES Y NORMALIZACIÓN DE PANTALLAS
            // =========================================================================================
            if (!skipToFinalSummary)
            {
                cutsceneAdvanceRequested = false;
                yield return null;
                PlaySfx(SfxType.ShootNormal);
                topSceneCounter.text = $"🏆 FINAL — ESCENA 2 DE 8  —  {nick}";
                subtitleSpeaker.text = "ESCENA 2 DE 8 — APAGÓN DE LA RED HOSTIL";
                subtitleText.text =
                    "Sin la señal del Núcleo Central, todos los drones de S.I.A. se desconectan en cascada.\n" +
                    "Las alertas rojas se apagan y las terminales de seguridad vuelven a su código azul estable.";
                AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

                bool screensSwapped = false;
                t = 0f;
                while (!skipToFinalSummary && !(t > 0.35f && (CheckCutsceneNextSceneInput() || GameInput.GetPauseDown())))
                {
                    t += Time.deltaTime;
                    if (cam != null)
                    {
                        cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(36f, 0.5f, -10f), Time.deltaTime * 4.5f);
                        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 5.8f, Time.deltaTime * 3f);
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        if (robots[i] != null && robotSrs[i] != null)
                        {
                            Animator rAnim = robots[i].GetComponent<Animator>();
                            if (rAnim != null) rAnim.enabled = false;
                            if (endingRobotDeadFrames != null && endingRobotDeadFrames.Length > 0)
                            {
                                float localT = Mathf.Clamp01((t - i * 0.25f) / 0.85f);
                                int dIdx = Mathf.Clamp(Mathf.FloorToInt(localT * endingRobotDeadFrames.Length), 0, endingRobotDeadFrames.Length - 1);
                                robotSrs[i].sprite = endingRobotDeadFrames[dIdx];
                            }
                            robots[i].transform.position = Vector3.MoveTowards(robots[i].transform.position, new Vector3(robots[i].transform.position.x, -2.0f, 0f), 3.0f * Time.deltaTime);
                        }
                        if (alarmLights[i] != null)
                        {
                            Color ac = alarmLights[i].color;
                            ac.a = Mathf.Lerp(ac.a, 0f, Time.deltaTime * 2.5f);
                            alarmLights[i].color = ac;
                        }
                    }

                    if (endingLightningFrames != null && endingLightningFrames.Length > 0 && t < 1.2f)
                    {
                        vfxBurstObj.SetActive(true);
                        vfxBurstObj.transform.position = new Vector3(36f, -0.4f, 0f);
                        vfxBurstObj.transform.localScale = new Vector3(3.2f, 2.2f, 1f);
                        AnimateSpriteFrames(vfxBurstSr, endingLightningFrames, t, 14f);
                    }
                    else
                    {
                        vfxBurstObj.SetActive(false);
                    }

                    if (!screensSwapped && t > 1.0f)
                    {
                        screensSwapped = true;
                        flashImg.color = new Color(0.25f, 0.92f, 1.0f, 0.35f * flashScale);
                        for (int i = 0; i < 3; i++)
                        {
                            if (alertScreens[i] != null)
                            {
                                Sprite stable = (i % 2 == 0) ? endingScreenBlueSprite : endingScreenCodeSprite;
                                if (stable != null) alertScreens[i].sprite = stable;
                            }
                        }
                    }
                    flashImg.color = Color.Lerp(flashImg.color, Color.clear, Time.deltaTime * 6f);
                    subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                    yield return null;
                }
            }

            // =========================================================================================
            // ESCENA 3 DE 8: LIBERACIÓN EN LOS 4 EDIFICIOS DEL CAMPUS
            // =========================================================================================
            if (!skipToFinalSummary)
            {
                cutsceneAdvanceRequested = false;
                yield return null;
                flashImg.color = new Color(0.30f, 1.0f, 0.72f, 0.40f * flashScale);
                topSceneCounter.text = $"🏆 FINAL — ESCENA 3 DE 8  —  {nick}";
                subtitleSpeaker.text = "ESCENA 3 DE 8 — LIBERACIÓN EN LOS 4 SECTORES";
                subtitleText.text =
                    "En el Laboratorio de Cómputo, el Centro de Redes, la Torre de Servidores y el Núcleo Central,\n" +
                    $"los docentes despiertan ilesos mientras el pulso purificador recorre toda la UTNG.";
                AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

                t = 0f;
                while (!skipToFinalSummary && !(t > 0.35f && (CheckCutsceneNextSceneInput() || GameInput.GetPauseDown())))
                {
                    t += Time.deltaTime;
                    flashImg.color = Color.Lerp(flashImg.color, Color.clear, Time.deltaTime * 6f);
                    if (cam != null)
                    {
                        cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(72f, 0.6f, -10f), Time.deltaTime * 4.5f);
                        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 6.0f, Time.deltaTime * 3f);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        if (freeProfs[i] == null) continue;
                        float targetX = 63f + i * 6f;
                        if (Mathf.Abs(freeProfs[i].transform.position.x - targetX) > 0.1f && t < 1.5f)
                        {
                            freeProfs[i].transform.position = Vector3.MoveTowards(freeProfs[i].transform.position, new Vector3(targetX, -2.05f, 0f), 4.0f * Time.deltaTime);
                            AnimateSpriteFrames(freeProfSrs[i], humanRunFrames, t + i * 0.2f, 12f);
                        }
                        else
                        {
                            AnimateSpriteFrames(freeProfSrs[i], humanIdleFrames, t + i * 0.25f, 6f);
                        }
                    }

                    subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                    yield return null;
                }
            }

            // =========================================================================================
            // ESCENA 4 DE 8: REUNIÓN DE HÉROES Y DOCENTES EN EL ATRIO UTNG
            // =========================================================================================
            if (!skipToFinalSummary)
            {
                cutsceneAdvanceRequested = false;
                yield return null;
                PlaySfx(SfxType.Dash);
                topSceneCounter.text = $"🏆 FINAL — ESCENA 4 DE 8  —  {nick}";
                subtitleSpeaker.text = "ESCENA 4 DE 8 — REENCUENTRO EN EL ATRIO CENTRAL";
                subtitleText.text =
                    $"¡Misión cumplida, {nick}! El equipo de agentes se reúne con el instructor del tutorial y los cuatro jefes de edificio.\n" +
                    "Todos los docentes y el personal universitario están a salvo gracias a tu intervención.";
                AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

                if (hero != null) hero.transform.position = new Vector3(98f, -2.05f, 0f);
                if (partner != null) partner.transform.position = new Vector3(95f, -2.05f, 0f);

                t = 0f;
                while (!skipToFinalSummary && !(t > 0.35f && (CheckCutsceneNextSceneInput() || GameInput.GetPauseDown())))
                {
                    t += Time.deltaTime;
                    if (cam != null)
                    {
                        cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(108.5f, 0.4f, -10f), Time.deltaTime * 4.5f);
                        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 5.5f, Time.deltaTime * 3f);
                    }

                    if (hero != null && heroAnim != null)
                    {
                        bool moving = Mathf.Abs(hero.transform.position.x - 105.2f) > 0.08f;
                        hero.transform.position = Vector3.MoveTowards(hero.transform.position, new Vector3(105.2f, -2.05f, 0f), 6.5f * Time.deltaTime);
                        heroAnim.SetBool("IsGrounded", true);
                        heroAnim.SetFloat("Speed", moving ? 8f : 0f);
                    }
                    if (partner != null && partnerAnim != null)
                    {
                        bool pMoving = Mathf.Abs(partner.transform.position.x - 102.0f) > 0.08f;
                        partner.transform.position = Vector3.MoveTowards(partner.transform.position, new Vector3(102.0f, -2.05f, 0f), 6.0f * Time.deltaTime);
                        partnerAnim.SetBool("IsGrounded", true);
                        partnerAnim.SetFloat("Speed", pMoving ? 8f : 0f);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        if (faculty[i] != null) AnimateSpriteFrames(facultySrs[i], humanIdleFrames, t + i * 0.2f, 7f);
                    }

                    subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                    yield return null;
                }
            }

            // =========================================================================================
            // ESCENA 5 DE 8: CEREMONIA DE CONDECORACIÓN UTNG (EL PREMIO PARA EL JUGADOR)
            // =========================================================================================
            if (!skipToFinalSummary)
            {
                cutsceneAdvanceRequested = false;
                yield return null;
                PlaySfx(SfxType.ChargeReady);
                topSceneCounter.text = $"🏆 FINAL — ESCENA 5 DE 8  —  {nick}";
                subtitleSpeaker.text = "ESCENA 5 DE 8 — CEREMONIA DE RECONOCIMIENTO UTNG";
                subtitleText.text =
                    $"Por su valentía al liberar cada laboratorio sin dañar a ningún docente, la UTNG entrega a {nick} el máximo galardón:\n" +
                    "★ ¡LA MEDALLA AL MÉRITO CIBERNÉTICO UTNG! ★";
                AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

                if (hero != null) hero.transform.position = new Vector3(105.2f, -2.05f, 0f);
                if (heroAnim != null) heroAnim.SetFloat("Speed", 0f);
                if (awardGlow != null) { awardGlow.SetActive(true); awardGlow.transform.position = new Vector3(105.2f, 0.8f, 0f); }
                if (medalObj != null) { medalObj.SetActive(true); medalObj.transform.position = new Vector3(105.2f, 4.6f, 0f); }
                flashImg.color = new Color(1.0f, 0.88f, 0.35f, 0.42f * flashScale);

                t = 0f;
                while (!skipToFinalSummary && !(t > 0.35f && (CheckCutsceneNextSceneInput() || GameInput.GetPauseDown())))
                {
                    t += Time.deltaTime;
                    flashImg.color = Color.Lerp(flashImg.color, Color.clear, Time.deltaTime * 4f);
                    if (cam != null)
                    {
                        cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(107.0f, 0.6f, -10f), Time.deltaTime * 4f);
                        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 4.6f, Time.deltaTime * 3f);
                    }

                    if (faculty[0] != null)
                    {
                        bool stepping = Mathf.Abs(faculty[0].transform.position.x - 107.4f) > 0.08f;
                        faculty[0].transform.position = Vector3.MoveTowards(faculty[0].transform.position, new Vector3(107.4f, -2.05f, 0f), 2.5f * Time.deltaTime);
                        AnimateSpriteFrames(facultySrs[0], stepping ? humanRunFrames : humanIdleFrames, t, stepping ? 10f : 7f);
                    }
                    for (int i = 1; i < 4; i++)
                    {
                        if (faculty[i] != null) AnimateSpriteFrames(facultySrs[i], humanIdleFrames, t + i * 0.2f, 7f);
                    }

                    if (medalObj != null)
                    {
                        float descT = Mathf.Clamp01(t / 1.4f);
                        float hoverY = Mathf.Lerp(4.6f, 1.95f, Mathf.SmoothStep(0f, 1f, descT)) + Mathf.Sin(t * 3.2f) * 0.12f;
                        medalObj.transform.position = new Vector3(105.2f, hoverY, 0f);
                        float pulse = 1.0f + Mathf.Sin(t * 4f) * 0.08f;
                        medalObj.transform.localScale = new Vector3(1.5f * pulse, 1.5f * pulse, 1f);
                    }
                    if (awardGlow != null)
                    {
                        float g = 2.6f + Mathf.Sin(t * 3f) * 0.3f;
                        awardGlow.transform.localScale = new Vector3(g, g, 1f);
                    }

                    subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                    yield return null;
                }
            }

            // =========================================================================================
            // ESCENA 6 DE 8: AMANECER SOBRE EL CAMPUS Y LA CIUDAD
            // =========================================================================================
            if (!skipToFinalSummary)
            {
                cutsceneAdvanceRequested = false;
                yield return null;
                PlaySfx(SfxType.ChargeReady);
                topSceneCounter.text = $"🏆 FINAL — ESCENA 6 DE 8  —  {nick}";
                subtitleSpeaker.text = "ESCENA 6 DE 8 — NUEVO AMANECER EN LA UTNG";
                subtitleText.text =
                    "Las compuertas de contención se abren hacia el horizonte mientras sale el sol sobre el campus.\n" +
                    $"La energía regresa a la normalidad. La ciudad vuelve a respirar tranquila gracias a {nick}.";
                AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

                t = 0f;
                while (!skipToFinalSummary && !(t > 0.35f && (CheckCutsceneNextSceneInput() || GameInput.GetPauseDown())))
                {
                    t += Time.deltaTime;
                    if (cam != null)
                    {
                        float panT = Mathf.Clamp01(t / 4.0f);
                        Vector3 targetCam = new Vector3(Mathf.Lerp(142f, 146f, panT), Mathf.Lerp(0.8f, 3.8f, panT), -10f);
                        cam.transform.position = Vector3.Lerp(cam.transform.position, targetCam, Time.deltaTime * 3.5f);
                        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 6.4f, Time.deltaTime * 2.5f);
                    }

                    subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                    yield return null;
                }
            }

            // =========================================================================================
            // ESCENA 7 DE 8: REPORTE FINAL DE MISIÓN UTNG (RESUMEN DEL JUGADOR Y MEDALLA)
            // =========================================================================================
            if (cam != null) { cam.transform.position = new Vector3(145f, 3.2f, -10f); cam.orthographicSize = 6.4f; }
            cutsceneAdvanceRequested = false;
            yield return null;
            topSceneCounter.text = $"🏆 FINAL — ESCENA 7 DE 8  —  {nick}";
            subtitleSpeaker.text = "ESCENA 7 DE 8 — REPORTE OFICIAL DE MISIÓN";
            subtitleText.text = $"Revisa el resumen oficial de tu hazaña, {nick}. Pulsa ESPACIO para ver los créditos finales.";
            if (nextStepBtnText != null) nextStepBtnText.text = "CRÉDITOS ▶";
            AnimatePanelShow(bottomCard, bottomCardRect, new Vector2(0f, -35f), 0.28f);

            GameObject reportCard = CreateGlassCard(cinemaRoot.transform, "Ending_Report_GlassCard",
                new Vector2(0.14f, 0.30f), new Vector2(0.86f, 0.87f),
                new Color(0.02f, 0.06f, 0.14f, 0.94f), new Color(1.0f, 0.84f, 0.30f, 0.98f));

            CreateDynamicText(reportCard.transform, "Rep_Header", "★  MISIÓN COMPLETADA — CAMPUS UTNG LIBERADO  ★", 20,
                new Vector2(0.04f, 0.84f), new Vector2(0.96f, 0.96f), new Color(1.0f, 0.86f, 0.32f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            // Retrato del héroe
            GameObject repHeroFrame = CreateGlassCard(reportCard.transform, "Rep_Hero_Frame",
                new Vector2(0.05f, 0.22f), new Vector2(0.30f, 0.80f),
                new Color(0.02f, 0.05f, 0.11f, 0.94f), new Color(0.25f, 0.95f, 1.0f, 0.85f));
            GameObject repHeroImgObj = new GameObject("Rep_Hero_Portrait");
            repHeroImgObj.transform.SetParent(repHeroFrame.transform, false);
            StretchFullScreen(repHeroImgObj);
            Image repHeroImg = repHeroImgObj.AddComponent<Image>();
            repHeroImg.preserveAspect = true;
            if (playableCharacterIdleSprites != null && selectedChar < playableCharacterIdleSprites.Length)
                repHeroImg.sprite = playableCharacterIdleSprites[selectedChar];

            // Medalla UTNG
            GameObject repMedalFrame = CreateGlassCard(reportCard.transform, "Rep_Medal_Frame",
                new Vector2(0.70f, 0.22f), new Vector2(0.95f, 0.80f),
                new Color(0.09f, 0.06f, 0.02f, 0.94f), new Color(1.0f, 0.82f, 0.25f, 0.92f));
            GameObject repMedalImgObj = new GameObject("Rep_Medal_Img");
            repMedalImgObj.transform.SetParent(repMedalFrame.transform, false);
            RectTransform rtMed = repMedalImgObj.AddComponent<RectTransform>();
            rtMed.anchorMin = new Vector2(0.18f, 0.28f);
            rtMed.anchorMax = new Vector2(0.82f, 0.86f);
            rtMed.offsetMin = Vector2.zero;
            rtMed.offsetMax = Vector2.zero;
            Image repMedalImg = repMedalImgObj.AddComponent<Image>();
            repMedalImg.preserveAspect = true;
            if (endingMedalSprite != null) repMedalImg.sprite = endingMedalSprite;
            repMedalImg.color = new Color(1.0f, 0.90f, 0.38f, 1f);
            CreateDynamicText(repMedalFrame.transform, "Rep_Medal_Label", "MEDALLA AL MÉRITO\nCIBERNÉTICO UTNG", 13,
                new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.26f), new Color(1.0f, 0.88f, 0.35f, 1f));

            string statsBody =
                $"👤 AGENTE LIBERADOR:  <color=#40E0D0><b>{nick}</b></color>\n\n" +
                $"🥷 PERSONAJE:  {CharacterNames[selectedChar]}\n\n" +
                $"🏢 SECTORES ASEGURADOS:  4 / 4  (Laboratorio, Redes, Servidores y Núcleo)\n\n" +
                $"⚔ DIFICULTAD:  {GameSettings.DifficultyDisplayName}\n\n" +
                $"♥ VIDAS RESTANTES AL COMPLETAR:  {savedLives} / {GameSettings.StartingLives}";
            CreateDynamicText(reportCard.transform, "Rep_Body", statsBody, 15,
                new Vector2(0.32f, 0.12f), new Vector2(0.68f, 0.80f), Color.white, TextAnchor.MiddleLeft);

            AnimatePanelShow(reportCard, reportCard.GetComponent<RectTransform>(), new Vector2(0f, -45f), 0.36f);

            t = 0f;
            while (!(t > 0.35f && CheckCutsceneNextSceneInput()))
            {
                t += Time.deltaTime;
                subtitlePrompt.color = Color.Lerp(new Color(1f, 0.85f, 0.25f, 0.65f), new Color(0.35f, 1f, 0.85f, 1f), Mathf.PingPong(t * 2.2f, 1f));
                yield return null;
            }
            Destroy(reportCard);

            // =========================================================================================
            // ESCENA 8 DE 8: EPÍLOGO Y CRÉDITOS CINEMÁTICOS CON GUARDADO FINAL Y MENÚ PRINCIPAL
            // =========================================================================================
            cutsceneAdvanceRequested = false;
            bottomCard.SetActive(false);
            topSceneCounter.text = $"🏆 FINAL — ESCENA 8 DE 8 — CRÉDITOS FINALES  •  {nick}";
            GameObject skipBtnObj = GameObject.Find("Btn_Skip_Ending");
            if (skipBtnObj != null) skipBtnObj.SetActive(false);

            GameObject creditsCard = CreateGlassCard(cinemaRoot.transform, "Ending_Credits_GlassCard",
                new Vector2(0.14f, 0.04f), new Vector2(0.86f, 0.87f),
                new Color(0.02f, 0.05f, 0.12f, 0.93f), new Color(0.28f, 0.96f, 1.0f, 0.95f));

            CreateDynamicText(creditsCard.transform, "EC_Title", "CÓDIGO DE EMERGENCIA — CRÉDITOS FINALES", 22,
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f), new Color(0.30f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            GameObject viewport = new GameObject("Credits_Mask_Viewport");
            viewport.transform.SetParent(creditsCard.transform, false);
            RectTransform rtView = viewport.AddComponent<RectTransform>();
            rtView.anchorMin = new Vector2(0.06f, 0.24f);
            rtView.anchorMax = new Vector2(0.94f, 0.86f);
            rtView.offsetMin = Vector2.zero;
            rtView.offsetMax = Vector2.zero;
            viewport.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            string endingPrefix =
                $"★  ¡GRACIAS POR JUGAR, {nick.ToUpper()}!  ★\n\n" +
                "LOGRO OBTENIDO:\n" +
                "Medalla al Mérito Cibernético UTNG\n\n" +
                "HISTORIA COMPLETADA:\n" +
                "Tutorial  •  Nivel 1: Laboratorio de Cómputo\n" +
                "Nivel 2: Centro de Redes  •  Nivel 3: Torre de Servidores\n" +
                "Nivel 4: Núcleo Central S.I.A.";
            string scrollLines = BuildFullCreditsBody(endingPrefix) + "\n\n★  LA UTNG ESTÁ A SALVO.  ★";

            Text creditsScrollText = CreateDynamicText(viewport.transform, "Credits_Scroll_Text", scrollLines, 15,
                new Vector2(0.03f, -2.55f), new Vector2(0.97f, 0.05f), new Color(0.94f, 0.98f, 1.0f, 1f), TextAnchor.UpperCenter);
            creditsScrollText.lineSpacing = 1.16f;
            RectTransform rtScroll = creditsScrollText.GetComponent<RectTransform>();

            endingSaveNotificationText = CreateDynamicText(creditsCard.transform, "EC_Save_Notify", "", 15,
                new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.23f), new Color(0.35f, 1.0f, 0.78f, 1f));

            CreateGlassButton(creditsCard.transform, "Btn_Ending_Save", "💾  GUARDAR PARTIDA COMPLETADA",
                new Vector2(0.08f, 0.04f), new Vector2(0.49f, 0.145f),
                new Color(0.06f, 0.48f, 0.38f, 0.96f), new Color(0.25f, 1.0f, 0.72f, 1f),
                () =>
                {
                    isSavingFromVictory = false;
                    isSavingFromEnding = true;
                    RefreshSavedGamesSubPanel();
                    if (pauseSaveSlotsPanel != null)
                    {
                        pauseSaveSlotsPanel.transform.SetAsLastSibling();
                        AnimatePanelShow(pauseSaveSlotsPanel, pauseSaveSlotsCardRect, new Vector2(0f, -45f), 0.28f);
                    }
                });

            CreateGlassButton(creditsCard.transform, "Btn_Ending_MainMenu", "🏠  MENÚ PRINCIPAL",
                new Vector2(0.52f, 0.04f), new Vector2(0.92f, 0.145f),
                new Color(0.10f, 0.28f, 0.50f, 0.96f), new Color(0.35f, 0.92f, 1.0f, 1f),
                () => LoadSceneWithAnimatedTransition("MainMenu"));

            AnimatePanelShow(creditsCard, creditsCard.GetComponent<RectTransform>(), new Vector2(0f, -45f), 0.40f);

            float scrollElapsed = 0f;
            while (true)
            {
                scrollElapsed += Time.deltaTime;
                if (rtScroll.anchorMin.y < 0.08f)
                {
                    float dy = Time.deltaTime * 0.085f;
                    rtScroll.anchorMin = new Vector2(rtScroll.anchorMin.x, rtScroll.anchorMin.y + dy);
                    rtScroll.anchorMax = new Vector2(rtScroll.anchorMax.x, rtScroll.anchorMax.y + dy);
                }
                yield return null;
            }
        }

        // =================================================================================================
        // PANTALLA DE CARGA DE 3 SEGUNDOS Y UTILIDADES DE COMPONENTES GLASSMORPHISM
        // =================================================================================================

        private void EnsureLoadingScreenUI()
        {
            if (loadingScreenPanel != null) return;

            loadingScreenPanel = new GameObject("Custom_3Sec_Loading_Screen");
            loadingScreenPanel.transform.SetParent(transform, false);
            StretchFullScreen(loadingScreenPanel);

            Image bg = loadingScreenPanel.AddComponent<Image>();
            bg.color = new Color(0.01f, 0.02f, 0.05f, 0.96f);

            GameObject card = CreateGlassCard(loadingScreenPanel.transform, "Loading_GlassCard",
                new Vector2(0.18f, 0.22f), new Vector2(0.82f, 0.78f),
                new Color(0.02f, 0.05f, 0.13f, 0.95f), new Color(0.25f, 0.95f, 1.0f, 0.94f));

            CreateDynamicText(card.transform, "Loading_Header", "⚡ CARGANDO ⚡", 24,
                new Vector2(0.06f, 0.76f), new Vector2(0.94f, 0.92f), new Color(0.25f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            GameObject sparkObj = new GameObject("Loading_Animated_Spark");
            sparkObj.transform.SetParent(card.transform, false);
            RectTransform rtSpark = sparkObj.AddComponent<RectTransform>();
            rtSpark.anchorMin = new Vector2(0.42f, 0.42f);
            rtSpark.anchorMax = new Vector2(0.58f, 0.72f);
            rtSpark.offsetMin = Vector2.zero;
            rtSpark.offsetMax = Vector2.zero;
            loadingSparkImage = sparkObj.AddComponent<Image>();
            loadingSparkImage.preserveAspect = true;
            loadingSparkImage.color = new Color(0.30f, 0.95f, 1.0f, 1f);
            if (electricSparkSprites != null && electricSparkSprites.Length > 0 && electricSparkSprites[0] != null)
            {
                loadingSparkImage.sprite = electricSparkSprites[0];
            }

            loadingStatusText = CreateDynamicText(card.transform, "Loading_Status", "INICIANDO...", 16,
                new Vector2(0.06f, 0.26f), new Vector2(0.94f, 0.40f), new Color(0.90f, 0.96f, 1.0f, 1f));

            GameObject barBg = new GameObject("Loading_Bar_BG");
            barBg.transform.SetParent(card.transform, false);
            RectTransform rtBarBg = barBg.AddComponent<RectTransform>();
            rtBarBg.anchorMin = new Vector2(0.10f, 0.12f);
            rtBarBg.anchorMax = new Vector2(0.90f, 0.22f);
            rtBarBg.offsetMin = Vector2.zero;
            rtBarBg.offsetMax = Vector2.zero;
            Image imgBarBg = barBg.AddComponent<Image>();
            if (uiBarBgSprite != null)
            {
                imgBarBg.sprite = uiBarBgSprite;
                imgBarBg.type = Image.Type.Sliced;
            }
            imgBarBg.color = new Color(0.08f, 0.14f, 0.24f, 0.95f);

            GameObject barFill = new GameObject("Loading_Bar_Fill");
            barFill.transform.SetParent(barBg.transform, false);
            RectTransform rtBarFill = barFill.AddComponent<RectTransform>();
            rtBarFill.anchorMin = new Vector2(0.01f, 0.12f);
            rtBarFill.anchorMax = new Vector2(0.99f, 0.88f);
            rtBarFill.offsetMin = Vector2.zero;
            rtBarFill.offsetMax = Vector2.zero;
            loadingProgressBarFill = barFill.AddComponent<Image>();
            if (uiBarFillSprite != null) loadingProgressBarFill.sprite = uiBarFillSprite;
            loadingProgressBarFill.type = Image.Type.Filled;
            loadingProgressBarFill.fillMethod = Image.FillMethod.Horizontal;
            loadingProgressBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            loadingProgressBarFill.fillAmount = 0f;
            loadingProgressBarFill.color = new Color(0.15f, 0.95f, 0.72f, 1f);

            if (uiBarBorderSprite != null)
            {
                GameObject barBorder = new GameObject("Loading_Bar_Border");
                barBorder.transform.SetParent(barBg.transform, false);
                StretchFullScreen(barBorder);
                Image imgBorder = barBorder.AddComponent<Image>();
                imgBorder.sprite = uiBarBorderSprite;
                imgBorder.color = new Color(0.70f, 0.96f, 1.0f, 0.85f);
                imgBorder.raycastTarget = false;
            }

            loadingScreenPanel.SetActive(false);
        }

        private void CreatePlaceholderSceneBanner(string sceneName)
        {
            GameObject darkBg = new GameObject("Placeholder_Dark_Backdrop");
            darkBg.transform.SetParent(transform, false);
            StretchFullScreen(darkBg);
            Image imgDark = darkBg.AddComponent<Image>();
            imgDark.color = new Color(0.01f, 0.02f, 0.06f, 0.78f);

            GameObject card = CreateGlassCard(darkBg.transform, "Placeholder_Sector_GlassBanner",
                new Vector2(0.20f, 0.24f), new Vector2(0.80f, 0.76f),
                new Color(0.02f, 0.05f, 0.13f, 0.94f), new Color(0.25f, 0.95f, 1.0f, 0.94f));

            CreateDynamicText(card.transform, "PH_Title", $"NIVEL: {sceneName.ToUpper()}", 24,
                new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.90f), new Color(0.25f, 0.96f, 1.0f, 1f), TextAnchor.MiddleCenter, useHeaderFont: true);

            CreateDynamicText(card.transform, "PH_Desc",
                "Este nivel se encuentra en desarrollo.\n" +
                "Puedes abrir el Menú de Pausa o volver al Menú Principal.",
                17, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.68f), Color.white);

            CreateGlassButton(card.transform, "Btn_PH_Pause", "⏸  MENÚ DE PAUSA",
                new Vector2(0.10f, 0.12f), new Vector2(0.48f, 0.28f),
                new Color(0.08f, 0.36f, 0.54f, 0.94f), new Color(0.30f, 0.92f, 1.0f, 1f),
                () => TogglePauseState());

            CreateGlassButton(card.transform, "Btn_PH_MainMenu", "🏠  MENÚ PRINCIPAL",
                new Vector2(0.52f, 0.12f), new Vector2(0.90f, 0.28f),
                new Color(0.42f, 0.10f, 0.18f, 0.94f), new Color(1.0f, 0.42f, 0.52f, 1f),
                () => LoadSceneWithAnimatedTransition("MainMenu"));

            AnimatePanelShow(darkBg, card.GetComponent<RectTransform>(), new Vector2(0f, -50f), 0.38f);
        }

        private GameObject CreateGlassCard(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color glassTint, Color borderAccent)
        {
            GameObject card = new GameObject(name);
            card.transform.SetParent(parent, false);
            RectTransform rt = card.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Capa 1: Base uniforme de cristal oscuro esmerilado (sin división a la mitad)
            Image baseImg = card.AddComponent<Image>();
            if (glassCardSprite != null)
            {
                baseImg.sprite = glassCardSprite;
                baseImg.type = Image.Type.Sliced;
            }
            baseImg.color = glassTint;

            Shadow dropShadow = card.AddComponent<Shadow>();
            dropShadow.effectColor = new Color(0f, 0f, 0f, 0.82f);
            dropShadow.effectDistance = new Vector2(0f, -10f);

            // Capa 2: Línea luminosa superior de cristal (Top Specular Edge)
            GameObject topEdge = new GameObject("Glass_Top_Border_Glow");
            topEdge.transform.SetParent(card.transform, false);
            RectTransform rtEdge = topEdge.AddComponent<RectTransform>();
            rtEdge.anchorMin = new Vector2(0.015f, 0.984f);
            rtEdge.anchorMax = new Vector2(0.985f, 0.998f);
            rtEdge.offsetMin = Vector2.zero;
            rtEdge.offsetMax = Vector2.zero;
            Image imgEdge = topEdge.AddComponent<Image>();
            imgEdge.color = borderAccent;
            imgEdge.raycastTarget = false;

            // Capa 3: Línea luminosa inferior de cristal
            GameObject botEdge = new GameObject("Glass_Bottom_Border_Glow");
            botEdge.transform.SetParent(card.transform, false);
            RectTransform rtBot = botEdge.AddComponent<RectTransform>();
            rtBot.anchorMin = new Vector2(0.04f, 0.003f);
            rtBot.anchorMax = new Vector2(0.96f, 0.013f);
            rtBot.offsetMin = Vector2.zero;
            rtBot.offsetMax = Vector2.zero;
            Image imgBot = botEdge.AddComponent<Image>();
            imgBot.color = new Color(borderAccent.r, borderAccent.g, borderAccent.b, borderAccent.a * 0.55f);
            imgBot.raycastTarget = false;

            return card;
        }

        private GameObject CreateGlassButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Color glassColor, Color accentColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = btnObj.AddComponent<Image>();
            Sprite btnSprite = uiButtonSprite != null ? uiButtonSprite : glassCardSprite;
            if (btnSprite != null)
            {
                img.sprite = btnSprite;
                img.type = Image.Type.Sliced;
            }
            img.color = glassColor;

            GameObject leftBar = new GameObject("Accent_Bar");
            leftBar.transform.SetParent(btnObj.transform, false);
            RectTransform rtBar = leftBar.AddComponent<RectTransform>();
            rtBar.anchorMin = new Vector2(0.01f, 0.14f);
            rtBar.anchorMax = new Vector2(0.022f, 0.86f);
            rtBar.offsetMin = Vector2.zero;
            rtBar.offsetMax = Vector2.zero;
            Image imgBar = leftBar.AddComponent<Image>();
            imgBar.color = accentColor;
            imgBar.raycastTarget = false;

            Button btn = btnObj.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
            cb.pressedColor = new Color(0.82f, 0.92f, 1.0f, 1f);
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            CreateDynamicText(btnObj.transform, "Label", label, 16, new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.95f), Color.white);
            return btnObj;
        }

        private Slider CreateGlassVolumeSlider(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float initialValue, UnityEngine.Events.UnityAction<float> onValueChanged)
        {
            GameObject sliderRoot = new GameObject(name);
            sliderRoot.transform.SetParent(parent, false);
            RectTransform rtRoot = sliderRoot.AddComponent<RectTransform>();
            rtRoot.anchorMin = anchorMin;
            rtRoot.anchorMax = anchorMax;
            rtRoot.offsetMin = Vector2.zero;
            rtRoot.offsetMax = Vector2.zero;

            GameObject bgTrack = new GameObject("Background_Track");
            bgTrack.transform.SetParent(sliderRoot.transform, false);
            RectTransform rtBg = bgTrack.AddComponent<RectTransform>();
            rtBg.anchorMin = new Vector2(0f, 0.25f);
            rtBg.anchorMax = new Vector2(1f, 0.75f);
            rtBg.offsetMin = Vector2.zero;
            rtBg.offsetMax = Vector2.zero;
            Image imgBg = bgTrack.AddComponent<Image>();
            if (uiBarBgSprite != null)
            {
                imgBg.sprite = uiBarBgSprite;
                imgBg.type = Image.Type.Sliced;
            }
            imgBg.color = new Color(0.08f, 0.14f, 0.24f, 0.95f);

            GameObject fillArea = new GameObject("Fill_Area");
            fillArea.transform.SetParent(sliderRoot.transform, false);
            RectTransform rtFillArea = fillArea.AddComponent<RectTransform>();
            rtFillArea.anchorMin = new Vector2(0.01f, 0.28f);
            rtFillArea.anchorMax = new Vector2(0.99f, 0.72f);
            rtFillArea.offsetMin = Vector2.zero;
            rtFillArea.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform rtFill = fill.AddComponent<RectTransform>();
            rtFill.anchorMin = Vector2.zero;
            rtFill.anchorMax = Vector2.one;
            rtFill.offsetMin = Vector2.zero;
            rtFill.offsetMax = Vector2.zero;
            Image imgFill = fill.AddComponent<Image>();
            if (uiBarFillSprite != null) imgFill.sprite = uiBarFillSprite;
            imgFill.color = new Color(0.18f, 0.92f, 1.0f, 0.95f);

            GameObject handleArea = new GameObject("Handle_Slide_Area");
            handleArea.transform.SetParent(sliderRoot.transform, false);
            RectTransform rtHandleArea = handleArea.AddComponent<RectTransform>();
            rtHandleArea.anchorMin = new Vector2(0.02f, 0f);
            rtHandleArea.anchorMax = new Vector2(0.98f, 1f);
            rtHandleArea.offsetMin = Vector2.zero;
            rtHandleArea.offsetMax = Vector2.zero;

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            RectTransform rtHandle = handle.AddComponent<RectTransform>();
            rtHandle.sizeDelta = new Vector2(28f, 0f);
            Image imgHandle = handle.AddComponent<Image>();
            if (uiButtonSprite != null)
            {
                imgHandle.sprite = uiButtonSprite;
                imgHandle.type = Image.Type.Sliced;
            }
            imgHandle.color = new Color(0.45f, 1.0f, 0.85f, 1f);

            Slider slider = sliderRoot.AddComponent<Slider>();
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.fillRect = rtFill;
            slider.handleRect = rtHandle;
            slider.targetGraphic = imgHandle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.SetValueWithoutNotify(initialValue);
            slider.onValueChanged.AddListener(onValueChanged);

            return slider;
        }

        private RectTransform StretchFullScreen(GameObject obj)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        private Text CreateDynamicText(Transform parent, string name, string content, int fontSize, Vector2 anchorMin, Vector2 anchorMax, Color color, TextAnchor align = TextAnchor.MiddleCenter, bool useHeaderFont = false)
        {
            // Contenedor anclado exactamente a las coordenadas solicitadas
            GameObject containerObj = new GameObject(name);
            containerObj.transform.SetParent(parent, false);
            RectTransform rtContainer = containerObj.AddComponent<RectTransform>();
            rtContainer.anchorMin = anchorMin;
            rtContainer.anchorMax = anchorMax;
            rtContainer.offsetMin = Vector2.zero;
            rtContainer.offsetMax = Vector2.zero;

            // Hijo de texto super-muestreado a 2x (rasteriza al doble de resolución y escala a 0.5x para nitidez HD absoluta)
            GameObject txtObj = new GameObject("HD_Text_2x");
            txtObj.transform.SetParent(containerObj.transform, false);
            RectTransform rtTxt = txtObj.AddComponent<RectTransform>();
            rtTxt.anchorMin = new Vector2(-0.5f, -0.5f);
            rtTxt.anchorMax = new Vector2(1.5f, 1.5f);
            rtTxt.pivot = new Vector2(0.5f, 0.5f);
            rtTxt.offsetMin = Vector2.zero;
            rtTxt.offsetMax = Vector2.zero;
            rtTxt.localScale = new Vector3(0.5f, 0.5f, 1f);

            Text txt = txtObj.AddComponent<Text>();
            Font hdFont = customUiFont != null ? customUiFont : cleanBodyFont;
            if (hdFont != null) txt.font = hdFont;
            txt.text = content;
            txt.fontSize = Mathf.Max(24, (fontSize + 2) * 2);
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = align;
            txt.color = color;
            txt.lineSpacing = 1.08f;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;

            Shadow crispShadow = txtObj.AddComponent<Shadow>();
            crispShadow.effectColor = new Color(0f, 0f, 0f, 0.92f);
            crispShadow.effectDistance = new Vector2(2.2f, -2.2f);

            return txt;
        }

        private void EnsureEventSystemExists()
        {
            EventSystem existing = FindAnyObjectByType<EventSystem>();
            if (existing == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<InputSystemUIInputModule>();
            }
            else if (existing.GetComponent<BaseInputModule>() == null)
            {
                existing.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        // =================================================================================================
        // ACTUALIZACIÓN DE TELEMETRÍA Y REPRODUCCIÓN DE AUDIO
        // =================================================================================================

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

            bgmSource.loop = (bgmPlaylist == null || bgmPlaylist.Length <= 1);
            bgmSource.playOnAwake = false;
            bgmSource.volume = musicVolume * 0.42f;

            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;

            if (bgmPlaylist != null && bgmPlaylist.Length > 0)
            {
                StartCoroutine(DynamicPlaylistLoopRoutine());
            }
            else if (bgmActionTrack != null && !bgmSource.isPlaying)
            {
                bgmSource.clip = bgmActionTrack;
                bgmSource.Play();
            }
        }

        private IEnumerator DynamicPlaylistLoopRoutine()
        {
            if (bgmPlaylist == null || bgmPlaylist.Length == 0) yield break;

            while (true)
            {
                AudioClip track = bgmPlaylist[currentPlaylistTrackIndex % bgmPlaylist.Length];
                if (track != null)
                {
                    bgmSource.clip = track;
                    bgmSource.volume = musicVolume * 0.42f;
                    bgmSource.Play();

                    float playTime = Mathf.Max(12f, track.length - 1.5f);
                    float elapsed = 0f;
                    while (elapsed < playTime)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        bgmSource.volume = musicVolume * 0.42f;
                        yield return null;
                    }
                }
                currentPlaylistTrackIndex = (currentPlaylistTrackIndex + 1) % bgmPlaylist.Length;
            }
        }

        private void PlaySfx(SfxType type)
        {
            if (sfxSource == null) return;

            AudioClip clip = null;
            float volumeScale = 0.85f;
            float targetPitch = 1.0f;

            switch (type)
            {
                case SfxType.ShootNormal:
                    clip = sfxShootNormal;
                    volumeScale = 0.88f * sfxVolume;
                    break;
                case SfxType.ShootCharged:
                    clip = sfxShootCharged;
                    volumeScale = 1.0f * sfxVolume;
                    break;
                case SfxType.ChargeReady:
                    clip = sfxChargeReady;
                    volumeScale = 0.65f * sfxVolume;
                    break;
                case SfxType.MeleeSlash:
                    clip = sfxMeleeSlash;
                    volumeScale = 0.96f * sfxVolume;
                    break;
                case SfxType.Jump:
                    if (!jumpSfxEnabled) return;
                    clip = sfxJump;
                    volumeScale = 0.70f * envVolume;
                    int selectedChar = PlayerPrefs.GetInt("CDE_SelectedCharacterIndex", 0);
                    // Distinción de audio de salto por género de personaje:
                    // Mujer Kunoichi (0): Tono ágil agudo acrobático (1.25x)
                    // Hombre Shinobi (1): Tono marcial grave sólido (0.92x)
                    targetPitch = (selectedChar == 0) ? 1.25f : 0.92f;
                    break;
                case SfxType.Dash:
                    clip = sfxDash;
                    volumeScale = 0.82f * envVolume;
                    break;
                case SfxType.PlayerHurt:
                    clip = sfxPlayerHurt;
                    volumeScale = 0.95f * envVolume;
                    break;
                case SfxType.PlayerDead:
                    clip = sfxPlayerDead;
                    volumeScale = 1.0f * envVolume;
                    break;
                case SfxType.EnemyHit:
                    clip = sfxEnemyHit;
                    volumeScale = 0.75f * sfxVolume;
                    break;
                case SfxType.EnemyDestroyed:
                    clip = sfxEnemyDestroyed;
                    volumeScale = 0.85f * sfxVolume;
                    break;
                case SfxType.BossCast:
                    clip = sfxBossCast;
                    volumeScale = 0.85f * sfxVolume;
                    break;
                case SfxType.Checkpoint:
                    // Sonido de checkpoint/confirmación eliminado por diseño para mantener un aviso visual discreto
                    return;
                case SfxType.GateClose:
                    clip = sfxGateClose;
                    volumeScale = 1.0f * envVolume;
                    targetPitch = 0.82f;
                    break;
            }

            if (clip != null)
            {
                sfxSource.pitch = targetPitch;
                sfxSource.PlayOneShot(clip, volumeScale);
                sfxSource.pitch = 1.0f;
            }
        }

        private void HandleCheckpointReached(int index)
        {
            if (checkpointStatusText != null)
            {
                checkpointStatusText.text = $"CHECKPOINT: {index}/8";
                checkpointStatusText.color = new Color(0.2f, 1.0f, 0.55f, 1.0f);
            }
            if (checkpointToastCoroutine != null) StopCoroutine(checkpointToastCoroutine);
            checkpointToastCoroutine = StartCoroutine(ShowCheckpointToastRoutine(index));
            SyncTutorialStepToPlayerPosition();
        }

        private void UpdateLivesUI(int currentLives, int maxLives)
        {
            if (playerLivesText == null) return;
            string hearts = "";
            for (int i = 0; i < maxLives; i++)
            {
                hearts += (i < currentLives) ? "♥ " : "♡ ";
            }
            playerLivesText.text = $"VIDAS: {currentLives}/{maxLives}  {hearts.TrimEnd()}";
            playerLivesText.color = currentLives > 1
                ? new Color(0.35f, 1.0f, 0.85f, 1.0f)
                : new Color(1.0f, 0.35f, 0.35f, 1.0f);
        }

        private void UpdatePlayerHealthUI(int current, int max)
        {
            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            if (playerHealthFill != null)
            {
                playerHealthFill.fillAmount = ratio;
                playerHealthFill.color = Color.Lerp(new Color(0.95f, 0.2f, 0.25f), new Color(0.15f, 0.95f, 0.5f), ratio);
            }

            if (playerHealthText != null)
            {
                playerHealthText.text = $"VIDA: {current} / {max}";
            }
        }

        private void UpdateWeaponDamageUI(int newDamage)
        {
            if (weaponDamageText != null)
            {
                int meleeDmg = playerShooting != null ? playerShooting.MeleeDamage : 30;
                weaponDamageText.text = $"PEM: {newDamage} DMG | KATANA: {meleeDmg}";
            }
        }

        private void UpdateBossBraceletUI(int current, int max, Transform targetTransform)
        {
            if (bossHealthContainer == null) return;

            bool isMainBoss = targetTransform != null && targetTransform.name.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!isMainBoss) return;

            bossHealthContainer.SetActive(true);
            bossHideTimer = 10.0f;

            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            if (bossHealthFill != null)
            {
                bossHealthFill.fillAmount = ratio;
            }

            if (bossTitleText != null)
            {
                bossTitleText.text = isTutorialScene
                    ? "MINI-JEFE DEL TUTORIAL"
                    : "JEFE DEL NIVEL";
            }

            if (bossHealthText != null)
            {
                bossHealthText.text = $"PULSERA: {current} / {max}";
            }
        }

        private void HandleBraceletOverloaded(Transform targetTransform)
        {
            if (bossHealthContainer == null) return;
            bool isMainBoss = targetTransform != null && targetTransform.name.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!isMainBoss) return;

            if (bossHealthFill != null) bossHealthFill.fillAmount = 0f;
            if (bossHealthText != null) bossHealthText.text = "DOCENTE LIBERADO";
            bossHideTimer = 5.0f;
        }
    }
}
