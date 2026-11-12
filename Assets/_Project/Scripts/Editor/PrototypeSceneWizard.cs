#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Player;
using CodigoDeEmergencia.Combat;
using CodigoDeEmergencia.Enemies;
using CodigoDeEmergencia.UI;

namespace CodigoDeEmergencia.EditorTools
{
    /// <summary>
    /// Asistente integral de Revisión 2 para generar y configurar las 7 escenas del proyecto:
    /// - MainMenu.unity (Primera escena al ejecutar el juego, diseño Glassmorphism con fondo oscurecido y transiciones suaves).
    /// - IntroCutscene.unity (Cinemática tipo VIDEO ANIMADO en 4 actos con cámara dinámica, acción de Katana/PEM y destransformación).
    /// - TutorialScene.unity (Issue #08: 7 estaciones con oscurecimiento/pausa + Mini-Jefe con destransformación Gigante -> Normal).
    /// - SampleScene.unity (Issue #05: Laboratorio de Cómputo 6x largo = 1,260m, 18 plantas, iluminación 2D sutil y calibrada,
    ///   objetos de fondo normalizados a escala real, plataformas sólidas y escaleras al ras desfasadas).
    /// - Level2Scene, Level3Scene, Level4Scene (Escenas base con Menú de Pausa Glassmorphism y deslizador de volumen).
    /// </summary>
    public static class PrototypeSceneWizard
    {
        private const string IndustrialRetinaFolder = "Assets/_Project/Sprites/World/Tileset_Industrial_Lab/PNG/Retina";
        private const string LabGeneratedFolder = "Assets/_Project/Sprites/World/Lab_Generated";
        private const string WenrexaUiFolder = "Assets/_Project/Sprites/UI/SciFi_Interface_Wenrexa/PNG";
        private const string KenneyUiFolder = "Assets/_Project/Sprites/UI/Buttons_And_Controls_Kenney/PNG";
        private const string AutoSceneRebuildKey = "CDE_Revision5_Progress8CPAssetsCredits_v11";

        [InitializeOnLoadMethod]
        private static void AutoRebuildSceneOnce()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (SessionState.GetBool(AutoSceneRebuildKey, false)) return;
            SessionState.SetBool(AutoSceneRebuildKey, true);

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                BuildAllRevision2ScenesSilent();
            };
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EnsureRevision2ScenesBuiltIfNeeded();
            }
        }

        private static void EnsureRevision2ScenesBuiltIfNeeded()
        {
            bool needsBuild = !System.IO.File.Exists("Assets/Scenes/MainMenu.unity")
                              || !System.IO.File.Exists("Assets/Scenes/IntroCutscene.unity")
                              || !System.IO.File.Exists("Assets/Scenes/EndingCutscene.unity")
                              || !System.IO.File.Exists("Assets/Scenes/TutorialScene.unity")
                              || !System.IO.File.Exists("Assets/_Project/Animations/Player/NinjaCyber_Idle.anim")
                              || !System.IO.File.Exists("Assets/_Project/Animations/Player/NinjaEmerald_Idle.anim")
                              || !System.IO.File.Exists("Assets/_Project/Audio/SFX/SFX_EMP_Pulse_Normal.wav")
                              || !System.IO.File.Exists("Assets/_Project/Audio/SFX/SFX_EMP_Pulse_Charged.wav")
                              || GameObject.Find("Player_Personal_Light") != null
                              || (SceneManager.GetActiveScene().name == "SampleScene"
                                  && (GameObject.Find("Boss_Arena_Gate_Structure") == null || GameObject.Find("Lab_Sign_Main_S0") == null || GameObject.Find("Level_Progress_Panel") == null || GameObject.Find("Checkpoint_8") == null))
                              || (SceneManager.GetActiveScene().name == "TutorialScene"
                                  && GameObject.Find("Level_Progress_Panel") == null);
            if (needsBuild)
            {
                BuildAllRevision2ScenesSilent();
            }
            else
            {
                EnsureMainMenuIsPlayModeStartScene();
            }
        }

        private static void EnsureMainMenuIsPlayModeStartScene()
        {
            SceneAsset mainMenuAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenu.unity");
            if (mainMenuAsset != null)
            {
                EditorSceneManager.playModeStartScene = mainMenuAsset;
            }
        }

        [MenuItem("Tools/Código de Emergencia/🚀 Montar Todas las Escenas (Revisión 2 Completa + Video Cinemática + Iluminación Calibrada)")]
        public static void SetupPrototypeScene()
        {
            BuildAllRevision2ScenesSilent();

            EditorUtility.DisplayDialog(
                "¡Revisión 2 Pulida y Actualizada con Éxito!",
                "Se han regenerado las 7 escenas del proyecto con todas las mejoras de calidad:\n\n" +
                "1. Iluminación 2D Calibrada y Objetos a Escala Real: Eliminado el resplandor exagerado; cada monitor, servidor y ventana tiene un halo sutil detrás del jugador y tamaño normalizado en metros.\n" +
                "2. Escaleras y Paracaídas: Bloqueados todos los ataques y el Dash mientras se sube/baja escaleras o se planea con el paracaídas energético.\n" +
                "3. IntroCutscene.unity: Cinemática tipo VIDEO ANIMADO automático en 4 actos (con profesores caminando, ataque de S.I.A., combate de la Ninja con Katana/PEM y destransformación).\n" +
                "4. Destransformación de Jefes: Primero pasa a Humano en tamaño Gigante (3.15x), luego reduce gradualmente su tamaño hasta el normal (1.18x), agradece y corre a la izquierda con animación de 6 cuadros.\n" +
                "5. Transiciones Suaves (Fade + Slide) y Fondo Oscurecido en todos los paneles Glassmorphism.",
                "¡Excelente!"
            );
        }

        [MenuItem("Tools/Código de Emergencia/🎬 Abrir Menú Principal (MainMenu)")]
        public static void OpenMainMenuScene()
        {
            if (System.IO.File.Exists("Assets/Scenes/MainMenu.unity"))
                EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            else
                BuildAllRevision2ScenesSilent();
        }

        [MenuItem("Tools/Código de Emergencia/📽️ Abrir Video Cinemática (IntroCutscene)")]
        public static void OpenIntroCutsceneScene()
        {
            if (System.IO.File.Exists("Assets/Scenes/IntroCutscene.unity"))
                EditorSceneManager.OpenScene("Assets/Scenes/IntroCutscene.unity");
            else
                BuildAllRevision2ScenesSilent();
        }

        [MenuItem("Tools/Código de Emergencia/🏆 Abrir Cinemática Final (EndingCutscene)")]
        public static void OpenEndingCutsceneScene()
        {
            if (System.IO.File.Exists("Assets/Scenes/EndingCutscene.unity"))
                EditorSceneManager.OpenScene("Assets/Scenes/EndingCutscene.unity");
            else
                BuildAllRevision2ScenesSilent();
        }

        [MenuItem("Tools/Código de Emergencia/🎓 Abrir Tutorial Interactivo (TutorialScene)")]
        public static void OpenTutorialScene()
        {
            if (System.IO.File.Exists("Assets/Scenes/TutorialScene.unity"))
                EditorSceneManager.OpenScene("Assets/Scenes/TutorialScene.unity");
            else
                BuildAllRevision2ScenesSilent();
        }

        [MenuItem("Tools/Código de Emergencia/🔬 Abrir Nivel 1: Laboratorio de Cómputo (SampleScene)")]
        public static void OpenLevel1Scene()
        {
            if (System.IO.File.Exists("Assets/Scenes/SampleScene.unity"))
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            else
                BuildAllRevision2ScenesSilent();
        }

        public static void BuildAllRevision2ScenesSilent()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            // 0. Generar sonidos realistas de combate y procesar sprites, animaciones y materiales físicos
            GenerateRealisticCombatAudioAssets();
            BatchSpriteSheetProcessor.ProcessAllSpriteSheetsSilent();
            PhysicsMaterial2D playerMat = GetOrCreateFrictionlessMaterial();
            GameObject pulsePrefab = CreateOrGetEMPPulsePrefab();
            GameObject smallEnemyPrefab = CreateOrGetSmallEnemyPrefab();

            // 1. Generar Escenas de Niveles 2, 3 y 4 con la misma estructura del Nivel 1 (tema y dificultad propios, +20% por nivel)
            BuildThemedLevelScene("Assets/Scenes/Level2Scene.unity", GetLevelTheme(2), pulsePrefab, smallEnemyPrefab, playerMat);
            BuildThemedLevelScene("Assets/Scenes/Level3Scene.unity", GetLevelTheme(3), pulsePrefab, smallEnemyPrefab, playerMat);
            BuildThemedLevelScene("Assets/Scenes/Level4Scene.unity", GetLevelTheme(4), pulsePrefab, smallEnemyPrefab, playerMat);
            activeTheme = GetLevelTheme(1);

            // 2. Generar Escenas Cinemáticas de Historia y Final del Juego
            BuildIntroCutsceneScene("Assets/Scenes/IntroCutscene.unity");
            BuildEndingCutsceneScene("Assets/Scenes/EndingCutscene.unity");

            // 3. Generar Escena del Tutorial Interactivo (Issue #08)
            BuildTutorialInteractiveScene("Assets/Scenes/TutorialScene.unity", pulsePrefab, smallEnemyPrefab, playerMat);

            // 4. Generar Escena del Nivel 1: Laboratorio de Cómputo 6x Largo x 18 Plantas (Issue #05)
            BuildThemedLevelScene("Assets/Scenes/SampleScene.unity", GetLevelTheme(1), pulsePrefab, smallEnemyPrefab, playerMat);
            activeTheme = GetLevelTheme(1);

            // 5. Generar Escena del Menú Principal AL FINAL para que quede abierta por defecto al iniciar el proyecto (Issue #07)
            BuildMainMenuScene("Assets/Scenes/MainMenu.unity", smallEnemyPrefab);

            // 6. Registrar todas las escenas en EditorBuildSettings (MainMenu en índice 0) y fijar MainMenu como escena de inicio al dar Play
            RegisterAllScenesInBuildSettings();
            EnsureMainMenuIsPlayModeStartScene();

            AssetDatabase.SaveAssets();
            Debug.Log("<color=cyan><b>[REVISIÓN 2 COMPLETA Y PULIDA]</b> Escenas MainMenu (inicio por defecto), IntroCutscene (Video), TutorialScene, SampleScene (Nivel 1 Lab), Level2Scene, Level3Scene y Level4Scene generadas con éxito.</color>");
        }

        private static void GenerateRealisticCombatAudioAssets()
        {
            const int sampleRate = 44100;
            const string sfxFolder = "Assets/_Project/Audio/SFX";
            if (!Directory.Exists(sfxFolder))
            {
                Directory.CreateDirectory(sfxFolder);
            }

            // 1. PULSO PEM NORMAL (Descarga táctica de banco capacitivo + golpe de bobina magnética + chispas corona)
            {
                float duration = 0.28f;
                int count = Mathf.RoundToInt(sampleRate * duration);
                float[] samples = new float[count];
                System.Random rng = new System.Random(41023);

                double coilPhase = 0.0;
                double ringPhase = 0.0;
                double mainsPhase = 0.0;
                float svfLow = 0f, svfBand = 0f;

                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / sampleRate;
                    double white = rng.NextDouble() * 2.0 - 1.0;

                    // Impacto electromagnético de bobina (185 Hz -> 42 Hz)
                    double coilFreq = 42.0 + 165.0 * Math.Exp(-t * 46.0);
                    coilPhase += 2.0 * Math.PI * coilFreq / sampleRate;
                    double coilThump = Math.Sin(coilPhase) * Math.Exp(-t * 21.0) * 0.72;

                    // Oscilación LC resonante corta (940 Hz -> 260 Hz)
                    double ringFreq = 260.0 + 680.0 * Math.Exp(-t * 30.0);
                    ringPhase += 2.0 * Math.PI * ringFreq / sampleRate;
                    double lcRing = Math.Sin(ringPhase) * Math.Exp(-t * 24.0) * 0.34;

                    // Zumbido breve de descarga eléctrica
                    mainsPhase += 2.0 * Math.PI * 120.0 / sampleRate;
                    double hum = (Math.Sin(mainsPhase) * 0.55 + Math.Sin(mainsPhase * 2.0) * 0.30) * Math.Exp(-t * 16.0) * 0.22;

                    // Arco dieléctrico inicial + micro-chispas SVF
                    double arcBurst = white * Math.Exp(-t * 145.0) * 1.15;
                    double sparkImpulse = (t > 0.008f && t < 0.145f && rng.NextDouble() < 0.022) ? (white * Math.Exp(-t * 14.0) * 0.85) : 0.0;
                    float inputNoise = (float)(arcBurst + sparkImpulse + white * Math.Exp(-t * 36.0) * 0.24);

                    float cutoffHz = Mathf.Lerp(5400f, 1850f, Mathf.Clamp01(t / 0.16f));
                    float f = Mathf.Clamp(2f * Mathf.Sin(Mathf.PI * cutoffHz / sampleRate), 0.02f, 0.86f);
                    float q = 0.32f;
                    float hp = inputNoise - svfLow - q * svfBand;
                    svfBand += f * hp;
                    svfLow += f * svfBand;

                    double combined = coilThump + lcRing + hum + svfBand * 0.68 + hp * Math.Exp(-t * 120.0) * 0.35;
                    samples[i] = (float)Math.Tanh(combined * 1.45);
                }

                WriteMono16BitWav($"{sfxFolder}/SFX_EMP_Pulse_Normal.wav", sampleRate, samples, 0.90f);
            }

            // 2. PULSO PEM CARGADO (Ruptura dieléctrica masiva + onda subgrave + transformador 60/120/240/360 Hz + colapso PEM)
            {
                float duration = 0.92f;
                int count = Mathf.RoundToInt(sampleRate * duration);
                float[] samples = new float[count];
                System.Random rng = new System.Random(88412);

                double subPhase = 0.0;
                double collapsePhase = 0.0;
                double humPhase60 = 0.0;
                float svfLow = 0f, svfBand = 0f;

                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / sampleRate;
                    double white = rng.NextDouble() * 2.0 - 1.0;

                    // Onda subgrave de descarga (118 Hz -> 27 Hz)
                    double subFreq = 27.0 + 96.0 * Math.Exp(-t * 20.0);
                    subPhase += 2.0 * Math.PI * subFreq / sampleRate;
                    double subPunch = Math.Sin(subPhase) * Math.Exp(-t * 5.8) * 0.82;

                    // Zumbido de transformador de alta tensión (60/120/240/360 Hz)
                    humPhase60 += 2.0 * Math.PI * 60.0 / sampleRate;
                    double hum60 = (Math.Sin(humPhase60) * 0.48
                                  + Math.Sin(humPhase60 * 2.0) * 0.40
                                  + Math.Sin(humPhase60 * 4.0) * 0.24
                                  + Math.Sin(humPhase60 * 6.0) * 0.14)
                                  * Math.Exp(-t * 3.6) * 0.42;

                    // Colapso progresivo del campo electromagnético (1650 Hz -> 140 Hz)
                    double colFreq = 140.0 + 1510.0 * Math.Exp(-t * 4.6);
                    collapsePhase += 2.0 * Math.PI * colFreq / sampleRate;
                    double fieldCollapse = Math.Sin(collapsePhase) * Math.Exp(-t * 4.2) * (t < 0.04f ? t / 0.04f : 1f) * 0.34;

                    // Chispas de corona eléctrica y ruptura inicial a través de filtro pasa-banda
                    double flash = white * Math.Exp(-t * 88.0) * 1.35;
                    double coronaArc = (t > 0.015f && t < 0.65f && rng.NextDouble() < 0.028)
                        ? white * Math.Exp(-t * 3.2) * 1.10
                        : 0.0;
                    float noiseFeed = (float)(flash + coronaArc + white * Math.Exp(-t * 10.5) * 0.18);

                    float cutoffHz = Mathf.Lerp(5200f, 1100f, Mathf.Clamp01(t / 0.68f));
                    float f = Mathf.Clamp(2f * Mathf.Sin(Mathf.PI * cutoffHz / sampleRate), 0.02f, 0.86f);
                    float q = 0.28f;
                    float hp = noiseFeed - svfLow - q * svfBand;
                    svfBand += f * hp;
                    svfLow += f * svfBand;

                    double combined = subPunch + hum60 + fieldCollapse + svfBand * 0.62 + hp * Math.Exp(-t * 90.0) * 0.40;
                    samples[i] = (float)Math.Tanh(combined * 1.60);
                }

                WriteMono16BitWav($"{sfxFolder}/SFX_EMP_Pulse_Charged.wav", sampleRate, samples, 0.92f);
            }

            // 3. CORTE REALISTA DE KATANA (Tachikaze aerodinámico + silbido de bo-hi + armónicos breves de acero forjado)
            {
                float duration = 0.34f;
                int count = Mathf.RoundToInt(sampleRate * duration);
                float[] samples = new float[count];
                System.Random rng = new System.Random(19507);

                float svfAirLow = 0f, svfAirBand = 0f;
                float svfWhistleLow = 0f, svfWhistleBand = 0f;
                double steelP1 = 0.0, steelP2 = 0.0, steelP3 = 0.0;
                double prevWhite = 0.0;

                for (int i = 0; i < count; i++)
                {
                    float t = (float)i / sampleRate;
                    double white = rng.NextDouble() * 2.0 - 1.0;
                    // Ruido levemente suavizado (color aire real, menos siseo digital crudo)
                    float pinkAir = (float)(white * 0.65 + prevWhite * 0.35);
                    prevWhite = white;

                    // Envolvente de velocidad del tajo (pico agudo en ~68 ms con cola rápida)
                    float attack = Mathf.Clamp01(t / 0.055f);
                    float velNorm = (t < 0.068f)
                        ? Mathf.Pow(t / 0.068f, 1.35f)
                        : Mathf.Exp(-(t - 0.068f) * 14.5f);
                    float airEnvelope = attack * (0.20f + 0.80f * velNorm);

                    // Tachikaze principal: barrido aerodinámico de frecuencia por velocidad de hoja (580 Hz -> 3250 Hz -> 780 Hz)
                    float airCutoff = 580f + 2670f * velNorm;
                    float fAir = Mathf.Clamp(2f * Mathf.Sin(Mathf.PI * airCutoff / sampleRate), 0.02f, 0.86f);
                    float qAir = 0.42f;
                    float hpAir = pinkAir - svfAirLow - qAir * svfAirBand;
                    svfAirBand += fAir * hpAir;
                    svfAirLow += fAir * svfAirBand;

                    // Silbido del canal bo-hi (resonancia estrecha 1100 Hz -> 2250 Hz -> 950 Hz)
                    float whistleCutoff = 950f + 1300f * velNorm;
                    float fW = Mathf.Clamp(2f * Mathf.Sin(Mathf.PI * whistleCutoff / sampleRate), 0.02f, 0.86f);
                    float qW = 0.16f;
                    float hpW = pinkAir - svfWhistleLow - qW * svfWhistleBand;
                    svfWhistleBand += fW * hpW;
                    svfWhistleLow += fW * svfWhistleBand;

                    // Brevísimo roce metálico del filo al desenvainar/cortar (0..55 ms)
                    steelP1 += 2.0 * Math.PI * 2940.0 / sampleRate;
                    steelP2 += 2.0 * Math.PI * 4280.0 / sampleRate;
                    steelP3 += 2.0 * Math.PI * 6180.0 / sampleRate;
                    double steelShimmer = (Math.Sin(steelP1) * 0.48 + Math.Sin(steelP2) * 0.34 + Math.Sin(steelP3) * 0.18)
                                        * Math.Exp(-t * 58.0) * 0.12;

                    // Impacto seco de aire comprimido en el quiebre de muñeca (~62 ms)
                    double wristSnap = Math.Sin(2.0 * Math.PI * 115.0 * t) * Math.Exp(-Math.Pow((t - 0.062) / 0.018, 2.0)) * 0.24;

                    double combined = (svfAirBand * 1.10 + svfWhistleBand * 0.34) * airEnvelope + steelShimmer + wristSnap;
                    samples[i] = (float)Math.Tanh(combined * 1.40);
                }

                WriteMono16BitWav($"{sfxFolder}/SFX_Katana_Slash.wav", sampleRate, samples, 0.90f);
            }
        }

        private static void WriteMono16BitWav(string assetPath, int sampleRate, float[] samples, float targetPeak)
        {
            if (samples == null || samples.Length == 0) return;

            float peak = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float a = Mathf.Abs(samples[i]);
                if (a > peak) peak = a;
            }
            float gain = peak > 0.0001f ? (targetPeak / peak) : 1f;

            int fadeInSamples = Mathf.Min(samples.Length / 10, Mathf.Max(16, sampleRate / 400));
            int fadeOutSamples = Mathf.Min(samples.Length / 4, Mathf.Max(64, sampleRate / 65));

            int dataBytes = samples.Length * 2;
            using (FileStream fs = new FileStream(assetPath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(36 + dataBytes);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16);
                bw.Write((short)1);
                bw.Write((short)1);
                bw.Write(sampleRate);
                bw.Write(sampleRate * 2);
                bw.Write((short)2);
                bw.Write((short)16);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(dataBytes);

                for (int i = 0; i < samples.Length; i++)
                {
                    float env = 1f;
                    if (i < fadeInSamples) env *= (float)i / fadeInSamples;
                    if (i >= samples.Length - fadeOutSamples)
                    {
                        float remain = (float)(samples.Length - 1 - i) / fadeOutSamples;
                        env *= Mathf.Clamp01(remain);
                    }

                    short s = (short)Mathf.Clamp(Mathf.RoundToInt(samples[i] * gain * env * 32767f), -32767, 32767);
                    bw.Write(s);
                }
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        public static void SetupPrototypeSceneSilent()
        {
            BuildAllRevision2ScenesSilent();
        }

        private static void RegisterAllScenesInBuildSettings()
        {
            string[] scenePaths = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/IntroCutscene.unity",
                "Assets/Scenes/TutorialScene.unity",
                "Assets/Scenes/SampleScene.unity",
                "Assets/Scenes/Level2Scene.unity",
                "Assets/Scenes/Level3Scene.unity",
                "Assets/Scenes/Level4Scene.unity",
                "Assets/Scenes/EndingCutscene.unity"
            };

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < scenePaths.Length; i++)
            {
                if (System.IO.File.Exists(scenePaths[i]))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(scenePaths[i], true));
                }
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        // =================================================================================================
        // ESCENA 0: MENÚ PRINCIPAL GLASSMORPHISM (ISSUE #07 — PRIMERA ESCENA AL EJECUTAR EL JUEGO)
        // =================================================================================================

        private static void BuildMainMenuScene(string scenePath, GameObject smallEnemyPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateSceneCamera(new Color(0.02f, 0.04f, 0.08f, 1f), null);
            CreateGlobalLight2D(0.85f, Color.white);
            SetupHUD(isMainMenu: true, isTutorial: false, nextScene: "IntroCutscene", smallEnemyPrefab: smallEnemyPrefab, isIntroCutscene: false);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        // =================================================================================================
        // ESCENA 0.5: VIDEO CINEMÁTICA ANIMADA IN-ENGINE (IntroCutscene.unity)
        // =================================================================================================

        private static void BuildIntroCutsceneScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera cam = CreateSceneCamera(new Color(0.03f, 0.05f, 0.10f, 1f), null);
            CreateGlobalLight2D(0.75f, new Color(0.85f, 0.94f, 1.0f, 1f));
            SetupLayeredParallaxBackground(cam.transform, 120f);

            GameObject stage = new GameObject("--- Cutscene_Stage ---");
            Sprite groundSurfaceTile = GetLabGeneratedSprite("Lab_Floor", 128f) ?? GetIndustrialTile("platformIndustrial_002", 140f);
            Sprite steelBlockTile = GetLabGeneratedSprite("Lab_Subfloor", 128f) ?? GetIndustrialTile("platformIndustrial_029", 140f);
            Sprite steelBeamTile = GetLabGeneratedSprite("Lab_Platform", 100f) ?? GetIndustrialTile("platformIndustrial_050", 140f);
            Sprite glowSprite = LoadSoftGlowSprite();

            CreateInteriorLabBackWall(stage.transform, "Cutscene_Lab_BackWall", new Vector3(0f, 2.5f, 0f), new Vector2(44f, 14f));
            CreateTiledPlatform(stage.transform, "Cutscene_Floor", groundSurfaceTile, new Vector3(0f, -2.55f, 0f), new Vector2(44f, 1.0f), false, 0);
            CreateTiledPlatform(stage.transform, "Cutscene_SubFloor", steelBlockTile, new Vector3(0f, -3.55f, 0f), new Vector2(44f, 1.0f), false, -1);
            CreateTiledPlatform(stage.transform, "Cutscene_Ceiling", steelBeamTile, new Vector3(0f, 6.2f, 0f), new Vector2(44f, 0.8f), false, 0);

            CreateLabWindowWithLight(stage.transform, "Cutscene_Window_Left", new Vector3(-7.5f, 2.2f, 0f), glowSprite);
            CreateLabWindowWithLight(stage.transform, "Cutscene_Window_Right", new Vector3(7.5f, 2.2f, 0f), glowSprite);
            CreateCeilingLampWithLightCone(stage.transform, "Cutscene_Lamp_Left", new Vector3(-4.0f, 5.45f, 0f), glowSprite);
            CreateCeilingLampWithLightCone(stage.transform, "Cutscene_Lamp_Right", new Vector3(4.0f, 5.45f, 0f), glowSprite);

            for (int i = -2; i <= 2; i++)
            {
                CreateServerRackUnit(stage.transform, $"Cutscene_ServerRack_{i}", new Vector3(i * 4.2f, -2.05f, 0f), glowSprite, i + 2, isDarkZone: false);
                CreateComputerWorkstation(stage.transform, $"Cutscene_Workstation_{i}", new Vector3(i * 4.2f + 1.9f, -2.05f, 0f), glowSprite, i + 3);
            }

            // Actriz 1: Protagonista Ninja con su Animator Controller completo
            GameObject heroActor = new GameObject("Cutscene_Hero_Ninja");
            heroActor.transform.SetParent(stage.transform, false);
            heroActor.transform.position = new Vector3(-11.5f, -2.05f, 0f);
            SpriteRenderer srHero = heroActor.AddComponent<SpriteRenderer>();
            srHero.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Player/Ninja_Female/png/Idle__000.png");
            srHero.sortingOrder = 6;
            Animator heroAnim = heroActor.AddComponent<Animator>();
            RuntimeAnimatorController playerCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/Player/PlayerAnimatorController.controller");
            if (playerCtrl != null) heroAnim.runtimeAnimatorController = playerCtrl;

            // Actor 2: Profesor 1 (que será corrompido por S.I.A. y luego purificado por la protagonista)
            Sprite[] idleFrames = LoadHumanProfessorIdleFrames();
            GameObject profActor1 = new GameObject("Cutscene_Professor");
            profActor1.transform.SetParent(stage.transform, false);
            profActor1.transform.position = new Vector3(-2.5f, -2.05f, 0f);
            profActor1.transform.localScale = new Vector3(1.22f, 1.22f, 1f);
            SpriteRenderer srProf1 = profActor1.AddComponent<SpriteRenderer>();
            srProf1.sprite = (idleFrames != null && idleFrames.Length > 0) ? idleFrames[0] : LoadHumanProfessorSprite(isRun: false);
            srProf1.sortingOrder = 5;

            // Actor 3: Profesor 2 trabajando al fondo
            GameObject profActor2 = new GameObject("Cutscene_Professor_2");
            profActor2.transform.SetParent(stage.transform, false);
            profActor2.transform.position = new Vector3(3.8f, -2.05f, 0f);
            profActor2.transform.localScale = new Vector3(-1.22f, 1.22f, 1f);
            SpriteRenderer srProf2 = profActor2.AddComponent<SpriteRenderer>();
            srProf2.sprite = (idleFrames != null && idleFrames.Length > 0) ? idleFrames[0] : LoadHumanProfessorSprite(isRun: false);
            srProf2.sortingOrder = 5;

            // Actor 4: Dron de Seguridad S.I.A.
            GameObject robotActor = new GameObject("Cutscene_Robot_1");
            robotActor.transform.SetParent(stage.transform, false);
            robotActor.transform.position = new Vector3(8.5f, -1.35f, 0f);
            robotActor.transform.localScale = new Vector3(-1f, 1f, 1f);
            SpriteRenderer srRobot = robotActor.AddComponent<SpriteRenderer>();
            srRobot.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Enemies/Robots_SIA/png/Run (1).png");
            srRobot.sortingOrder = 5;
            Animator robotAnim = robotActor.AddComponent<Animator>();
            RuntimeAnimatorController robotCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/Enemies/RobotAnimatorController.controller");
            if (robotCtrl != null) robotAnim.runtimeAnimatorController = robotCtrl;
            robotActor.SetActive(false);

            // Actor 5: Entidad Digital S.I.A. (Wraith_03)
            GameObject wraithActor = new GameObject("Cutscene_SIA_Wraith");
            wraithActor.transform.SetParent(stage.transform, false);
            wraithActor.transform.position = new Vector3(1.5f, 5.2f, 0f);
            wraithActor.transform.localScale = new Vector3(-1.05f, 1.05f, 1f);
            SpriteRenderer srWraith = wraithActor.AddComponent<SpriteRenderer>();
            srWraith.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_03/PNG Sequences/Idle/Wraith_03_Idle_000.png");
            srWraith.color = new Color(1f, 0.35f, 0.55f, 0.96f);
            srWraith.sortingOrder = 7;
            Animator wraithAnim = wraithActor.AddComponent<Animator>();
            RuntimeAnimatorController bossCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/Enemies/BossWraithAnimator.controller");
            if (bossCtrl != null) wraithAnim.runtimeAnimatorController = bossCtrl;
            wraithActor.SetActive(false);

            SetupHUD(isMainMenu: false, isTutorial: false, nextScene: "TutorialScene", smallEnemyPrefab: null, isIntroCutscene: true);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        // =================================================================================================
        // ESCENA FINAL: ENDING CUTSCENE (8 ESCENAS — COLAPSO S.I.A., RESCATE, PREMIO, AMANECER Y CRÉDITOS)
        // =================================================================================================

        private static void BuildEndingCutsceneScene(string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera cam = CreateSceneCamera(new Color(0.03f, 0.04f, 0.09f, 1f), null);
            CreateGlobalLight2D(0.78f, new Color(0.95f, 0.96f, 1.0f, 1f));

            GameObject stage = new GameObject("--- Ending_Cutscene_Stage ---");
            Sprite groundSurfaceTile = GetLabGeneratedSprite("Lab_Floor", 128f) ?? GetIndustrialTile("platformIndustrial_002", 140f);
            Sprite steelBlockTile = GetLabGeneratedSprite("Lab_Subfloor", 128f) ?? GetIndustrialTile("platformIndustrial_029", 140f);
            Sprite steelBeamTile = GetLabGeneratedSprite("Lab_Platform", 100f) ?? GetIndustrialTile("platformIndustrial_050", 140f);
            Sprite glowSprite = LoadSoftGlowSprite();
            Sprite[] profIdleFrames = LoadHumanProfessorIdleFrames();
            Sprite profSprite = (profIdleFrames != null && profIdleFrames.Length > 0) ? profIdleFrames[0] : LoadHumanProfessorSprite(isRun: false);

            // Fondo de amanecer usando las capas de city 1 y city 4 para el tramo final (X = 120 a 175)
            Sprite dawnSky = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/World/Backgrounds_Cyberpunk_City/city 4/1.png", 32f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            Sprite dawnCityFar = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/World/Backgrounds_Cyberpunk_City/city 1/5.png", 32f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            Sprite dawnCityNear = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/World/Backgrounds_Cyberpunk_City/city 4/8.png", 32f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);

            for (int b = 0; b < 3; b++)
            {
                float bx = 128f + b * 24f;
                GameObject s1 = new GameObject($"Dawn_Sky_{b}");
                s1.transform.SetParent(stage.transform, false);
                s1.transform.position = new Vector3(bx, 4.5f, 0f);
                s1.transform.localScale = new Vector3(2.4f, 2.8f, 1f);
                SpriteRenderer sr1 = s1.AddComponent<SpriteRenderer>();
                sr1.sprite = dawnSky;
                sr1.color = new Color(1.0f, 0.78f, 0.45f, 1f);
                sr1.sortingOrder = -28;

                GameObject s2 = new GameObject($"Dawn_FarCity_{b}");
                s2.transform.SetParent(stage.transform, false);
                s2.transform.position = new Vector3(bx, 2.8f, 0f);
                s2.transform.localScale = new Vector3(2.4f, 2.6f, 1f);
                SpriteRenderer sr2 = s2.AddComponent<SpriteRenderer>();
                sr2.sprite = dawnCityFar;
                sr2.color = new Color(1.0f, 0.85f, 0.65f, 0.92f);
                sr2.sortingOrder = -24;

                GameObject s3 = new GameObject($"Dawn_NearCity_{b}");
                s3.transform.SetParent(stage.transform, false);
                s3.transform.position = new Vector3(bx, 1.8f, 0f);
                s3.transform.localScale = new Vector3(2.4f, 2.5f, 1f);
                SpriteRenderer sr3 = s3.AddComponent<SpriteRenderer>();
                sr3.sprite = dawnCityNear;
                sr3.color = new Color(0.85f, 0.94f, 1.0f, 0.95f);
                sr3.sortingOrder = -20;
            }

            // Suelo y techo continuos a lo largo de las 5 zonas (X = -20 a +170)
            CreateTiledPlatform(stage.transform, "Ending_Floor", groundSurfaceTile, new Vector3(75f, -2.55f, 0f), new Vector2(195f, 1.0f), false, 0);
            CreateTiledPlatform(stage.transform, "Ending_SubFloor", steelBlockTile, new Vector3(75f, -3.55f, 0f), new Vector2(195f, 1.0f), false, -1);
            CreateTiledPlatform(stage.transform, "Ending_Ceiling", steelBeamTile, new Vector3(52f, 6.4f, 0f), new Vector2(148f, 0.8f), false, 0);

            // ZONA 1 (X = 0): Núcleo Central S.I.A. en colapso
            CreateDarkServerZoneWall(stage.transform, "Ending_Core_BackWall", new Vector3(0f, 2.0f, 0f), new Vector2(34f, 12.5f));
            CreateLabSignWithGlow(stage.transform, "Ending_Core_Sign", "Lab_Sign_Core", new Vector3(0f, 4.2f, 0f), glowSprite, new Color(1.0f, 0.25f, 0.35f, 0.24f));
            for (int r = -1; r <= 1; r += 2)
            {
                CreateServerRackUnit(stage.transform, $"Ending_Core_Rack_{r}", new Vector3(r * 7.5f, -2.05f, 0f), glowSprite, 1, isDarkZone: true);
            }

            GameObject bossWraith = new GameObject("Ending_Wraith_Boss");
            bossWraith.transform.SetParent(stage.transform, false);
            bossWraith.transform.position = new Vector3(3.2f, 0.4f, 0f);
            bossWraith.transform.localScale = new Vector3(-1.15f, 1.15f, 1f);
            SpriteRenderer srWraith = bossWraith.AddComponent<SpriteRenderer>();
            srWraith.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_03/PNG Sequences/Idle/Wraith_03_Idle_000.png");
            srWraith.sortingOrder = 7;

            GameObject bossHuman = new GameObject("Ending_Boss_Human");
            bossHuman.transform.SetParent(stage.transform, false);
            bossHuman.transform.position = new Vector3(3.2f, -2.05f, 0f);
            bossHuman.transform.localScale = new Vector3(-1.25f, 1.25f, 1f);
            SpriteRenderer srBossHuman = bossHuman.AddComponent<SpriteRenderer>();
            srBossHuman.sprite = profSprite;
            srBossHuman.sortingOrder = 6;
            bossHuman.SetActive(false);

            // ZONA 2 (X = 36): Apagón de Drones y Pantallas cambiando de Alerta Roja a Código Azul
            CreateDarkServerZoneWall(stage.transform, "Ending_Shutdown_BackWall", new Vector3(36f, 2.0f, 0f), new Vector2(34f, 12.5f));
            CreateLabSignWithGlow(stage.transform, "Ending_Net_Sign", "Lab_Sign_Network", new Vector3(36f, 4.2f, 0f), glowSprite, new Color(0.25f, 0.90f, 1.0f, 0.20f));
            Sprite alertScreenSprite = GetLabGeneratedSprite("Lab_Screen_Alert", 100f);
            Sprite robotIdleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Enemies/Robots_SIA/png/Idle (1).png");
            for (int i = 0; i < 3; i++)
            {
                float sx = 28.5f + i * 7.5f;
                GameObject scr = new GameObject($"Ending_Alert_Screen_{i}");
                scr.transform.SetParent(stage.transform, false);
                scr.transform.position = new Vector3(sx, 2.1f, 0f);
                SpriteRenderer srScr = scr.AddComponent<SpriteRenderer>();
                srScr.sprite = alertScreenSprite;
                srScr.sortingOrder = -2;
                NormalizePropWorldScale(scr.transform, alertScreenSprite, 2.2f);

                CreatePointLightGlow(stage.transform, $"Ending_Alarm_Light_{i}", new Vector3(sx, 4.6f, 0f),
                    glowSprite, new Color(1.0f, 0.16f, 0.26f, 0.30f), 5.0f);

                GameObject rob = new GameObject($"Ending_Robot_{i}");
                rob.transform.SetParent(stage.transform, false);
                rob.transform.position = new Vector3(sx + 1.2f, -1.25f, 0f);
                rob.transform.localScale = new Vector3(-1.0f, 1.0f, 1f);
                SpriteRenderer srRob = rob.AddComponent<SpriteRenderer>();
                srRob.sprite = robotIdleSprite;
                srRob.sortingOrder = 5;
            }

            // ZONA 3 (X = 72): 4 Sectores Liberados (Laboratorio, Redes, Servidores, Núcleo)
            CreateInteriorLabBackWall(stage.transform, "Ending_Sectors_BackWall", new Vector3(72f, 2.0f, 0f), new Vector2(36f, 12.5f));
            string[] secSigns = { "Lab_Sign_Lab", "Lab_Sign_Network", "Lab_Sign_Servers", "Lab_Sign_Core" };
            Color[] secGlows =
            {
                new Color(0.20f, 0.95f, 1.0f, 0.22f),
                new Color(1.0f, 0.65f, 0.25f, 0.22f),
                new Color(0.70f, 0.45f, 1.0f, 0.22f),
                new Color(0.25f, 1.0f, 0.72f, 0.22f)
            };
            for (int i = 0; i < 4; i++)
            {
                float px = 61f + i * 7.2f;
                CreateLabSignWithGlow(stage.transform, $"Ending_Sec_Sign_{i}", secSigns[i], new Vector3(px, 3.8f, 0f), glowSprite, secGlows[i]);
                CreateLabStaticProp(stage.transform, $"Ending_Sec_Door_{i}", "Lab_Door", new Vector3(px - 2.0f, -0.70f, 0f), 2.7f, -3);

                GameObject fp = new GameObject($"Ending_Free_Prof_{i}");
                fp.transform.SetParent(stage.transform, false);
                fp.transform.position = new Vector3(px - 1.5f, -2.05f, 0f);
                fp.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                SpriteRenderer srFp = fp.AddComponent<SpriteRenderer>();
                srFp.sprite = profSprite;
                srFp.sortingOrder = 5;
            }

            // ZONA 4 (X = 108): Atrio UTNG — Encuentro y Ceremonia de Condecoración (Premio)
            CreateInteriorLabBackWall(stage.transform, "Ending_Atrium_BackWall", new Vector3(108f, 2.0f, 0f), new Vector2(34f, 12.5f));
            CreateLabWindowWithLight(stage.transform, "Ending_Atrium_Window_L", new Vector3(101f, 2.4f, 0f), glowSprite);
            CreateLabWindowWithLight(stage.transform, "Ending_Atrium_Window_R", new Vector3(115f, 2.4f, 0f), glowSprite);
            CreateCeilingLampWithLightCone(stage.transform, "Ending_Atrium_Lamp", new Vector3(108f, 5.4f, 0f), glowSprite);

            GameObject heroObj = new GameObject("Ending_Hero");
            heroObj.transform.SetParent(stage.transform, false);
            heroObj.transform.position = new Vector3(98f, -2.05f, 0f);
            SpriteRenderer srH = heroObj.AddComponent<SpriteRenderer>();
            srH.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Player/Ninja_Female/png/Idle__000.png");
            srH.sortingOrder = 8;
            Animator hAnim = heroObj.AddComponent<Animator>();
            RuntimeAnimatorController pCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/Player/PlayerAnimatorController.controller");
            if (pCtrl != null) hAnim.runtimeAnimatorController = pCtrl;

            GameObject partnerObj = new GameObject("Ending_Partner");
            partnerObj.transform.SetParent(stage.transform, false);
            partnerObj.transform.position = new Vector3(95f, -2.05f, 0f);
            SpriteRenderer srP = partnerObj.AddComponent<SpriteRenderer>();
            srP.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Player/Ninja_Male/png/Idle__000.png");
            srP.sortingOrder = 7;
            Animator pAnim = partnerObj.AddComponent<Animator>();
            if (pCtrl != null) pAnim.runtimeAnimatorController = pCtrl;

            float[] facultyX = { 110.2f, 112.4f, 114.6f, 116.8f };
            for (int i = 0; i < 4; i++)
            {
                GameObject fac = new GameObject($"Ending_Faculty_{i}");
                fac.transform.SetParent(stage.transform, false);
                fac.transform.position = new Vector3(facultyX[i], -2.05f, 0f);
                fac.transform.localScale = new Vector3(-1.25f, 1.25f, 1f);
                SpriteRenderer srFac = fac.AddComponent<SpriteRenderer>();
                srFac.sprite = profSprite;
                srFac.sortingOrder = 6;
            }

            CreatePointLightGlow(stage.transform, "Ending_Award_Glow", new Vector3(105.2f, 0.8f, 0f),
                glowSprite, new Color(1.0f, 0.86f, 0.28f, 0.42f), 5.5f);

            GameObject medalObj = new GameObject("Ending_Medal_Icon");
            medalObj.transform.SetParent(stage.transform, false);
            medalObj.transform.position = new Vector3(105.2f, 4.6f, 0f);
            SpriteRenderer srMedal = medalObj.AddComponent<SpriteRenderer>();
            srMedal.sprite = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/UI/SciFi_Interface_Wenrexa/PNG/Icons/Icon05.png", 40f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear)
                ?? GetOrCreateCircleSprite();
            srMedal.color = new Color(1.0f, 0.90f, 0.35f, 1f);
            srMedal.sortingOrder = 12;

            // ZONA 5 (X = 144): Mirador Panorámico al Amanecer
            CreatePointLightGlow(stage.transform, "Ending_Sunrise_Glow", new Vector3(148f, 6.5f, 0f),
                glowSprite, new Color(1.0f, 0.80f, 0.40f, 0.32f), 14.0f);

            SetupHUD(isMainMenu: false, isTutorial: false, nextScene: "MainMenu", smallEnemyPrefab: null, isIntroCutscene: false, isEndingCutscene: true);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        // =================================================================================================
        // ESCENAS 3, 4, 5: NIVELES 2, 3 Y 4 EN CONSTRUCCIÓN CON MENÚ DE PAUSA FUNCIONAL (ISSUE #07)
        // =================================================================================================

        private static void BuildPlaceholderLevelScene(string scenePath, string nextScene, Color bgColor)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera cam = CreateSceneCamera(bgColor, null);
            CreateGlobalLight2D(0.70f, new Color(0.85f, 0.94f, 1.0f, 1f));
            SetupLayeredParallaxBackground(cam.transform, 200f);
            SetupHUD(isMainMenu: false, isTutorial: false, nextScene: nextScene, smallEnemyPrefab: null, isIntroCutscene: false);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        // =================================================================================================
        // ESCENA 1: TUTORIAL INTERACTIVO ANTES DEL NIVEL 1 + MINI-JEFE FLOTANTE (ISSUE #08)
        // =================================================================================================

        private static void BuildTutorialInteractiveScene(string scenePath, GameObject pulsePrefab, GameObject smallEnemyPrefab, PhysicsMaterial2D playerMat)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera cam = CreateSceneCamera(new Color(0.04f, 0.06f, 0.12f, 1f), null);
            CreateGlobalLight2D(0.74f, new Color(0.86f, 0.94f, 1.0f, 1f));
            SetupLayeredParallaxBackground(cam.transform, 220f);

            GameObject env = new GameObject("--- Environment ---");

            Sprite groundSurfaceTile = GetLabGeneratedSprite("Lab_Floor", 128f) ?? GetIndustrialTile("platformIndustrial_002", 140f);
            Sprite steelBlockTile = GetLabGeneratedSprite("Lab_Subfloor", 128f) ?? GetIndustrialTile("platformIndustrial_029", 140f);
            Sprite steelBeamTile = GetLabGeneratedSprite("Lab_Platform", 100f) ?? GetIndustrialTile("platformIndustrial_050", 140f);
            Sprite hazardBeamTile = GetLabGeneratedSprite("Lab_PlatformHazard", 100f) ?? GetIndustrialTile("platformIndustrial_064", 140f);
            Sprite pillarTile = GetLabGeneratedSprite("Lab_Pillar", 64f) ?? steelBlockTile;
            Sprite ladderMidTile = GetIndustrialTile("platformIndustrial_047", 140f);
            Sprite terminalScreenTile = GetIndustrialTile("platformIndustrial_045", 140f);
            Sprite sirenRedTile = GetIndustrialTile("platformIndustrial_054", 140f);
            Sprite glowSprite = LoadSoftGlowSprite();

            CreateInteriorLabBackWall(env.transform, "Wall_Bg_Tutorial_Interior", new Vector3(128f, 6.8f, 0f), new Vector2(310f, 18.5f));

            // Suelo continuo espacioso desde X = -26 hasta X = +282 (32-38m entre cada estación y checkpoint)
            CreateTiledPlatform(env.transform, "Ground_Tutorial_Main", groundSurfaceTile, new Vector3(128f, -2.25f, 0f), new Vector2(310f, 1.0f), true, 0);
            CreateTiledPlatform(env.transform, "Ground_Tutorial_Sub", steelBlockTile, new Vector3(128f, -3.25f, 0f), new Vector2(310f, 1.0f), false, -1);
            CreateTiledPlatform(env.transform, "Ceiling_Tutorial", steelBeamTile, new Vector3(128f, 16.5f, 0f), new Vector2(310f, 0.8f), true, 0);
            CreateTiledPlatform(env.transform, "Wall_Left_Tutorial", pillarTile, new Vector3(-26f, 7.0f, 0f), new Vector2(1.5f, 20.0f), true, 0);
            CreateTiledPlatform(env.transform, "Wall_Right_Tutorial", pillarTile, new Vector3(282f, 7.0f, 0f), new Vector2(1.5f, 20.0f), true, 0);

            // Estación 1 (X = -21.5 a +10): Amplio pasillo inicial para practicar movimiento lateral
            CreateLabWindowWithLight(env.transform, "Window_Tut_Station1A", new Vector3(-13f, 3.4f, 0f), glowSprite);
            CreateComputerWorkstation(env.transform, "Workstation_Tut_1A", new Vector3(-8f, -1.75f, 0f), glowSprite, 0);
            CreateLabStaticProp(env.transform, "Whiteboard_Tut_1", "Lab_Whiteboard", new Vector3(-1f, 1.35f, 0f), 2.8f, -3);
            CreateComputerWorkstation(env.transform, "Workstation_Tut_1B", new Vector3(5f, -1.75f, 0f), glowSprite, 1);

            // Estación 2 (X = +12 a +44): Escaleras cómodas al ras (height = 3.60f) y plataforma elevada
            CreateClimbableLadder(env.transform, "Ladder_Tut_1to2", ladderMidTile, new Vector3(18.0f, LadderCenterY(1), 0f), 3.60f);
            CreateTiledPlatform(env.transform, "Platform_Tut_Station2", steelBeamTile, new Vector3(25.0f, FloorCenterY(2), 0f), new Vector2(18f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_Tut_2to1", ladderMidTile, new Vector3(32.0f, LadderCenterY(1), 0f), 3.60f);
            CreateComputerWorkstation(env.transform, "Workstation_Tut_P2", new Vector3(25.0f, FloorSurfaceY(2), 0f), glowSprite, 2);
            CreateLabWindowWithLight(env.transform, "Window_Tut_Station2", new Vector3(39f, 3.4f, 0f), glowSprite);

            // Estación 3 (X = +46 a +80): Plataformas escalonadas para Salto y Caída Suave (Paracaídas)
            CreateTiledPlatform(env.transform, "Platform_Tut_JumpStep2", steelBeamTile, new Vector3(54.5f, FloorCenterY(2), 0f), new Vector2(11f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_Tut_Jump1to2", ladderMidTile, new Vector3(51.0f, LadderCenterY(1), 0f), 3.60f);

            CreateTiledPlatform(env.transform, "Platform_Tut_HighGlide", hazardBeamTile, new Vector3(62.5f, FloorCenterY(3), 0f), new Vector2(12f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_Tut_Jump2to3", ladderMidTile, new Vector3(58.5f, LadderCenterY(2), 0f), 3.60f);
            CreateLabWindowWithLight(env.transform, "Window_Tut_Station3", new Vector3(72f, 4.5f, 0f), glowSprite);

            // Estación 4 (X = +82 a +114): Largo corredor de prácticas para Esquive Dash
            CreateComputerWorkstation(env.transform, "Workstation_Tut_4A", new Vector3(90f, -1.75f, 0f), glowSprite, 3);
            CreateServerRackUnit(env.transform, "ServerRack_Tut_4", new Vector3(98f, -1.75f, 0f), glowSprite, 1, isDarkZone: false);
            CreateComputerWorkstation(env.transform, "Workstation_Tut_4B", new Vector3(106f, -1.75f, 0f), glowSprite, 4);

            // Estación 5 (X = +116 a +148): Campo de práctica para Corte de Katana
            CreateLabWindowWithLight(env.transform, "Window_Tut_Station5", new Vector3(128f, 3.4f, 0f), glowSprite);
            CreateComputerWorkstation(env.transform, "Workstation_Tut_5A", new Vector3(124f, -1.75f, 0f), glowSprite, 5);
            CreateComputerWorkstation(env.transform, "Workstation_Tut_5B", new Vector3(140f, -1.75f, 0f), glowSprite, 6);

            // Estación 6 (X = +150 a +182): Galería de tiro para Disparo PEM
            CreateLabWindowWithLight(env.transform, "Window_Tut_Station6", new Vector3(166f, 3.4f, 0f), glowSprite);
            CreateServerRackUnit(env.transform, "ServerRack_Tut_6", new Vector3(158f, -1.75f, 0f), glowSprite, 2, isDarkZone: false);
            CreateComputerWorkstation(env.transform, "Workstation_Tut_6B", new Vector3(174f, -1.75f, 0f), glowSprite, 7);

            // Estación 7 (X = +184 a +222): Galería amplia para Disparo PEM Cargado
            CreateLabWindowWithLight(env.transform, "Window_Tut_Station7", new Vector3(202f, 3.4f, 0f), glowSprite);
            CreateComputerWorkstation(env.transform, "Workstation_Tut_7A", new Vector3(194f, -1.75f, 0f), glowSprite, 8);
            CreateServerRackUnit(env.transform, "ServerRack_Tut_7", new Vector3(210f, -1.75f, 0f), glowSprite, 3, isDarkZone: false);

            // 8 Checkpoints del Tutorial (muy bien espaciados: uno por estación + pre-jefe)
            CreateCheckpointStation(env.transform, "Checkpoint_1", terminalScreenTile, sirenRedTile, new Vector3(-21.5f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_2", terminalScreenTile, sirenRedTile, new Vector3(12.0f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_3", terminalScreenTile, sirenRedTile, new Vector3(46.0f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_4", terminalScreenTile, sirenRedTile, new Vector3(82.0f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_5", terminalScreenTile, sirenRedTile, new Vector3(116.0f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_6", terminalScreenTile, sirenRedTile, new Vector3(150.0f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_7", terminalScreenTile, sirenRedTile, new Vector3(184.0f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_8", terminalScreenTile, sirenRedTile, new Vector3(224.0f, -1.75f, 0f), glowSprite);

            // Estación 8 (X = +232 a +280): ARENA DEL MINI-JEFE DEL TUTORIAL
            CreateBossArenaContainmentGate(env.transform, gateX: 232.0f, triggerX: 237.0f, hazardBeamTile);
            CreateLabWindowWithLight(env.transform, "Window_Tut_BossArena_1", new Vector3(244f, 4.2f, 0f), glowSprite);
            CreateComputerWorkstation(env.transform, "Workstation_Tut_Arena", new Vector3(256f, -1.75f, 0f), glowSprite, 9);
            CreateLabWindowWithLight(env.transform, "Window_Tut_BossArena_2", new Vector3(268f, 4.2f, 0f), glowSprite);

            GameObject player = SetupPlayer(pulsePrefab, playerMat, new Vector3(-21f, -1.75f, 0f));
            cam.GetComponent<CameraFollow2D>().SetTarget(player.transform);

            CreateWraithEnemy(
                objName: "Teacher_Boss_Tutorial",
                pos: new Vector3(258.0f, 2.45f, 0f),
                isFinalBoss: true,
                isFloatingBoss: true,
                braceletHp: 100,
                hoverY: 2.45f,
                arenaRadius: 11.5f,
                projectileDmg: 5,
                attackInterval: 3.1f
            );

            SetupHUD(isMainMenu: false, isTutorial: true, nextScene: "SampleScene", smallEnemyPrefab: smallEnemyPrefab, isIntroCutscene: false);
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        // =================================================================================================
        // TEMAS Y DIFICULTAD DE LOS 4 NIVELES (MISMA ESTRUCTURA DE 1,260M; CADA NIVEL +20% MÁS DIFÍCIL)
        // =================================================================================================

        private sealed class LevelTheme
        {
            public int levelNumber;
            public string displayName;
            public string nextScene;
            public Color cameraBg;
            public Color globalLightColor;
            public float globalLightIntensity;
            public Color lightWallTint;
            public Color darkWallTint;
            public Color darkVeilColor;
            public Color platformTint;
            public Color parallaxSky;
            public Color parallaxFar;
            public Color parallaxNear;
            public string[] sectorSigns;
            public Color[] sectorColors;
            public int[] topFloors;
            public string wallDisplayA;
            public string wallDisplayB;
            public Color bossSignColor;
            public Color accentColor;
            public int decorStyle;       // 0 = Laboratorio, 1 = Centro de Redes, 2 = Torre de Servidores, 3 = Núcleo de Control S.I.A.
            public float difficulty;     // 1.0, 1.2, 1.44, 1.728 (+20% por nivel)
        }

        private static LevelTheme activeTheme = GetLevelTheme(1);

        private static LevelTheme GetLevelTheme(int level)
        {
            float difficulty = Mathf.Pow(1.2f, Mathf.Clamp(level, 1, 4) - 1);
            switch (level)
            {
                case 2:
                    return new LevelTheme
                    {
                        levelNumber = 2, displayName = "Centro de Redes", nextScene = "Level3Scene",
                        cameraBg = new Color(0.07f, 0.05f, 0.03f, 1f),
                        globalLightColor = new Color(1.0f, 0.90f, 0.78f, 1f), globalLightIntensity = 0.68f,
                        lightWallTint = new Color(0.98f, 0.86f, 0.72f, 0.98f),
                        darkWallTint = new Color(0.46f, 0.34f, 0.24f, 0.99f),
                        darkVeilColor = new Color(0.05f, 0.025f, 0.01f, 0.52f),
                        platformTint = new Color(1.0f, 0.86f, 0.70f, 1f),
                        parallaxSky = new Color(0.45f, 0.30f, 0.18f, 1f),
                        parallaxFar = new Color(0.90f, 0.62f, 0.38f, 0.92f),
                        parallaxNear = new Color(0.98f, 0.76f, 0.52f, 0.90f),
                        sectorSigns = new[] { "Lab_Sign_Network", "Lab_Sign_Network", "Lab_Sign_Servers", "Lab_Sign_Network", "Lab_Sign_Data" },
                        sectorColors = new[]
                        {
                            new Color(1.0f, 0.60f, 0.22f, 0.18f), new Color(1.0f, 0.74f, 0.25f, 0.18f), new Color(1.0f, 0.45f, 0.18f, 0.18f),
                            new Color(1.0f, 0.66f, 0.30f, 0.18f), new Color(1.0f, 0.55f, 0.20f, 0.18f)
                        },
                        topFloors = new[] { 10, 14, 18, 12, 10 },
                        wallDisplayA = "Lab_WallDisplay_Net", wallDisplayB = "Lab_WallDisplay_Net",
                        bossSignColor = new Color(1.0f, 0.55f, 0.15f, 0.22f), accentColor = new Color(1.0f, 0.62f, 0.20f, 0.20f),
                        decorStyle = 1, difficulty = difficulty
                    };
                case 3:
                    return new LevelTheme
                    {
                        levelNumber = 3, displayName = "Torre de Servidores", nextScene = "Level4Scene",
                        cameraBg = new Color(0.04f, 0.03f, 0.10f, 1f),
                        globalLightColor = new Color(0.80f, 0.78f, 1.0f, 1f), globalLightIntensity = 0.64f,
                        lightWallTint = new Color(0.72f, 0.70f, 0.96f, 0.98f),
                        darkWallTint = new Color(0.26f, 0.24f, 0.46f, 0.99f),
                        darkVeilColor = new Color(0.02f, 0.01f, 0.07f, 0.55f),
                        platformTint = new Color(0.80f, 0.76f, 1.0f, 1f),
                        parallaxSky = new Color(0.22f, 0.18f, 0.50f, 1f),
                        parallaxFar = new Color(0.55f, 0.45f, 0.92f, 0.92f),
                        parallaxNear = new Color(0.68f, 0.60f, 0.98f, 0.90f),
                        sectorSigns = new[] { "Lab_Sign_Servers", "Lab_Sign_Data", "Lab_Sign_Servers", "Lab_Sign_Data", "Lab_Sign_Servers" },
                        sectorColors = new[]
                        {
                            new Color(0.70f, 0.45f, 1.0f, 0.18f), new Color(0.50f, 0.55f, 1.0f, 0.18f), new Color(0.85f, 0.40f, 1.0f, 0.18f),
                            new Color(0.55f, 0.50f, 1.0f, 0.18f), new Color(0.75f, 0.45f, 1.0f, 0.18f)
                        },
                        topFloors = new[] { 14, 18, 20, 18, 14 },
                        wallDisplayA = "Lab_WallDisplay_Eye", wallDisplayB = "Lab_WallDisplay_Net",
                        bossSignColor = new Color(0.78f, 0.40f, 1.0f, 0.22f), accentColor = new Color(0.72f, 0.45f, 1.0f, 0.20f),
                        decorStyle = 2, difficulty = difficulty
                    };
                case 4:
                    return new LevelTheme
                    {
                        levelNumber = 4, displayName = "Núcleo Central S.I.A.", nextScene = "EndingCutscene",
                        cameraBg = new Color(0.09f, 0.02f, 0.04f, 1f),
                        globalLightColor = new Color(1.0f, 0.74f, 0.78f, 1f), globalLightIntensity = 0.58f,
                        lightWallTint = new Color(0.92f, 0.66f, 0.72f, 0.98f),
                        darkWallTint = new Color(0.40f, 0.18f, 0.24f, 0.99f),
                        darkVeilColor = new Color(0.07f, 0.01f, 0.03f, 0.58f),
                        platformTint = new Color(0.96f, 0.72f, 0.78f, 1f),
                        parallaxSky = new Color(0.45f, 0.12f, 0.22f, 1f),
                        parallaxFar = new Color(0.92f, 0.35f, 0.50f, 0.92f),
                        parallaxNear = new Color(0.98f, 0.52f, 0.62f, 0.90f),
                        sectorSigns = new[] { "Lab_Sign_Core", "Lab_Sign_Data", "Lab_Sign_Core", "Lab_Sign_Servers", "Lab_Sign_Core" },
                        sectorColors = new[]
                        {
                            new Color(1.0f, 0.22f, 0.38f, 0.20f), new Color(1.0f, 0.35f, 0.45f, 0.20f), new Color(1.0f, 0.18f, 0.32f, 0.20f),
                            new Color(1.0f, 0.40f, 0.30f, 0.20f), new Color(1.0f, 0.22f, 0.38f, 0.20f)
                        },
                        topFloors = new[] { 12, 16, 20, 16, 12 },
                        wallDisplayA = "Lab_WallDisplay_Eye", wallDisplayB = "Lab_WallDisplay_Eye",
                        bossSignColor = new Color(1.0f, 0.15f, 0.30f, 0.26f), accentColor = new Color(1.0f, 0.22f, 0.38f, 0.22f),
                        decorStyle = 3, difficulty = difficulty
                    };
                default:
                    return new LevelTheme
                    {
                        levelNumber = 1, displayName = "Laboratorio de Cómputo", nextScene = "Level2Scene",
                        cameraBg = new Color(0.03f, 0.05f, 0.10f, 1f),
                        globalLightColor = new Color(0.84f, 0.92f, 1.0f, 1f), globalLightIntensity = 0.72f,
                        lightWallTint = new Color(0.76f, 0.85f, 0.96f, 0.98f),
                        darkWallTint = new Color(0.24f, 0.32f, 0.45f, 0.99f),
                        darkVeilColor = new Color(0.01f, 0.02f, 0.05f, 0.52f),
                        platformTint = Color.white,
                        parallaxSky = new Color(0.20f, 0.32f, 0.52f, 1f),
                        parallaxFar = new Color(0.45f, 0.65f, 0.90f, 0.92f),
                        parallaxNear = new Color(0.62f, 0.78f, 0.96f, 0.90f),
                        sectorSigns = new[] { "Lab_Sign_Lab", "Lab_Sign_Servers", "Lab_Sign_Network", "Lab_Sign_Data", "Lab_Sign_Servers" },
                        sectorColors = new[]
                        {
                            new Color(0.15f, 0.85f, 1.0f, 0.16f), new Color(0.20f, 1.0f, 0.65f, 0.16f), new Color(1.0f, 0.60f, 0.22f, 0.16f),
                            new Color(0.70f, 0.45f, 1.0f, 0.16f), new Color(0.20f, 0.95f, 0.85f, 0.16f)
                        },
                        topFloors = new[] { 9, 12, 18, 12, 9 },
                        wallDisplayA = "Lab_WallDisplay_Net", wallDisplayB = "Lab_WallDisplay_Eye",
                        bossSignColor = new Color(1.0f, 0.22f, 0.38f, 0.20f), accentColor = new Color(0.15f, 0.85f, 1.0f, 0.16f),
                        decorStyle = 0, difficulty = 1.0f
                    };
            }
        }

        // =================================================================================================
        // NIVELES 1 A 4 — MISMA ESTRUCTURA (1,260M x HASTA 20 PLANTAS) CON TEMÁTICA Y DIFICULTAD PROPIAS
        // =================================================================================================

        private static void BuildThemedLevelScene(string scenePath, LevelTheme theme, GameObject pulsePrefab, GameObject smallEnemyPrefab, PhysicsMaterial2D playerMat)
        {
            activeTheme = theme;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Cámara y Luz Global 2D con la paleta del tema
            Camera cam = CreateSceneCamera(theme.cameraBg, null);
            CreateGlobalLight2D(theme.globalLightIntensity, theme.globalLightColor);

            // 2. Fondos en Capas (ParallaxBackground) a lo largo de los 1,260 metros con el tinte del tema
            SetupLayeredParallaxBackground(cam.transform, 1320f);

            // 3. Construir el nivel de 1,260m de largo (-30 a +1230) y hasta 20 plantas de altura
            SetupThemedLevelEnvironment(theme);

            // 4. Protagonista en el extremo izquierdo (X = -24, Y = -1.75)
            GameObject player = SetupPlayer(pulsePrefab, playerMat, new Vector3(-24f, -1.75f, 0f));
            cam.GetComponent<CameraFollow2D>().SetTarget(player.transform);

            // 5. Patrulleros intermedios y Jefe Final Flotante escalados por dificultad
            SetupThemedEnemiesAndFloatingBoss(theme);

            // 6. Spawner de Drones por proximidad escalado por dificultad
            SetupSpawner(smallEnemyPrefab, bossZoneStartX: 1135f, theme.difficulty);

            // 7. HUD Sci-Fi (el siguiente nivel de cada escena encadena Nivel 1 -> 2 -> 3 -> 4 -> Menú Principal)
            SetupHUD(isMainMenu: false, isTutorial: false, nextScene: theme.nextScene, smallEnemyPrefab: smallEnemyPrefab, isIntroCutscene: false);

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void SetupThemedLevelEnvironment(LevelTheme theme)
        {
            GameObject env = new GameObject("--- Environment ---");

            int maxFloor = 1;
            for (int i = 0; i < theme.topFloors.Length; i++) maxFloor = Mathf.Max(maxFloor, theme.topFloors[i]);
            float ceilingY = FloorSurfaceY(maxFloor) + 6.05f;
            float boundaryHeight = ceilingY + 4.5f;
            float boundaryCenterY = (ceilingY - 3.5f) * 0.5f;

            Sprite groundSurfaceTile = GetLabGeneratedSprite("Lab_Floor", 128f) ?? GetIndustrialTile("platformIndustrial_002", 140f);
            Sprite steelBlockTile = GetLabGeneratedSprite("Lab_Subfloor", 128f) ?? GetIndustrialTile("platformIndustrial_029", 140f);
            Sprite steelBeamTile = GetLabGeneratedSprite("Lab_Platform", 100f) ?? GetIndustrialTile("platformIndustrial_050", 140f);
            Sprite hazardBeamTile = GetLabGeneratedSprite("Lab_PlatformHazard", 100f) ?? GetIndustrialTile("platformIndustrial_064", 140f);
            Sprite pillarTile = GetLabGeneratedSprite("Lab_Pillar", 64f) ?? steelBlockTile;
            Sprite cableTrayTile = GetLabGeneratedSprite("Lab_CableTray", 100f);
            Sprite pipeHTile = GetLabGeneratedSprite("Lab_PipeH", 100f);
            Sprite pipeVTile = GetLabGeneratedSprite("Lab_PipeV", 100f);
            Sprite ladderMidTile = GetIndustrialTile("platformIndustrial_047", 140f);
            Sprite terminalScreenTile = GetIndustrialTile("platformIndustrial_045", 140f);
            Sprite sirenRedTile = GetIndustrialTile("platformIndustrial_054", 140f);
            Sprite arrowRightTile = GetIndustrialTile("platformIndustrial_070", 140f);
            Sprite arrowUpTile = GetIndustrialTile("platformIndustrial_084", 140f);
            Sprite glowSprite = LoadSoftGlowSprite();

            // Suelo y bordes exteriores del Mapa Nivel 1 (1,260m x 66m) con piso reflectante de Laboratorio de Cómputo y subsuelo de conductos
            CreateTiledPlatform(env.transform, "Ground_Main_Surface", groundSurfaceTile, new Vector3(600f, -2.25f, 0f), new Vector2(1260f, 1.0f), true, 0);
            CreateTiledPlatform(env.transform, "Ground_Main_Subfloor", steelBlockTile, new Vector3(600f, -3.25f, 0f), new Vector2(1260f, 1.0f), false, -1);
            CreateTiledPlatform(env.transform, "Ceiling_Main_18F", steelBeamTile, new Vector3(600f, ceilingY, 0f), new Vector2(1262f, 0.8f), true, 0);
            CreateTiledPlatform(env.transform, "Wall_Left_Boundary", pillarTile, new Vector3(-30f, boundaryCenterY, 0f), new Vector2(1.5f, boundaryHeight), true, 0);
            CreateTiledPlatform(env.transform, "Wall_Right_Boundary", pillarTile, new Vector3(1230f, boundaryCenterY, 0f), new Vector2(1.5f, boundaryHeight), true, 0);

            // Zona inicial de bienvenida (X = -30 a 0)
            CreateInteriorLabBackWall(env.transform, "Wall_Bg_Entry_Foyer", new Vector3(-15f, 6.2f, 0f), new Vector2(30f, 18.0f));
            CreateLabStaticProp(env.transform, "Door_Entry_Start", "Lab_Door", new Vector3(-26.5f, -0.40f, 0f), 2.70f, -3);
            CreateLabSignWithGlow(env.transform, "Lab_Sign_Entry", theme.sectorSigns[0], new Vector3(-14f, 2.85f, 0f), glowSprite, theme.accentColor);
            CreateLabStaticProp(env.transform, "Whiteboard_Entry", "Lab_Whiteboard", new Vector3(-14f, 0.85f, 0f), 2.85f, -3);
            CreateCeilingLampWithLightCone(env.transform, "Lamp_Entry_1", new Vector3(-20f, 5.2f, 0f), glowSprite);
            CreateCeilingLampWithLightCone(env.transform, "Lamp_Entry_2", new Vector3(-8f, 5.2f, 0f), glowSprite);
            CreateComputerWorkstation(env.transform, "WS_Entry_Reception", new Vector3(-6.0f, -1.75f, 0f), glowSprite, 0);

            string[] sectorSigns = theme.sectorSigns;
            Color[] sectorColors = theme.sectorColors;

            for (int m = 0; m < 5; m++)
            {
                float bX = m * 230f; // 0, 230, 460, 690, 920

                // =========================================================================================
                // ARQUITECTURA DE MUROS EN CAPAS: Alternancia entre ZONAS ILUMINADAS (Aulas con Ventanas)
                // y ZONAS OSCURAS DE SERVIDORES (Bóvedas Acústicas donde resaltan luces de PCs y Racks)
                // =========================================================================================
                CreateInteriorLabBackWall(env.transform, $"Wall_Bg_S{m}_WestClassroom", new Vector3(bX + 21f, 6.2f, 0f), new Vector2(42f, 18.0f));
                CreateDarkServerZoneWall(env.transform, $"Wall_Bg_S{m}_DarkVault_West", new Vector3(bX + 68.5f, 6.2f, 0f), new Vector2(53f, 18.0f));
                CreateInteriorLabBackWall(env.transform, $"Wall_Bg_S{m}_MidAtrium", new Vector3(bX + 102f, 6.2f, 0f), new Vector2(14f, 18.0f));
                CreateInteriorLabBackWall(env.transform, $"Wall_Bg_S{m}_EastClassroom", new Vector3(bX + 205.5f, 6.2f, 0f), new Vector2(49f, 18.0f));

                // Letrero luminoso del Sector y Puerta de Laboratorio
                CreateLabSignWithGlow(env.transform, $"Lab_Sign_Main_S{m}", sectorSigns[m], new Vector3(bX + 9f, 3.65f, 0f), glowSprite, sectorColors[m]);
                CreateLabStaticProp(env.transform, $"Lab_Door_West_S{m}", "Lab_Door", new Vector3(bX + 1.8f, -0.40f, 0f), 2.70f, -3);

                // 1. Sub-sección Izquierda (Aula de Cómputo P1 y P2 + Bóveda Oscura escalonada P1-P4)
                CreateTiledPlatform(env.transform, $"Wall_S{m}_BlockA", pillarTile, new Vector3(bX + 26f, 0.05f, 0f), new Vector2(1.4f, 3.6f), true, 1);
                CreateTiledPlatform(env.transform, $"Platform_S{m}_P2_A", steelBeamTile, new Vector3(bX + 22f, FloorCenterY(2), 0f), new Vector2(30f, 0.8f), true, 0);
                CreateClimbableLadder(env.transform, $"Ladder_S{m}_1to2_A", ladderMidTile, new Vector3(bX + 11f, LadderCenterY(1), 0f), 3.60f);

                CreateComputerWorkstation(env.transform, $"WS_S{m}_P2_A1", new Vector3(bX + 16f, FloorSurfaceY(2), 0f), glowSprite, m + 1);
                CreateComputerWorkstation(env.transform, $"WS_S{m}_P2_A2", new Vector3(bX + 29f, FloorSurfaceY(2), 0f), glowSprite, m + 2);
                CreateLabStaticProp(env.transform, $"Whiteboard_S{m}_P2", "Lab_Whiteboard", new Vector3(bX + 22.5f, FloorSurfaceY(2) + 1.45f, 0f), 2.65f, -3);
                CreateCeilingLampWithLightCone(env.transform, $"Lamp_S{m}_P2_A", new Vector3(bX + 22.5f, FloorSurfaceY(2) + 3.25f, 0f), glowSprite);

                CreateTiledPlatform(env.transform, $"Wall_S{m}_BlockB", pillarTile, new Vector3(bX + 42f, 5.45f, 0f), new Vector2(1.4f, 7.2f), true, 1);
                CreateClimbableLadder(env.transform, $"Ladder_S{m}_2to1_Drop", ladderMidTile, new Vector3(bX + 34f, LadderCenterY(1), 0f), 3.60f);

                CreateTiledPlatform(env.transform, $"Wall_S{m}_BlockC", pillarTile, new Vector3(bX + 68f, 3.65f, 0f), new Vector2(1.5f, 10.8f), true, 1);

                CreateTiledPlatform(env.transform, $"Platform_S{m}_P2_B", steelBeamTile, new Vector3(bX + 55f, FloorCenterY(2), 0f), new Vector2(22f, 0.8f), true, 0);
                CreateClimbableLadder(env.transform, $"Ladder_S{m}_1to2_B", ladderMidTile, new Vector3(bX + 47f, LadderCenterY(1), 0f), 3.60f);
                CreateComputerWorkstation(env.transform, $"WS_S{m}_P2_B", new Vector3(bX + 55f, FloorSurfaceY(2), 0f), glowSprite, m + 3);
                CreateServerRackUnit(env.transform, $"Rack_DarkVault_S{m}_P2", new Vector3(bX + 61f, FloorSurfaceY(2), 0f), glowSprite, m + 1, isDarkZone: true);

                CreateTiledPlatform(env.transform, $"Platform_S{m}_P3_B", hazardBeamTile, new Vector3(bX + 57f, FloorCenterY(3), 0f), new Vector2(20f, 0.8f), true, 0);
                CreateClimbableLadder(env.transform, $"Ladder_S{m}_2to3_B", ladderMidTile, new Vector3(bX + 63f, LadderCenterY(2), 0f), 3.60f);
                CreateComputerWorkstation(env.transform, $"WS_S{m}_P3_B", new Vector3(bX + 57f, FloorSurfaceY(3), 0f), glowSprite, m + 4);
                CreateServerRackUnit(env.transform, $"Rack_DarkVault_S{m}_P3", new Vector3(bX + 51.5f, FloorSurfaceY(3), 0f), glowSprite, m + 2, isDarkZone: true);

                CreateTiledPlatform(env.transform, $"Platform_S{m}_P4_Bridge", steelBeamTile, new Vector3(bX + 68f, FloorCenterY(4), 0f), new Vector2(28f, 0.8f), true, 0);
                CreateClimbableLadder(env.transform, $"Ladder_S{m}_3to4_B", ladderMidTile, new Vector3(bX + 51f, LadderCenterY(3), 0f), 3.60f);
                CreateComputerWorkstation(env.transform, $"WS_S{m}_P4_Bridge", new Vector3(bX + 68f, FloorSurfaceY(4), 0f), glowSprite, m + 5);
                CreateLabStaticProp(env.transform, $"WallDisplay_Bridge_S{m}", (m % 2 == 0) ? theme.wallDisplayA : theme.wallDisplayB,
                    new Vector3(bX + 62f, FloorSurfaceY(4) + 1.55f, 0f), 3.1f, -3);

                // Descenso escalonado al Este de BlockC
                CreateTiledPlatform(env.transform, $"Platform_S{m}_P3_EastStep", hazardBeamTile, new Vector3(bX + 82f, FloorCenterY(3), 0f), new Vector2(18f, 0.8f), true, 0);
                CreateClimbableLadder(env.transform, $"Ladder_S{m}_4to3_East", ladderMidTile, new Vector3(bX + 77f, LadderCenterY(3), 0f), 3.60f);
                CreateServerRackUnit(env.transform, $"Rack_EastStep_S{m}_P3", new Vector3(bX + 84f, FloorSurfaceY(3), 0f), glowSprite, m + 3, isDarkZone: true);

                CreateTiledPlatform(env.transform, $"Platform_S{m}_P2_EastStep", steelBeamTile, new Vector3(bX + 88f, FloorCenterY(2), 0f), new Vector2(18f, 0.8f), true, 0);
                CreateClimbableLadder(env.transform, $"Ladder_S{m}_3to2_East", ladderMidTile, new Vector3(bX + 86f, LadderCenterY(2), 0f), 3.60f);
                CreateClimbableLadder(env.transform, $"Ladder_S{m}_2to1_East", ladderMidTile, new Vector3(bX + 94f, LadderCenterY(1), 0f), 3.60f);
                CreateComputerWorkstation(env.transform, $"WS_EastStep_S{m}_P2", new Vector3(bX + 90f, FloorSurfaceY(2), 0f), glowSprite, m + 6);

                // Tuberías de enfriamiento y charolas de cables en la Bóveda Oscura Oeste
                if (pipeHTile != null)
                {
                    CreateTiledPlatform(env.transform, $"PipeH_DarkVault_S{m}", pipeHTile, new Vector3(bX + 68f, 1.35f, 0f), new Vector2(48f, 0.42f), false, -6);
                }
                if (cableTrayTile != null)
                {
                    CreateTiledPlatform(env.transform, $"CableTray_DarkVault_S{m}", cableTrayTile, new Vector3(bX + 68f, 4.95f, 0f), new Vector2(46f, 0.46f), false, -6);
                }
                CreateLabStaticProp(env.transform, $"CablesHang_VaultW_S{m}", "Lab_CablesHang", new Vector3(bX + 55f, 1.15f, 0f), 3.2f, -5);
                CreateLabStaticProp(env.transform, $"CablesHang_VaultE_S{m}", "Lab_CablesHang", new Vector3(bX + 82f, 1.15f, 0f), 3.2f, -5);
                CreateExtractorVent(env.transform, $"Vent_VaultW_S{m}", new Vector3(bX + 49f, 0.45f, 0f));
                CreateExtractorVent(env.transform, $"Vent_VaultE_S{m}", new Vector3(bX + 75f, 0.45f, 0f));

                // 2. Sub-sección Central del Módulo: Gran Torre Vertical de Servidores (Mainframe Data Center)
                int topFloor = theme.topFloors[m];
                float wallHeight = (topFloor - 1) * 3.6f;

                // Los pisos inferiores/medios de la Torre son Bóveda Oscura de Servidores; la cima es Observatorio Iluminado
                float darkTowerHeight = Mathf.Max(14f, wallHeight - 7.2f);
                CreateDarkServerZoneWall(env.transform, $"Wall_Bg_Tower_Dark_S{m}",
                    new Vector3(bX + 145f, -1.75f + darkTowerHeight * 0.5f, 0f), new Vector2(78f, darkTowerHeight));
                CreateInteriorLabBackWall(env.transform, $"Wall_Bg_Tower_Top_S{m}",
                    new Vector3(bX + 145f, -1.75f + darkTowerHeight + 5.5f, 0f), new Vector2(78f, 11.0f));

                CreateTiledPlatform(env.transform, $"Wall_S{m}_MainframeTower", pillarTile,
                    new Vector3(bX + 145f, -1.75f + wallHeight * 0.5f, 0f), new Vector2(1.6f, wallHeight), true, 1);

                // Tuberías verticales de refrigeración líquida subiendo por la columna central del Mainframe
                if (pipeVTile != null)
                {
                    CreateTiledPlatform(env.transform, $"PipeV_Tower_W_S{m}", pipeVTile,
                        new Vector3(bX + 143.4f, -1.75f + wallHeight * 0.5f, 0f), new Vector2(0.42f, wallHeight), false, -5);
                    CreateTiledPlatform(env.transform, $"PipeV_Tower_E_S{m}", pipeVTile,
                        new Vector3(bX + 146.6f, -1.75f + wallHeight * 0.5f, 0f), new Vector2(0.42f, wallHeight), false, -5);
                }

                float[] westLadderOffsets = new float[] { 113f, 137f, 121f, 129f };
                float[] eastLadderOffsets = new float[] { 175f, 153f, 168f, 160f };

                for (int f = 2; f <= topFloor; f++)
                {
                    bool isTopObservatory = (f >= topFloor - 1);
                    float westWidth = (f == topFloor) ? 54f : 32f;
                    float westCenterX = (f == topFloor) ? (bX + 145f) : (bX + 125f);
                    CreateTiledPlatform(env.transform, $"Platform_S{m}_Tower_P{f}_W", (f % 2 == 0) ? steelBeamTile : hazardBeamTile,
                        new Vector3(westCenterX, FloorCenterY(f), 0f), new Vector2(westWidth, 0.8f), true, 0);

                    float ladderWestX = bX + westLadderOffsets[f % 4];
                    CreateClimbableLadder(env.transform, $"Ladder_S{m}_Tower_W_{f - 1}to{f}", ladderMidTile,
                        new Vector3(ladderWestX, LadderCenterY(f - 1), 0f), 3.60f);

                    Vector3 westPropPos = new Vector3(bX + ((f % 2 == 0) ? 133f : 117f), FloorSurfaceY(f), 0f);
                    CreateComputerWorkstation(env.transform, $"TowerWS_W_S{m}_F{f}", westPropPos, glowSprite, m + f);

                    // Racks de Servidores con LEDs emisivos en todos los pisos de la Torre
                    Vector3 rackPosW = new Vector3(bX + 125f, FloorSurfaceY(f), 0f);
                    CreateServerRackUnit(env.transform, $"ServerRack_W_S{m}_F{f}", rackPosW, glowSprite, m + f, isDarkZone: !isTopObservatory);

                    if (f % 3 == 0)
                    {
                        CreateLabStaticProp(env.transform, $"TowerCables_W_S{m}_F{f}", "Lab_CablesHang",
                            new Vector3(bX + 125f, FloorSurfaceY(f) + 2.55f, 0f), 3.0f, -5);
                        CreateExtractorVent(env.transform, $"TowerVent_W_S{m}_F{f}", new Vector3(bX + 139f, FloorSurfaceY(f) + 1.65f, 0f));
                    }
                    if (f % 4 == 0)
                    {
                        CreateLabStaticProp(env.transform, $"TowerDisplay_W_S{m}_F{f}",
                            (f % 8 == 0) ? "Lab_WallDisplay_Eye" : "Lab_WallDisplay_Net",
                            new Vector3(bX + 129f, FloorSurfaceY(f) + 1.65f, 0f), 2.8f, -4);
                    }

                    if (f < topFloor)
                    {
                        float eastCenterX = bX + 165f;
                        CreateTiledPlatform(env.transform, $"Platform_S{m}_Tower_P{f}_E", (f % 2 == 0) ? hazardBeamTile : steelBeamTile,
                            new Vector3(eastCenterX, FloorCenterY(f), 0f), new Vector2(32f, 0.8f), true, 0);

                        float ladderEastX = bX + eastLadderOffsets[f % 4];
                        CreateClimbableLadder(env.transform, $"Ladder_S{m}_Tower_E_{f}to{f - 1}", ladderMidTile,
                            new Vector3(ladderEastX, LadderCenterY(f - 1), 0f), 3.60f);

                        Vector3 eastPropPos = new Vector3(bX + ((f % 2 == 0) ? 156f : 172f), FloorSurfaceY(f), 0f);
                        CreateComputerWorkstation(env.transform, $"TowerWS_E_S{m}_F{f}", eastPropPos, glowSprite, m + f + 3);

                        Vector3 rackPosE = new Vector3(bX + 164f, FloorSurfaceY(f), 0f);
                        CreateServerRackUnit(env.transform, $"ServerRack_E_S{m}_F{f}", rackPosE, glowSprite, m + f + 1, isDarkZone: !isTopObservatory);

                        if (f % 3 == 0)
                        {
                            CreateLabStaticProp(env.transform, $"LabCabinet_E_S{m}_F{f}", "Lab_Cabinet",
                                new Vector3(bX + 176f, FloorSurfaceY(f) + 0.92f, 0f), 1.85f, -3);
                            CreateExtractorVent(env.transform, $"TowerVent_E_S{m}_F{f}", new Vector3(bX + 151f, FloorSurfaceY(f) + 1.65f, 0f));
                        }
                    }
                }
                CreateClimbableLadder(env.transform, $"Ladder_S{m}_Tower_TopDrop", ladderMidTile,
                    new Vector3(bX + 164f, LadderCenterY(topFloor - 1), 0f), 3.60f);

                // Letrero y lámparas en la cima de cada Torre de Servidores
                CreateLabSignWithGlow(env.transform, $"TowerTop_Sign_S{m}", (m == 2) ? "Lab_Sign_Core" : sectorSigns[m],
                    new Vector3(bX + 145f, FloorSurfaceY(topFloor) + 4.2f, 0f), glowSprite, sectorColors[m]);
                CreateCeilingLampWithLightCone(env.transform, $"TowerTop_LampL_S{m}", new Vector3(bX + 132f, FloorSurfaceY(topFloor) + 4.8f, 0f), glowSprite);
                CreateCeilingLampWithLightCone(env.transform, $"TowerTop_LampR_S{m}", new Vector3(bX + 158f, FloorSurfaceY(topFloor) + 4.8f, 0f), glowSprite);

                // 3. Ambientación de Laboratorio de Cómputo en Planta 1 (Ventanas con vista a la ciudad, lámparas, pizarrones, impresoras y racks)
                CreateLabWindowWithLight(env.transform, $"Window_S{m}_West", new Vector3(bX + 18f, 4.2f, 0f), glowSprite);
                CreateLabWindowWithLight(env.transform, $"Window_S{m}_Mid", new Vector3(bX + 102f, 4.2f, 0f), glowSprite);
                CreateLabWindowWithLight(env.transform, $"Window_S{m}_High", new Vector3(bX + 145f, FloorSurfaceY(topFloor) + 2.2f, 0f), glowSprite);
                CreateLabWindowWithLight(env.transform, $"Window_S{m}_East", new Vector3(bX + 205f, 4.2f, 0f), glowSprite);

                // Lámparas de techo sólo en las Zonas Iluminadas (Oeste, Atrio Central y Este), dejando las Bóvedas de Servidores en penumbra
                CreateCeilingLampWithLightCone(env.transform, $"Lamp_S{m}_W1", new Vector3(bX + 8f, 1.35f, 0f), glowSprite);
                CreateCeilingLampWithLightCone(env.transform, $"Lamp_S{m}_W2", new Vector3(bX + 20f, 1.35f, 0f), glowSprite);
                CreateCeilingLampWithLightCone(env.transform, $"Lamp_S{m}_Mid", new Vector3(bX + 102f, 5.2f, 0f), glowSprite);
                CreateCeilingLampWithLightCone(env.transform, $"Lamp_S{m}_E1", new Vector3(bX + 190f, 5.2f, 0f), glowSprite);
                CreateCeilingLampWithLightCone(env.transform, $"Lamp_S{m}_E2", new Vector3(bX + 214f, 5.2f, 0f), glowSprite);

                // Mobiliario académico e informático de Planta 1 (Pizarrones, Posters, Impresoras, Dispensadores, Gabinetes, Plantas y Puertas)
                CreateLabClassroomDecorSet(env.transform, bX, m, arrowRightTile, arrowUpTile);

                float[] labClusterX = { bX + 6f, bX + 21f, bX + 38f, bX + 60f, bX + 82f, bX + 104f, bX + 122f, bX + 158f, bX + 182f, bX + 206f };
                for (int c = 0; c < labClusterX.Length; c++)
                {
                    bool inDarkZone = (c == 3 || c == 4 || c == 6 || c == 7);
                    CreateComputerWorkstation(env.transform, $"ComputerCluster_S{m}_{c}", new Vector3(labClusterX[c], -1.75f, 0f), glowSprite, m * 5 + c);
                    if (c % 2 == 1 || inDarkZone)
                    {
                        Vector3 groundRackPos = new Vector3(labClusterX[c] + 3.6f, -1.75f, 0f);
                        CreateServerRackUnit(env.transform, $"GroundServerRack_S{m}_{c}", groundRackPos, glowSprite, m + c, isDarkZone: inDarkZone);
                    }
                }
                // Elementos exclusivos de la temática del nivel (Redes / Torre / Núcleo)
                AddThemeSpecificDecor(env.transform, theme, m, bX, topFloor, glowSprite);
            }

            // 8 Checkpoints distribuidos a lo largo de los 1,200m (Checkpoint_8 justo antes de entrar a la arena del Jefe)
            CreateCheckpointStation(env.transform, "Checkpoint_1", terminalScreenTile, sirenRedTile, new Vector3(102f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_2", terminalScreenTile, sirenRedTile, new Vector3(248f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_3", terminalScreenTile, sirenRedTile, new Vector3(428f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_4", terminalScreenTile, sirenRedTile, new Vector3(605f, FloorSurfaceY(theme.topFloors[2]), 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_5", terminalScreenTile, sirenRedTile, new Vector3(792f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_6", terminalScreenTile, sirenRedTile, new Vector3(906f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_7", terminalScreenTile, sirenRedTile, new Vector3(1022f, -1.75f, 0f), glowSprite);
            CreateCheckpointStation(env.transform, "Checkpoint_8", terminalScreenTile, sirenRedTile, new Vector3(1158f, -1.75f, 0f), glowSprite);

            // =========================================================================================
            // ARENA DEL JEFE FINAL (X = 1168 a 1230): NÚCLEO CENTRAL S.I.A.
            // =========================================================================================
            CreateDarkServerZoneWall(env.transform, "Wall_Bg_BossArena_Core", new Vector3(1196f, 6.2f, 0f), new Vector2(72f, 18.0f));
            CreateBossArenaContainmentGate(env.transform, gateX: 1168.0f, triggerX: 1174.0f, hazardBeamTile);
            CreateLabSignWithGlow(env.transform, "BossArena_CoreSign", "Lab_Sign_Core", new Vector3(1200f, 8.2f, 0f), glowSprite, theme.bossSignColor);

            CreateLabWindowWithLight(env.transform, "Window_BossArena_Left", new Vector3(1182f, 4.8f, 0f), glowSprite);
            CreateLabWindowWithLight(env.transform, "Window_BossArena_Center", new Vector3(1200f, 4.8f, 0f), glowSprite);
            CreateLabWindowWithLight(env.transform, "Window_BossArena_Right", new Vector3(1218f, 4.8f, 0f), glowSprite);

            CreateLabStaticProp(env.transform, "BossArena_Display_Left", "Lab_WallDisplay_Eye", new Vector3(1191f, 2.2f, 0f), 3.4f, -3);
            CreateLabStaticProp(env.transform, "BossArena_Display_Right", "Lab_WallDisplay_Eye", new Vector3(1209f, 2.2f, 0f), 3.4f, -3);

            for (int a = 0; a < 4; a++)
            {
                float arenaPropX = 1180f + a * 12f;
                CreateComputerWorkstation(env.transform, $"BossArena_Workstation_{a}", new Vector3(arenaPropX, -1.75f, 0f), glowSprite, a + 10);
                CreateServerRackUnit(env.transform, $"BossArena_ServerRack_{a}", new Vector3(arenaPropX + 4.2f, -1.75f, 0f), glowSprite, a + 2, isDarkZone: true);
            }

            // Tinte cromático de estructuras (suelos, plataformas, techo y muros) según la temática del nivel
            if (theme.platformTint != Color.white)
            {
                SpriteRenderer[] allRenderers = env.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < allRenderers.Length; i++)
                {
                    string n = allRenderers[i].gameObject.name;
                    if (n.StartsWith("Ground_") || n.StartsWith("Platform_") || n.StartsWith("Ceiling_") || n.StartsWith("Gate_Pillar") ||
                        (n.StartsWith("Wall_") && !n.StartsWith("Wall_Bg") && !n.StartsWith("Wall_Column") && !n.StartsWith("Wall_Baseboard") && !n.StartsWith("Wall_TopMolding")))
                    {
                        Color c = allRenderers[i].color;
                        allRenderers[i].color = new Color(c.r * theme.platformTint.r, c.g * theme.platformTint.g, c.b * theme.platformTint.b, c.a);
                    }
                }
            }
        }

        /// <summary>
        /// Decoración exclusiva de cada temática: Centro de Redes (fibra óptica y backbone), Torre de Servidores (columnas de
        /// refrigeración y pasarelas de datos) y Núcleo Central S.I.A. (alarmas rojas, pantallas de alerta y niebla de refrigerante).
        /// </summary>
        private static void AddThemeSpecificDecor(Transform env, LevelTheme theme, int m, float bX, int topFloor, Sprite glowSprite)
        {
            if (theme.decorStyle == 0) return;

            Sprite trayTile = GetLabGeneratedSprite("Lab_CableTray", 100f);
            Sprite pipeHTile = GetLabGeneratedSprite("Lab_PipeH", 100f);
            Sprite pipeVTile = GetLabGeneratedSprite("Lab_PipeV", 100f);

            if (theme.decorStyle == 1)
            {
                // CENTRO DE REDES: bandas de fibra óptica con luz ámbar, switches y paneles de red en el pasillo principal
                if (trayTile != null)
                {
                    CreateTiledPlatform(env, $"NetBackbone_Low_S{m}", trayTile, new Vector3(bX + 110f, 3.9f, 0f), new Vector2(210f, 0.46f), false, -6);
                    CreateTiledPlatform(env, $"NetBackbone_High_S{m}", trayTile, new Vector3(bX + 110f, 11.4f, 0f), new Vector2(210f, 0.46f), false, -7);
                }
                if (pipeHTile != null)
                {
                    CreateTiledPlatform(env, $"FiberConduit_S{m}", pipeHTile, new Vector3(bX + 110f, 7.7f, 0f), new Vector2(210f, 0.42f), false, -7);
                }
                for (int i = 0; i < 7; i++)
                {
                    float fx = bX + 14f + i * 32f;
                    CreatePointLightGlow(env, $"FiberGlow_S{m}_{i}", new Vector3(fx, 3.9f, 0f), glowSprite, new Color(1.0f, 0.62f, 0.18f, 0.22f), 5.2f);
                    CreateLabStaticProp(env, $"NetSwitch_S{m}_{i}", "Lab_ServerRack_B", new Vector3(fx + 5f, -0.20f, 0f), 2.2f, -3);
                    CreateLabStaticProp(env, $"NetPanel_S{m}_{i}", (i % 2 == 0) ? "Lab_Screen_Graph" : "Lab_Screen_Code", new Vector3(fx + 11f, 1.65f, 0f), 1.5f, -3);
                }
            }
            else if (theme.decorStyle == 2)
            {
                // TORRE DE SERVIDORES: columnas verticales de refrigeración líquida y racks monumentales a lo alto de cada torre
                if (pipeVTile != null)
                {
                    float coolantHeight = (topFloor - 1) * 3.6f;
                    for (int i = 0; i < 4; i++)
                    {
                        float px = bX + 108f + i * 18f;
                        CreateTiledPlatform(env, $"CoolantColumn_S{m}_{i}", pipeVTile, new Vector3(px, -1.75f + coolantHeight * 0.5f, 0f), new Vector2(0.55f, coolantHeight), false, -8);
                    }
                }
                for (int f = 3; f <= topFloor; f += 3)
                {
                    CreatePointLightGlow(env, $"TowerGlow_S{m}_F{f}", new Vector3(bX + 145f, FloorSurfaceY(f) + 1.8f, 0f), glowSprite, new Color(0.70f, 0.45f, 1.0f, 0.20f), 7.0f);
                    CreateLabStaticProp(env, $"TowerStatus_S{m}_F{f}", (f % 2 == 0) ? "Lab_Screen_Blue" : "Lab_Screen_Graph", new Vector3(bX + 141f, FloorSurfaceY(f) + 1.7f, 0f), 1.5f, -3);
                }
                for (int i = 0; i < 5; i++)
                {
                    CreateLabStaticProp(env, $"HallRackLeds_S{m}_{i}", (i % 2 == 0) ? "Lab_RackLeds_A" : "Lab_RackLeds_B", new Vector3(bX + 20f + i * 44f, 1.2f, 0f), 2.0f, -4);
                }
            }
            else
            {
                // NÚCLEO CENTRAL S.I.A.: luces de alarma rojas, pantallas de alerta y niebla de refrigerante a ras de suelo
                for (int i = 0; i < 8; i++)
                {
                    float ax = bX + 10f + i * 27f;
                    CreatePointLightGlow(env, $"AlarmGlow_S{m}_{i}", new Vector3(ax, 4.6f, 0f), glowSprite, new Color(1.0f, 0.12f, 0.22f, 0.24f), 6.0f);
                    CreateLabStaticProp(env, $"AlertScreen_S{m}_{i}", "Lab_Screen_Alert", new Vector3(ax + 6f, 1.8f, 0f), 1.6f, -3);
                    CreateLabStaticProp(env, $"CoreFog_S{m}_{i}", "Lab_Fog", new Vector3(ax + 13f, -0.9f, 0f), 4.2f, -5);
                }
                if (trayTile != null)
                {
                    CreateTiledPlatform(env, $"CoreConduit_S{m}", trayTile, new Vector3(bX + 110f, 6.3f, 0f), new Vector2(210f, 0.5f), false, -6);
                }
                if (pipeVTile != null)
                {
                    float coolantHeight = (topFloor - 1) * 3.6f;
                    CreateTiledPlatform(env, $"CoreColumn_L_S{m}", pipeVTile, new Vector3(bX + 129f, -1.75f + coolantHeight * 0.5f, 0f), new Vector2(0.6f, coolantHeight), false, -8);
                    CreateTiledPlatform(env, $"CoreColumn_R_S{m}", pipeVTile, new Vector3(bX + 161f, -1.75f + coolantHeight * 0.5f, 0f), new Vector2(0.6f, coolantHeight), false, -8);
                }
            }
        }

        private static float FloorSurfaceY(int floorNumber)
        {
            return -1.75f + (floorNumber - 1) * 3.6f;
        }

        private static float FloorCenterY(int floorNumber)
        {
            return FloorSurfaceY(floorNumber) - 0.4f;
        }

        private static float LadderCenterY(int fromFloorNumber)
        {
            return FloorSurfaceY(fromFloorNumber) + 1.80f;
        }

        // =================================================================================================
        // ARMONIZACIÓN INTEGRAL DEL LABORATORIO DE CÓMPUTO + ILUMINACIÓN 2D POR ZONAS (ISSUE #05)
        // =================================================================================================

        private static Sprite GetLabGeneratedSprite(string fileNameWithoutExt, float ppu)
        {
            string path = $"{LabGeneratedFolder}/{fileNameWithoutExt}.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return null;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }
            if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.5f)
            {
                importer.spritePixelsPerUnit = ppu;
                changed = true;
            }
            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteMeshType != SpriteMeshType.FullRect)
            {
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>
        /// Construye el muro interior de un Aula Iluminada del Laboratorio de Cómputo usando paneles arquitectónicos
        /// en mosaico (Lab_WallPanelA), molduras superior/inferior (Lab_Trim), charola de cables (Lab_CableTray) y columnas.
        /// </summary>
        private static void CreateInteriorLabBackWall(Transform parent, string name, Vector3 pos, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;

            Sprite panelSprite = GetLabGeneratedSprite("Lab_WallPanelA", 72f);
            Sprite trimSprite = GetLabGeneratedSprite("Lab_Trim", 100f);
            Sprite traySprite = GetLabGeneratedSprite("Lab_CableTray", 100f);
            Sprite pillarSprite = GetLabGeneratedSprite("Lab_Pillar", 64f);

            SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
            if (panelSprite != null)
            {
                sr.sprite = panelSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = size;
                sr.color = activeTheme.lightWallTint;
            }
            else
            {
                sr.sprite = GetOrCreateSquareSprite();
                wall.transform.localScale = new Vector3(size.x, size.y, 1f);
                sr.color = new Color(0.08f, 0.13f, 0.22f, 0.95f);
            }
            sr.sortingOrder = -12;

            if (trimSprite != null)
            {
                CreateTiledPlatform(wall.transform, "Wall_Baseboard", trimSprite,
                    pos + new Vector3(0f, -size.y * 0.5f + 0.20f, 0f), new Vector2(size.x, 0.36f), false, -11);
                CreateTiledPlatform(wall.transform, "Wall_TopMolding", trimSprite,
                    pos + new Vector3(0f, size.y * 0.5f - 0.20f, 0f), new Vector2(size.x, 0.36f), false, -11);
            }

            if (traySprite != null && size.x >= 12f)
            {
                CreateTiledPlatform(wall.transform, "Wall_Overhead_CableTray", traySprite,
                    pos + new Vector3(0f, -size.y * 0.5f + 5.15f, 0f), new Vector2(size.x - 1.5f, 0.45f), false, -10);
            }

            if (pillarSprite != null && size.x >= 24f)
            {
                int pillarCount = Mathf.Max(2, Mathf.RoundToInt(size.x / 20f));
                float step = size.x / pillarCount;
                for (int i = 1; i < pillarCount; i++)
                {
                    float px = pos.x - size.x * 0.5f + i * step;
                    CreateTiledPlatform(wall.transform, $"Wall_Column_{i}", pillarSprite,
                        new Vector3(px, pos.y, 0f), new Vector2(0.85f, size.y), false, -11);
                }
            }
        }

        /// <summary>
        /// Construye el muro acústico oscuro para las Bóvedas de Servidores (Lab_WallPanelB) con penumbra ambiental
        /// para que las pantallas de las computadoras y los LEDs de los servidores resalten con contraste real.
        /// </summary>
        private static void CreateDarkServerZoneWall(Transform parent, string name, Vector3 pos, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;

            Sprite darkPanelSprite = GetLabGeneratedSprite("Lab_WallPanelB", 72f);
            Sprite trimSprite = GetLabGeneratedSprite("Lab_Trim", 100f);

            SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
            if (darkPanelSprite != null)
            {
                sr.sprite = darkPanelSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = size;
                sr.color = activeTheme.darkWallTint;
            }
            else
            {
                sr.sprite = GetOrCreateSquareSprite();
                wall.transform.localScale = new Vector3(size.x, size.y, 1f);
                sr.color = new Color(0.03f, 0.05f, 0.09f, 0.96f);
            }
            sr.sortingOrder = -12;

            // Velo de penumbra localizada (sortingOrder = -5, detrás de los monitores y LEDs en -2/-1 y de las luces en -4)
            GameObject shadowVeil = new GameObject(name + "_AmbientDarkness");
            shadowVeil.transform.SetParent(wall.transform, false);
            shadowVeil.transform.position = pos;
            shadowVeil.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer srVeil = shadowVeil.AddComponent<SpriteRenderer>();
            srVeil.sprite = GetOrCreateSquareSprite();
            srVeil.color = activeTheme.darkVeilColor;
            srVeil.sortingOrder = -5;

            if (trimSprite != null)
            {
                CreateTiledPlatform(wall.transform, "DarkWall_Baseboard", trimSprite,
                    pos + new Vector3(0f, -size.y * 0.5f + 0.20f, 0f), new Vector2(size.x, 0.36f), false, -11);
            }
        }

        private static void SetupLayeredParallaxBackground(Transform camTransform, float totalCoverageMeters)
        {
            GameObject bgRoot = GameObject.Find("--- Background_Parallax ---");
            if (bgRoot != null) UnityEngine.Object.DestroyImmediate(bgRoot);
            bgRoot = new GameObject("--- Background_Parallax ---");

            Sprite citySky = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/World/Backgrounds_Cyberpunk_City/city 2/1.png", 32f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            Sprite cityFarTowers = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/World/Backgrounds_Cyberpunk_City/city 2/5.png", 32f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            Sprite cityMidBuildings = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/World/Backgrounds_Cyberpunk_City/city 2/8.png", 32f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);

            CreateParallaxLayer(bgRoot.transform, "Parallax_Layer_01_Sky", citySky, camTransform, 0.88f, -30, activeTheme.parallaxSky);
            CreateParallaxLayer(bgRoot.transform, "Parallax_Layer_02_FarCity", cityFarTowers, camTransform, 0.62f, -25, activeTheme.parallaxFar);
            CreateParallaxLayer(bgRoot.transform, "Parallax_Layer_03_NearLabs", cityMidBuildings, camTransform, 0.35f, -20, activeTheme.parallaxNear);
        }

        private static void CreateParallaxLayer(Transform parent, string layerName, Sprite sprite, Transform camTransform, float parallaxFactor, int sortingOrder, Color tint)
        {
            if (sprite == null) return;

            GameObject layerObj = new GameObject(layerName);
            layerObj.transform.SetParent(parent, false);

            float panelWidth = 36f;
            Transform[] panels = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject p = new GameObject($"{layerName}_Panel_{i}");
                p.transform.SetParent(layerObj.transform, false);
                p.transform.localPosition = new Vector3((i - 2) * panelWidth, 6.0f, 0f);
                p.transform.localScale = new Vector3(2.2f, 2.6f, 1f);

                SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = tint;
                sr.sortingOrder = sortingOrder;
                panels[i] = p.transform;
            }

            ParallaxBackground parallax = layerObj.AddComponent<ParallaxBackground>();
            SerializedObject so = new SerializedObject(parallax);
            so.FindProperty("parallaxEffectX").floatValue = parallaxFactor;
            so.FindProperty("cameraTransform").objectReferenceValue = camTransform;
            so.FindProperty("panelWidth").floatValue = panelWidth;
            SerializedProperty panelsProp = so.FindProperty("panels");
            panelsProp.arraySize = panels.Length;
            for (int i = 0; i < panels.Length; i++)
            {
                panelsProp.GetArrayElementAtIndex(i).objectReferenceValue = panels[i];
            }
            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// Crea un ventanal panorámico del Laboratorio de Cómputo con vista exterior a los edificios del campus/ciudad,
        /// marco metálico de 6 paneles (Lab_WindowFrame), reflejo diagonal en el cristal (Lab_WindowGlare) y haz de luz ambiental.
        /// </summary>
        private static void CreateLabWindowWithLight(Transform parent, string name, Vector3 pos, Sprite glowSprite)
        {
            GameObject win = new GameObject(name);
            win.transform.SetParent(parent, false);
            win.transform.position = pos;

            Sprite cityViewSprite = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/World/Backgrounds_Cyberpunk_City/city 2/10.png", 100f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            Sprite frameSprite = GetLabGeneratedSprite("Lab_WindowFrame", 100f);
            Sprite glareSprite = GetLabGeneratedSprite("Lab_WindowGlare", 100f);
            Sprite coneSprite = GetLabGeneratedSprite("Lab_LightCone", 100f);
            Sprite fogSprite = GetLabGeneratedSprite("Lab_Fog", 100f);

            // 1. Paisaje exterior nocturno/ciberpunk visible a través del ventanal
            if (cityViewSprite != null)
            {
                GameObject extView = new GameObject("Window_Exterior_Skyline");
                extView.transform.SetParent(win.transform, false);
                extView.transform.localPosition = new Vector3(0f, 0.08f, 0f);
                SpriteRenderer srExt = extView.AddComponent<SpriteRenderer>();
                srExt.sprite = cityViewSprite;
                srExt.color = new Color(0.78f, 0.90f, 1.0f, 0.96f);
                srExt.sortingOrder = -10;
                extView.transform.localScale = new Vector3(5.95f / Mathf.Max(0.1f, cityViewSprite.bounds.size.x),
                                                           2.72f / Mathf.Max(0.1f, cityViewSprite.bounds.size.y), 1f);
            }

            // 2. Marco metálico de 6 paneles con alféizar
            GameObject frame = new GameObject("Window_Frame");
            frame.transform.SetParent(win.transform, false);
            SpriteRenderer srFrame = frame.AddComponent<SpriteRenderer>();
            if (frameSprite != null)
            {
                srFrame.sprite = frameSprite;
                srFrame.color = Color.white;
            }
            else
            {
                srFrame.sprite = GetOrCreateSquareSprite();
                frame.transform.localScale = new Vector3(6.4f, 3.3f, 1f);
                srFrame.color = new Color(0.16f, 0.26f, 0.40f, 0.96f);
            }
            srFrame.sortingOrder = -9;

            // 3. Reflejos diagonales sobre el cristal
            if (glareSprite != null)
            {
                GameObject glare = new GameObject("Window_Glass_Glare");
                glare.transform.SetParent(win.transform, false);
                SpriteRenderer srGlare = glare.AddComponent<SpriteRenderer>();
                srGlare.sprite = glareSprite;
                srGlare.color = new Color(0.85f, 0.96f, 1.0f, 0.85f);
                srGlare.sortingOrder = -8;
            }

            // 4. Haz volumétrico diagonal de luz ambiental entrando por la ventana
            if (coneSprite != null)
            {
                GameObject beam = new GameObject("Window_LightRay");
                beam.transform.SetParent(win.transform, false);
                beam.transform.localPosition = new Vector3(0.45f, -1.85f, 0f);
                beam.transform.localRotation = Quaternion.Euler(0f, 0f, -14f);
                beam.transform.localScale = new Vector3(2.35f, 1.55f, 1f);
                SpriteRenderer srBeam = beam.AddComponent<SpriteRenderer>();
                srBeam.sprite = coneSprite;
                srBeam.color = new Color(0.42f, 0.86f, 1.0f, 0.13f);
                srBeam.sortingOrder = -4;
            }

            // 5. Reflejo de luz de ventana sobre el piso cuando está en Planta 1
            if (fogSprite != null && pos.y < 6.0f)
            {
                GameObject floorPool = new GameObject("Window_FloorLightPool");
                floorPool.transform.SetParent(parent, false);
                floorPool.transform.position = new Vector3(pos.x + 0.6f, -1.62f, 0f);
                floorPool.transform.localScale = new Vector3(1.35f, 0.55f, 1f);
                SpriteRenderer srPool = floorPool.AddComponent<SpriteRenderer>();
                srPool.sprite = fogSprite;
                srPool.color = new Color(0.38f, 0.84f, 1.0f, 0.14f);
                srPool.sortingOrder = -3;
            }

            // 6. Halo ambiental suave de ventana (diámetro 4.6m, alpha 0.14)
            CreatePointLightGlow(parent, name + "_AmbientBeam", pos,
                glowSprite, new Color(0.32f, 0.86f, 1.0f, 0.14f), 4.6f);
        }

        /// <summary>
        /// Crea una lámpara fluorescente colgada del techo (Lab_CeilingLamp) con su cono de luz volumétrica (Lab_LightCone)
        /// para las aulas iluminadas del Laboratorio de Cómputo.
        /// </summary>
        private static void CreateCeilingLampWithLightCone(Transform parent, string name, Vector3 pos, Sprite glowSprite)
        {
            GameObject lampObj = new GameObject(name);
            lampObj.transform.SetParent(parent, false);
            lampObj.transform.position = pos;

            Sprite lampSprite = GetLabGeneratedSprite("Lab_CeilingLamp", 100f);
            Sprite coneSprite = GetLabGeneratedSprite("Lab_LightCone", 100f);

            if (lampSprite != null)
            {
                SpriteRenderer srLamp = lampObj.AddComponent<SpriteRenderer>();
                srLamp.sprite = lampSprite;
                srLamp.color = Color.white;
                srLamp.sortingOrder = -2;
            }

            if (coneSprite != null)
            {
                GameObject coneObj = new GameObject("Lamp_LightCone");
                coneObj.transform.SetParent(lampObj.transform, false);
                coneObj.transform.localPosition = new Vector3(0f, -1.85f, 0f);
                coneObj.transform.localScale = new Vector3(1.35f, 0.95f, 1f);
                SpriteRenderer srCone = coneObj.AddComponent<SpriteRenderer>();
                srCone.sprite = coneSprite;
                srCone.color = new Color(0.75f, 0.95f, 1.0f, 0.11f);
                srCone.sortingOrder = -4;
            }

            CreatePointLightGlow(parent, name + "_Glow", pos + new Vector3(0f, -0.35f, 0f),
                glowSprite, new Color(0.65f, 0.94f, 1.0f, 0.12f), 3.2f);
        }

        /// <summary>
        /// Escala automáticamente cualquier sprite de fondo para que su dimensión mayor en el mundo
        /// mida exactamente desiredMaxMeters (evitando íconos de 512px que midan 5 metros).
        /// </summary>
        private static void NormalizePropWorldScale(Transform propTransform, Sprite sprite, float desiredMaxMeters)
        {
            if (propTransform == null || sprite == null) return;
            float maxBound = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            if (maxBound <= 0.001f) maxBound = 1f;
            float uniformScale = desiredMaxMeters / maxBound;
            propTransform.localScale = new Vector3(uniformScale, uniformScale, 1f);
        }

        /// <summary>
        /// Crea una estación de cómputo completa sobre el piso (floorSurfacePos.y = superficie del suelo):
        /// Silla giratoria (Lab_Chair), Escritorio con cajonera (Lab_Desk), Teclado (Lab_Keyboard),
        /// Gabinete CPU (1 Monitor/6_2.png), Monitor CRT/LCD con pantalla encendida (Lab_Screen_Code/Blue/Graph/Alert)
        /// y luz localizada del color de la pantalla.
        /// </summary>
        private static void CreateComputerWorkstation(Transform parent, string name, Vector3 floorSurfacePos, Sprite glowSprite, int variant)
        {
            GameObject ws = new GameObject(name);
            ws.transform.SetParent(parent, false);
            ws.transform.position = floorSurfacePos;

            Sprite deskSprite = GetLabGeneratedSprite("Lab_Desk", 112f);
            Sprite chairSprite = GetLabGeneratedSprite("Lab_Chair", 96f);
            Sprite keyboardSprite = GetLabGeneratedSprite("Lab_Keyboard", 120f);

            string[] monitorPaths = new string[]
            {
                "Assets/_Project/Sprites/UI/Hacker_Terminal_Monitors/1 Monitor/1.png",
                "Assets/_Project/Sprites/UI/Hacker_Terminal_Monitors/1 Monitor/2.png",
                "Assets/_Project/Sprites/UI/Hacker_Terminal_Monitors/1 Monitor/3.png",
                "Assets/_Project/Sprites/UI/Hacker_Terminal_Monitors/1 Monitor/4.png",
                "Assets/_Project/Sprites/UI/Hacker_Terminal_Monitors/1 Monitor/5.png",
                "Assets/_Project/Sprites/UI/Hacker_Terminal_Monitors/1 Monitor/6.png"
            };
            // Centros normalizados de la pantalla dentro de cada uno de los 6 monitores CRT
            Vector2[] screenLocalOffsets = new Vector2[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, 0.06f),
                new Vector2(0.01f, 0.09f),
                new Vector2(0.0f, 0.14f),
                new Vector2(0.0f, 0.16f),
                new Vector2(0.0f, 0.16f)
            };
            float[] screenSizes = new float[] { 0.90f, 0.86f, 0.74f, 0.68f, 0.56f, 0.46f };

            string[] screenNames = new string[] { "Lab_Screen_Code", "Lab_Screen_Blue", "Lab_Screen_Graph", "Lab_Screen_Alert" };
            Color[] screenLightColors = new Color[]
            {
                new Color(0.15f, 1.0f, 0.65f, 0.16f),
                new Color(0.20f, 0.78f, 1.0f, 0.16f),
                new Color(0.18f, 0.92f, 1.0f, 0.16f),
                new Color(1.0f, 0.22f, 0.38f, 0.16f)
            };

            int monIdx = Mathf.Abs(variant) % monitorPaths.Length;
            int scrIdx = Mathf.Abs(variant) % screenNames.Length;

            Sprite monitorSprite = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                monitorPaths[monIdx], 100f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            Sprite screenSprite = GetLabGeneratedSprite(screenNames[scrIdx], 100f);
            Sprite cpuTowerSprite = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/UI/Hacker_Terminal_Monitors/1 Monitor/6_2.png", 100f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);

            // 1. Silla ergonómica de laboratorio detrás del escritorio
            if (chairSprite != null)
            {
                GameObject chairObj = new GameObject("Lab_Swivel_Chair");
                chairObj.transform.SetParent(ws.transform, false);
                chairObj.transform.localPosition = new Vector3(-0.28f, 0.62f, 0f);
                SpriteRenderer srChair = chairObj.AddComponent<SpriteRenderer>();
                srChair.sprite = chairSprite;
                srChair.sortingOrder = -4;
            }

            // 2. Escritorio metálico con cajonera apoyado sobre la superficie del piso
            GameObject desk = new GameObject("Lab_Desk_Base");
            desk.transform.SetParent(ws.transform, false);
            desk.transform.localPosition = new Vector3(0f, 0.46f, 0f);
            SpriteRenderer srDesk = desk.AddComponent<SpriteRenderer>();
            if (deskSprite != null)
            {
                srDesk.sprite = deskSprite;
                srDesk.color = Color.white;
            }
            else
            {
                srDesk.sprite = GetOrCreateSquareSprite();
                desk.transform.localScale = new Vector3(1.85f, 0.85f, 1f);
                srDesk.color = new Color(0.16f, 0.22f, 0.34f, 0.98f);
            }
            srDesk.sortingOrder = -3;

            // 3. Teclado sobre el escritorio
            if (keyboardSprite != null)
            {
                GameObject kbObj = new GameObject("Lab_Keyboard");
                kbObj.transform.SetParent(ws.transform, false);
                kbObj.transform.localPosition = new Vector3(-0.14f, 0.96f, 0f);
                SpriteRenderer srKb = kbObj.AddComponent<SpriteRenderer>();
                srKb.sprite = keyboardSprite;
                srKb.sortingOrder = -2;
            }

            // 4. Gabinete Torre CPU a un costado del monitor
            if (cpuTowerSprite != null)
            {
                GameObject cpuObj = new GameObject("Desktop_CPU_Tower");
                cpuObj.transform.SetParent(ws.transform, false);
                cpuObj.transform.localPosition = new Vector3(0.68f, 1.22f, 0f);
                SpriteRenderer srCpu = cpuObj.AddComponent<SpriteRenderer>();
                srCpu.sprite = cpuTowerSprite;
                srCpu.sortingOrder = -2;
                NormalizePropWorldScale(cpuObj.transform, cpuTowerSprite, 0.64f);
            }

            // 5. Monitor de cómputo sobre el escritorio
            GameObject monObj = new GameObject("Terminal_Monitor");
            monObj.transform.SetParent(ws.transform, false);
            monObj.transform.localPosition = new Vector3(-0.10f, 1.30f, 0f);
            SpriteRenderer srMon = monObj.AddComponent<SpriteRenderer>();
            srMon.sprite = monitorSprite != null ? monitorSprite : GetOrCreateSquareSprite();
            srMon.color = Color.white;
            srMon.sortingOrder = -2;
            NormalizePropWorldScale(monObj.transform, srMon.sprite, 1.05f);

            // 6. Pantalla encendida dentro del marco del monitor (Código, Sistema, Telemetría o Alerta S.I.A.)
            if (screenSprite != null)
            {
                GameObject scrObj = new GameObject("Monitor_Active_Screen");
                scrObj.transform.SetParent(ws.transform, false);
                Vector2 off = screenLocalOffsets[monIdx];
                scrObj.transform.localPosition = new Vector3(-0.10f + off.x, 1.30f + off.y, 0f);
                SpriteRenderer srScr = scrObj.AddComponent<SpriteRenderer>();
                srScr.sprite = screenSprite;
                srScr.color = Color.white;
                srScr.sortingOrder = -1;
                NormalizePropWorldScale(scrObj.transform, screenSprite, screenSizes[monIdx]);
            }

            // 7. Luz localizada del monitor del mismo color que su pantalla activa
            CreatePointLightGlow(parent, name + "_MonitorLight", floorSurfacePos + new Vector3(-0.10f, 1.32f, 0f),
                glowSprite, screenLightColors[scrIdx], 2.3f);
        }

        /// <summary>
        /// Crea un Rack de Servidores de alta densidad (Lab_ServerRack_A/B) con su matriz de LEDs emisivos (Lab_RackLeds_A/B),
        /// consola industrial lateral, reflejo en el piso y luz puntual (más intensa cuando está en una Bóveda Oscura).
        /// </summary>
        private static void CreateServerRackUnit(Transform parent, string name, Vector3 floorSurfacePos, Sprite glowSprite, int variant, bool isDarkZone)
        {
            GameObject rack = new GameObject(name);
            rack.transform.SetParent(parent, false);
            rack.transform.position = floorSurfacePos;

            bool useVariantB = (Mathf.Abs(variant) % 2 == 1);
            Sprite bodySprite = GetLabGeneratedSprite(useVariantB ? "Lab_ServerRack_B" : "Lab_ServerRack_A", 104f);
            Sprite ledsSprite = GetLabGeneratedSprite(useVariantB ? "Lab_RackLeds_B" : "Lab_RackLeds_A", 104f);
            Sprite fogSprite = GetLabGeneratedSprite("Lab_Fog", 100f);

            Color[] ledPalette = new Color[]
            {
                new Color(0.15f, 0.95f, 1.0f, 0.96f),
                new Color(0.20f, 1.0f, 0.68f, 0.96f),
                new Color(1.0f, 0.25f, 0.45f, 0.96f),
                new Color(1.0f, 0.72f, 0.18f, 0.96f)
            };
            Color ledCol = ledPalette[Mathf.Abs(variant) % ledPalette.Length];

            // 1. Gabinete del Servidor (2.5m de altura)
            GameObject bodyObj = new GameObject("ServerRack_Cabinet");
            bodyObj.transform.SetParent(rack.transform, false);
            bodyObj.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            SpriteRenderer srBody = bodyObj.AddComponent<SpriteRenderer>();
            if (bodySprite != null)
            {
                srBody.sprite = bodySprite;
                srBody.color = Color.white;
            }
            else
            {
                srBody.sprite = GetIndustrialTile("platformIndustrial_071", 140f) ?? GetOrCreateSquareSprite();
            }
            srBody.sortingOrder = -3;

            // 2. Matriz de LEDs emisivos encendidos sobre el cristal del rack
            if (ledsSprite != null)
            {
                GameObject ledsObj = new GameObject("ServerRack_EmissiveLEDs");
                ledsObj.transform.SetParent(rack.transform, false);
                ledsObj.transform.localPosition = new Vector3(0f, 1.25f, 0f);
                SpriteRenderer srLeds = ledsObj.AddComponent<SpriteRenderer>();
                srLeds.sprite = ledsSprite;
                srLeds.color = ledCol;
                srLeds.sortingOrder = -2;
            }

            // 3. Consola industrial / Celda de energía UPS adosada junto al servidor (usando los sprites del proyecto)
            string[] sideTiles = new string[] { "platformIndustrial_073", "platformIndustrial_074", "platformIndustrial_075", "platformIndustrial_090", "platformIndustrial_104", "platformIndustrial_106" };
            Sprite sideTile = GetIndustrialTile(sideTiles[Mathf.Abs(variant) % sideTiles.Length], 120f);
            if (sideTile != null)
            {
                GameObject sideConsole = new GameObject("Server_Diagnostic_Unit");
                sideConsole.transform.SetParent(rack.transform, false);
                sideConsole.transform.localPosition = new Vector3(0.95f, 0.95f, 0f);
                SpriteRenderer srSide = sideConsole.AddComponent<SpriteRenderer>();
                srSide.sprite = sideTile;
                srSide.sortingOrder = -3;
            }

            // 4. Reflejo luminoso sobre el piso frente al servidor
            if (fogSprite != null)
            {
                GameObject floorGlow = new GameObject("Server_FloorReflection");
                floorGlow.transform.SetParent(rack.transform, false);
                floorGlow.transform.localPosition = new Vector3(0f, 0.10f, 0f);
                floorGlow.transform.localScale = new Vector3(0.62f, 0.28f, 1f);
                SpriteRenderer srFloor = floorGlow.AddComponent<SpriteRenderer>();
                srFloor.sprite = fogSprite;
                srFloor.color = new Color(ledCol.r, ledCol.g, ledCol.b, isDarkZone ? 0.22f : 0.12f);
                srFloor.sortingOrder = -2;
            }

            // 5. Luz puntual del servidor (más marcada en Zonas Oscuras para apreciar la iluminación en la penumbra)
            float glowAlpha = isDarkZone ? 0.18f : 0.13f;
            float glowDiameter = isDarkZone ? 2.85f : 2.30f;
            CreatePointLightGlow(parent, name + "_ServerLight", floorSurfacePos + new Vector3(0f, 1.25f, 0f),
                glowSprite, new Color(ledCol.r, ledCol.g, ledCol.b, glowAlpha), glowDiameter);
        }

        private static void CreateLabStaticProp(Transform parent, string name, string generatedSpriteName, Vector3 pos, float desiredMaxMeters, int sortingOrder)
        {
            Sprite sp = GetLabGeneratedSprite(generatedSpriteName, 100f);
            if (sp == null) return;

            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.position = pos;
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.color = Color.white;
            sr.sortingOrder = sortingOrder;
            NormalizePropWorldScale(obj.transform, sp, desiredMaxMeters);
        }

        private static void CreateLabSignWithGlow(Transform parent, string name, string signSpriteName, Vector3 pos, Sprite glowSprite, Color glowColor)
        {
            CreateLabStaticProp(parent, name, signSpriteName, pos, 3.4f, -2);
            CreatePointLightGlow(parent, name + "_NeonGlow", pos, glowSprite, glowColor, 3.0f);
        }

        private static void CreateExtractorVent(Transform parent, string name, Vector3 pos)
        {
            Sprite frameSp = GetLabGeneratedSprite("Lab_VentFrame", 100f);
            Sprite fanSp = GetLabGeneratedSprite("Lab_VentFan", 100f);
            if (frameSp == null) return;

            GameObject vent = new GameObject(name);
            vent.transform.SetParent(parent, false);
            vent.transform.position = pos;
            SpriteRenderer srFrame = vent.AddComponent<SpriteRenderer>();
            srFrame.sprite = frameSp;
            srFrame.sortingOrder = -7;
            NormalizePropWorldScale(vent.transform, frameSp, 1.35f);

            if (fanSp != null)
            {
                GameObject fan = new GameObject("Vent_Fan_Blades");
                fan.transform.SetParent(vent.transform, false);
                SpriteRenderer srFan = fan.AddComponent<SpriteRenderer>();
                srFan.sprite = fanSp;
                srFan.sortingOrder = -6;
            }
        }

        /// <summary>
        /// Distribuye mobiliario realista de Laboratorio de Cómputo a lo largo de cada módulo de 230m:
        /// Pizarrones de Redes, Posters académicos, Impresoras en red, Dispensadores de agua, Gabinetes,
        /// Cajas de hardware, Plantas, Puertas dobles y Señalética direccional.
        /// </summary>
        private static void CreateLabClassroomDecorSet(Transform parent, float bX, int m, Sprite arrowRightTile, Sprite arrowUpTile)
        {
            // Aula Oeste (X = bX + 2 a bX + 36)
            CreateLabStaticProp(parent, $"Whiteboard_West_S{m}", "Lab_Whiteboard", new Vector3(bX + 13.5f, 0.95f, 0f), 2.75f, -3);
            CreateLabStaticProp(parent, $"PosterA_West_S{m}", "Lab_Poster_A", new Vector3(bX + 3.8f, 1.15f, 0f), 1.05f, -3);
            CreateLabStaticProp(parent, $"PosterB_West_S{m}", "Lab_Poster_B", new Vector3(bX + 31.5f, 1.15f, 0f), 1.05f, -3);
            CreateLabStaticProp(parent, $"Cooler_West_S{m}", "Lab_Cooler", new Vector3(bX + 16.8f, -0.95f, 0f), 1.55f, -3);
            CreateLabStaticProp(parent, $"Printer_West_S{m}", "Lab_Printer", new Vector3(bX + 28.5f, -1.25f, 0f), 1.25f, -3);
            CreateLabStaticProp(parent, $"Plant_West_S{m}", "Lab_Plant", new Vector3(bX + 35.2f, -1.00f, 0f), 1.45f, -3);

            // Atrio Central (X = bX + 96 a bX + 112)
            CreateLabStaticProp(parent, $"Door_Mid_S{m}", "Lab_Door", new Vector3(bX + 97.5f, -0.40f, 0f), 2.70f, -3);
            CreateLabStaticProp(parent, $"WallDisplay_Mid_S{m}", "Lab_WallDisplay_Net", new Vector3(bX + 108.5f, 1.55f, 0f), 3.15f, -3);
            CreateLabStaticProp(parent, $"Boxes_Mid_S{m}", "Lab_Boxes", new Vector3(bX + 100.2f, -1.20f, 0f), 1.35f, -3);
            CreateLabStaticProp(parent, $"Cabinet_Mid_S{m}", "Lab_Cabinet", new Vector3(bX + 111.0f, -0.75f, 0f), 1.95f, -3);

            // Aula Este (X = bX + 178 a bX + 225)
            CreateLabStaticProp(parent, $"Whiteboard_East_S{m}", "Lab_Whiteboard", new Vector3(bX + 190.0f, 0.95f, 0f), 2.75f, -3);
            CreateLabStaticProp(parent, $"PosterC_East_S{m}", "Lab_Poster_C", new Vector3(bX + 178.5f, 1.15f, 0f), 1.05f, -3);
            CreateLabStaticProp(parent, $"Cabinet_East_S{m}", "Lab_Cabinet", new Vector3(bX + 198.5f, -0.75f, 0f), 1.95f, -3);
            CreateLabStaticProp(parent, $"Printer_East_S{m}", "Lab_Printer", new Vector3(bX + 212.5f, -1.25f, 0f), 1.25f, -3);
            CreateLabStaticProp(parent, $"Cooler_East_S{m}", "Lab_Cooler", new Vector3(bX + 216.0f, -0.95f, 0f), 1.55f, -3);
            CreateLabStaticProp(parent, $"Plant_East_S{m}", "Lab_Plant", new Vector3(bX + 221.5f, -1.00f, 0f), 1.45f, -3);

            // Flechas direccionales de neón en puntos clave de ascenso
            if (arrowUpTile != null)
            {
                GameObject upArrow = new GameObject($"Sign_ArrowUp_S{m}");
                upArrow.transform.SetParent(parent, false);
                upArrow.transform.position = new Vector3(bX + 111.2f, 0.45f, 0f);
                SpriteRenderer srUp = upArrow.AddComponent<SpriteRenderer>();
                srUp.sprite = arrowUpTile;
                srUp.sortingOrder = -2;
                NormalizePropWorldScale(upArrow.transform, arrowUpTile, 0.72f);
            }
            if (arrowRightTile != null)
            {
                GameObject rightArrow = new GameObject($"Sign_ArrowRight_S{m}");
                rightArrow.transform.SetParent(parent, false);
                rightArrow.transform.position = new Vector3(bX + 177.0f, 0.45f, 0f);
                SpriteRenderer srRight = rightArrow.AddComponent<SpriteRenderer>();
                srRight.sprite = arrowRightTile;
                srRight.sortingOrder = -2;
                NormalizePropWorldScale(rightArrow.transform, arrowRightTile, 0.72f);
            }
        }

        /// <summary>
        /// Crea un halo de iluminación 2D sutil, localizado y detrás del jugador (sortingOrder = -4),
        /// convirtiendo diameterMeters a la escala exacta del sprite (base 4m) para nunca saturar la pantalla.
        /// </summary>
        private static void CreatePointLightGlow(Transform parent, string name, Vector3 pos, Sprite glowSprite, Color lightColor, float diameterMeters)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.SetParent(parent, false);
            lightObj.transform.position = pos;

            // SoftLightGlow.png mide 256px a 64 PPU = 4.0 metros en escala 1.0
            float normalizedScale = Mathf.Clamp(diameterMeters / 4.0f, 0.25f, 1.35f);
            lightObj.transform.localScale = new Vector3(normalizedScale, normalizedScale, 1f);

            SpriteRenderer srOuter = lightObj.AddComponent<SpriteRenderer>();
            srOuter.sprite = glowSprite != null ? glowSprite : GetOrCreateCircleSprite();
            srOuter.color = new Color(lightColor.r, lightColor.g, lightColor.b, Mathf.Clamp(lightColor.a, 0.05f, 0.22f));
            srOuter.sortingOrder = -4;

            TryAttachUrpLight2D(lightObj, isGlobal: false, intensity: 0.52f, color: new Color(lightColor.r, lightColor.g, lightColor.b, 1f), outerRadius: diameterMeters * 0.58f);
        }

        private static void CreateGlobalLight2D(float intensity, Color ambientColor)
        {
            GameObject globalLight = GameObject.Find("Global Light 2D");
            if (globalLight == null) globalLight = new GameObject("Global Light 2D");
            TryAttachUrpLight2D(globalLight, isGlobal: true, intensity: intensity, color: ambientColor, outerRadius: 20f);
        }

        private static void TryAttachUrpLight2D(GameObject targetObj, bool isGlobal, float intensity, Color color, float outerRadius)
        {
            Type light2DType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (light2DType == null) return;

            Component comp = targetObj.GetComponent(light2DType);
            if (comp == null) comp = targetObj.AddComponent(light2DType);
            if (comp == null) return;

            SerializedObject so = new SerializedObject(comp);
            SerializedProperty lightTypeProp = so.FindProperty("m_LightType");
            if (lightTypeProp != null)
            {
                lightTypeProp.intValue = isGlobal ? 4 : 3;
            }
            SerializedProperty intensityProp = so.FindProperty("m_Intensity");
            if (intensityProp != null) intensityProp.floatValue = intensity;

            SerializedProperty colorProp = so.FindProperty("m_Color");
            if (colorProp != null) colorProp.colorValue = color;

            if (!isGlobal)
            {
                SerializedProperty radiusProp = so.FindProperty("m_PointLightOuterRadius");
                if (radiusProp != null) radiusProp.floatValue = outerRadius;
            }
            so.ApplyModifiedProperties();
        }

        private static void CreateBossArenaContainmentGate(Transform parent, float gateX, float triggerX, Sprite beamSprite)
        {
            GameObject gateStructure = new GameObject("Boss_Arena_Gate_Structure");
            gateStructure.transform.SetParent(parent, false);

            Sprite pillarSprite = GetLabGeneratedSprite("Lab_Pillar", 64f) ?? GetIndustrialTile("platformIndustrial_029", 140f);
            Sprite hazardSprite = GetLabGeneratedSprite("Lab_PlatformHazard", 100f) ?? beamSprite ?? GetIndustrialTile("platformIndustrial_064", 140f);
            Sprite doorSprite = GetLabGeneratedSprite("Lab_Door", 100f) ?? hazardSprite;
            Sprite sirenSprite = GetIndustrialTile("platformIndustrial_054", 140f);
            Sprite glowSprite = LoadSoftGlowSprite();

            // 1. Pilares laterales de contención pesada
            CreateTiledPlatform(gateStructure.transform, "Gate_Pillar_Left", pillarSprite, new Vector3(gateX - 1.25f, 6.25f, 0f), new Vector2(0.9f, 16.0f), false, 4);
            CreateTiledPlatform(gateStructure.transform, "Gate_Pillar_Right", pillarSprite, new Vector3(gateX + 1.25f, 6.25f, 0f), new Vector2(0.9f, 16.0f), false, 4);

            // 2. Dintel superior reforzado con franjas de advertencia industrial
            CreateTiledPlatform(gateStructure.transform, "Gate_Ceiling_Lintel", hazardSprite, new Vector3(gateX, 15.0f, 0f), new Vector2(3.6f, 1.8f), false, 5);

            // 3. Baliza de alarma / sirena roja superior con halo de alerta
            GameObject sirenObj = new GameObject("Gate_Warning_Siren");
            sirenObj.transform.SetParent(gateStructure.transform, false);
            sirenObj.transform.position = new Vector3(gateX, 16.15f, 0f);
            SpriteRenderer srSiren = sirenObj.AddComponent<SpriteRenderer>();
            srSiren.sprite = sirenSprite != null ? sirenSprite : GetOrCreateCircleSprite();
            srSiren.color = new Color(1.0f, 0.25f, 0.35f, 1f);
            srSiren.sortingOrder = 6;
            CreatePointLightGlow(sirenObj.transform, "Gate_Siren_Glow", sirenObj.transform.position,
                glowSprite, new Color(1.0f, 0.15f, 0.25f, 0.30f), 3.2f);

            // 4. Compuerta deslizante pesada (inicialmente retraída en el techo a Y = 18.0f)
            GameObject gate = new GameObject("Boss_Arena_Gate");
            gate.transform.SetParent(parent, false);
            gate.transform.position = new Vector3(gateX, 18.0f, 0f);

            SpriteRenderer sr = gate.AddComponent<SpriteRenderer>();
            sr.sprite = doorSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(2.0f, 16.0f);
            sr.color = new Color(0.95f, 0.95f, 1.0f, 0.98f);
            sr.sortingOrder = 3;

            // Placas de blindaje secundario en la compuerta para mayor detalle
            GameObject subPlate = new GameObject("Gate_Heavy_SubPlate");
            subPlate.transform.SetParent(gate.transform, false);
            subPlate.transform.localPosition = Vector3.zero;
            SpriteRenderer srSub = subPlate.AddComponent<SpriteRenderer>();
            srSub.sprite = hazardSprite;
            srSub.drawMode = SpriteDrawMode.Tiled;
            srSub.size = new Vector2(1.7f, 15.6f);
            srSub.color = new Color(1f, 0.9f, 0.9f, 0.75f);
            srSub.sortingOrder = 4;

            BoxCollider2D gateCol = gate.AddComponent<BoxCollider2D>();
            gateCol.size = new Vector2(2.0f, 16.0f);
            gateCol.enabled = false;

            // 5. Sensor de proximidad BossArenaTrigger para sellar la compuerta hidráulica
            GameObject triggerObj = new GameObject("BossArenaTrigger");
            triggerObj.transform.SetParent(parent, false);
            triggerObj.transform.position = new Vector3(triggerX, 4.0f, 0f);
            BoxCollider2D trigCol = triggerObj.AddComponent<BoxCollider2D>();
            trigCol.isTrigger = true;
            trigCol.size = new Vector2(2.5f, 14.0f);
        }

        private static void SetupThemedEnemiesAndFloatingBoss(LevelTheme theme)
        {
            // Dificultad: la vida de cada enemigo crece exactamente +20% por nivel (x1.0, x1.2, x1.44, x1.728);
            // además aumentan gradualmente la cantidad de patrulleros, su cadencia de disparo y el daño de proyectil.
            float d = theme.difficulty;
            float gentle = Mathf.Sqrt(d);

            float[] patrolX;
            if (theme.levelNumber == 1)
            {
                patrolX = new float[] { 85f, 225f, 365f, 510f, 655f, 805f, 955f, 1095f };
            }
            else
            {
                // Posiciones seguras a nivel de suelo (lejos de muros, torres y escaleras) repartidas por los 5 módulos
                float[] safeOffsets = { 35f, 85f, 135f, 175f, 225f };
                List<float> candidates = new List<float>();
                for (int m = 0; m < 5; m++)
                {
                    for (int i = 0; i < safeOffsets.Length; i++)
                    {
                        float x = m * 230f + safeOffsets[i];
                        if (x >= 80f && x <= 1105f) candidates.Add(x);
                    }
                }
                int count = Mathf.Min(candidates.Count, Mathf.RoundToInt(8f * gentle));
                patrolX = new float[count];
                for (int i = 0; i < count; i++)
                {
                    int idx = Mathf.Clamp(Mathf.RoundToInt(i * (candidates.Count - 1) / Mathf.Max(1f, count - 1f)), 0, candidates.Count - 1);
                    patrolX[i] = candidates[idx];
                }
            }

            for (int i = 0; i < patrolX.Length; i++)
            {
                CreateWraithEnemy(
                    objName: $"Teacher_Patrol_{i + 1}",
                    pos: new Vector3(patrolX[i], -1.75f, 0f),
                    isFinalBoss: false,
                    isFloatingBoss: false,
                    braceletHp: Mathf.RoundToInt(90f * d),
                    hoverY: -1.75f,
                    arenaRadius: 7.0f,
                    projectileDmg: 8,
                    attackInterval: 2.8f / gentle
                );
            }

            CreateWraithEnemy(
                objName: "Teacher_Boss",
                pos: new Vector3(1200.0f, 2.45f, 0f),
                isFinalBoss: true,
                isFloatingBoss: true,
                braceletHp: Mathf.RoundToInt(100f * d),
                hoverY: 2.45f,
                arenaRadius: 14.0f,
                projectileDmg: Mathf.RoundToInt(10f * gentle),
                attackInterval: 2.3f / gentle
            );
        }

        private static Sprite LoadHumanProfessorSprite(bool isRun)
        {
            string path = isRun
                ? "Assets/_Project/Sprites/Characters/Processed/Professor_Human_Run.png"
                : "Assets/_Project/Sprites/Characters/Processed/Professor_Human_Idle.png";
            return BatchSpriteSheetProcessor.ConfigureSingleSprite(path, 28f, new Vector2(0.5f, 0.0f), FilterMode.Point);
        }

        private static Sprite[] LoadHumanProfessorIdleFrames()
        {
            Sprite[] frames = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                frames[i] = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                    $"Assets/_Project/Sprites/Characters/Processed/Professor_Human_Idle_{i}.png", 28f, new Vector2(0.5f, 0.0f), FilterMode.Point);
            }
            return frames;
        }

        private static Sprite[] LoadHumanProfessorRunFrames()
        {
            Sprite[] frames = new Sprite[6];
            for (int i = 0; i < 6; i++)
            {
                frames[i] = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                    $"Assets/_Project/Sprites/Characters/Processed/Professor_Human_Run_{i}.png", 28f, new Vector2(0.5f, 0.0f), FilterMode.Point);
            }
            return frames;
        }

        private static void CreateWraithEnemy(
            string objName,
            Vector3 pos,
            bool isFinalBoss,
            bool isFloatingBoss,
            int braceletHp,
            float hoverY,
            float arenaRadius,
            int projectileDmg,
            float attackInterval)
        {
            GameObject teacher = new GameObject(objName);
            teacher.transform.position = pos;
            teacher.transform.localScale = Vector3.one;

            SpriteRenderer sr = teacher.AddComponent<SpriteRenderer>();
            string idleSpritePath = isFinalBoss
                ? "Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_03/PNG Sequences/Idle/Wraith_03_Idle_000.png"
                : "Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_01/PNG Sequences/Idle/Wraith_01_Idle_000.png";
            Sprite wraithSprite = AssetDatabase.LoadAssetAtPath<Sprite>(idleSpritePath);
            if (wraithSprite != null)
            {
                sr.sprite = wraithSprite;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = GetOrCreateSquareSprite();
                sr.color = new Color(0.85f, 0.18f, 0.22f);
            }
            sr.sortingOrder = 5;

            Animator anim = teacher.AddComponent<Animator>();
            string ctrlPath = isFinalBoss
                ? "Assets/_Project/Animations/Enemies/BossWraithAnimator.controller"
                : "Assets/_Project/Animations/Enemies/WraithPatrolAnimator.controller";
            RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);
            if (ctrl != null) anim.runtimeAnimatorController = ctrl;

            GameObject braceletVisual = new GameObject("Bracelet_SIA_Glow");
            braceletVisual.transform.SetParent(teacher.transform, false);
            braceletVisual.transform.localPosition = isFinalBoss ? new Vector3(1.1f, 3.0f, 0f) : new Vector3(0.45f, 1.1f, 0f);
            braceletVisual.transform.localScale = isFinalBoss ? new Vector3(0.55f, 0.55f, 1f) : new Vector3(0.28f, 0.28f, 1f);
            SpriteRenderer srBracelet = braceletVisual.AddComponent<SpriteRenderer>();
            srBracelet.sprite = GetOrCreateCircleSprite();
            srBracelet.color = new Color(1.0f, 0.15f, 0.9f, 0.9f);
            srBracelet.sortingOrder = 6;

            Rigidbody2D rb = teacher.AddComponent<Rigidbody2D>();
            rb.gravityScale = isFloatingBoss ? 0f : 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            BoxCollider2D col = teacher.AddComponent<BoxCollider2D>();
            col.size = isFinalBoss ? new Vector2(2.3f, 5.2f) : new Vector2(0.95f, 2.1f);
            col.offset = isFinalBoss ? new Vector2(0f, 2.8f) : new Vector2(0f, 1.05f);
            col.sharedMaterial = GetOrCreateFrictionlessMaterial();

            BraceletHealth bracelet = teacher.AddComponent<BraceletHealth>();
            bracelet.ConfigureHealth(braceletHp);

            TeacherAI ai = teacher.AddComponent<TeacherAI>();
            Sprite projSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_03/Vector Parts/Spells-Effect.png");
            Sprite humanIdle = LoadHumanProfessorSprite(isRun: false);
            Sprite humanRun = LoadHumanProfessorSprite(isRun: true);
            Sprite[] idleFrames = LoadHumanProfessorIdleFrames();
            Sprite[] runFrames = LoadHumanProfessorRunFrames();

            SerializedObject soAi = new SerializedObject(ai);
            soAi.FindProperty("detectionRange").floatValue = isFinalBoss ? 26.0f : 9.0f;
            soAi.FindProperty("isFloatingBoss").boolValue = isFloatingBoss;
            soAi.FindProperty("floatingTargetY").floatValue = hoverY;
            soAi.FindProperty("arenaPatrolRadius").floatValue = arenaRadius;
            soAi.FindProperty("canRangedAttack").boolValue = isFinalBoss;
            soAi.FindProperty("rangedAttackInterval").floatValue = attackInterval;
            soAi.FindProperty("rangedDamage").intValue = projectileDmg;
            soAi.FindProperty("projectileSpeed").floatValue = 9.0f;
            soAi.FindProperty("contactDamage").intValue = isFinalBoss ? 14 : 12;
            if (projSprite != null) soAi.FindProperty("projectileSprite").objectReferenceValue = projSprite;
            if (humanIdle != null) soAi.FindProperty("humanIdleSprite").objectReferenceValue = humanIdle;
            if (humanRun != null) soAi.FindProperty("humanRunSprite").objectReferenceValue = humanRun;

            if (idleFrames != null)
            {
                SerializedProperty idleProp = soAi.FindProperty("humanIdleFrames");
                idleProp.arraySize = idleFrames.Length;
                for (int i = 0; i < idleFrames.Length; i++) idleProp.GetArrayElementAtIndex(i).objectReferenceValue = idleFrames[i];
            }
            if (runFrames != null)
            {
                SerializedProperty runProp = soAi.FindProperty("humanRunFrames");
                runProp.arraySize = runFrames.Length;
                for (int i = 0; i < runFrames.Length; i++) runProp.GetArrayElementAtIndex(i).objectReferenceValue = runFrames[i];
            }
            soAi.ApplyModifiedProperties();

            float barHeight = isFinalBoss ? 6.25f : 2.45f;
            float barWidth = isFinalBoss ? 2.8f : 1.5f;
            AttachWorldHealthBar(teacher.transform, barHeight, barWidth, 0.28f,
                isFinalBoss ? new Color(1.0f, 0.18f, 0.35f, 1f) : new Color(0.95f, 0.35f, 0.15f, 1f));
        }

        private static void AttachWorldHealthBar(Transform targetParent, float offsetY, float worldWidth, float worldHeight, Color initialFillColor)
        {
            Sprite bgSprite = LoadWorldBarBgSprite() ?? GetOrCreateSquareSprite();
            Sprite fillSprite = LoadKenneyBarFillSprite() ?? GetOrCreateSquareSprite();
            Sprite borderSprite = LoadWenrexaBarBorderSprite();

            GameObject healthBarObj = new GameObject("FloatingHealthBar");
            healthBarObj.transform.SetParent(targetParent, false);
            healthBarObj.transform.localPosition = new Vector3(0f, offsetY, 0f);

            GameObject barBg = new GameObject("Bar_BG");
            barBg.transform.SetParent(healthBarObj.transform, false);
            SpriteRenderer srBg = barBg.AddComponent<SpriteRenderer>();
            srBg.sprite = bgSprite;
            srBg.color = new Color(0.10f, 0.14f, 0.22f, 0.95f);
            srBg.sortingOrder = 10;
            Vector2 bgSize = bgSprite != null ? (Vector2)bgSprite.bounds.size : Vector2.one;
            barBg.transform.localScale = new Vector3(
                worldWidth / Mathf.Max(0.01f, bgSize.x),
                worldHeight / Mathf.Max(0.01f, bgSize.y), 1f);

            GameObject barFill = new GameObject("Bar_Fill");
            barFill.transform.SetParent(healthBarObj.transform, false);
            SpriteRenderer srFill = barFill.AddComponent<SpriteRenderer>();
            srFill.sprite = fillSprite;
            srFill.color = initialFillColor;
            srFill.sortingOrder = 11;
            Vector2 fillSize = fillSprite != null ? (Vector2)fillSprite.bounds.size : Vector2.one;
            barFill.transform.localScale = new Vector3(
                Mathf.Max(0.1f, worldWidth - 0.08f) / Mathf.Max(0.01f, fillSize.x),
                Mathf.Max(0.08f, worldHeight - 0.06f) / Mathf.Max(0.01f, fillSize.y), 1f);

            if (borderSprite != null)
            {
                GameObject barBorder = new GameObject("Bar_Border");
                barBorder.transform.SetParent(healthBarObj.transform, false);
                SpriteRenderer srBorder = barBorder.AddComponent<SpriteRenderer>();
                srBorder.sprite = borderSprite;
                srBorder.color = new Color(0.80f, 0.96f, 1.0f, 0.85f);
                srBorder.sortingOrder = 12;
                Vector2 bSize = borderSprite.bounds.size;
                barBorder.transform.localScale = new Vector3(
                    worldWidth / Mathf.Max(0.01f, bSize.x),
                    worldHeight / Mathf.Max(0.01f, bSize.y), 1f);
            }

            FloatingHealthBar floatingBar = healthBarObj.AddComponent<FloatingHealthBar>();
            SerializedObject soBar = new SerializedObject(floatingBar);
            soBar.FindProperty("fillTransform").objectReferenceValue = barFill.transform;
            soBar.FindProperty("offset").vector3Value = new Vector3(0f, offsetY, 0f);
            soBar.ApplyModifiedProperties();
        }

        private static Camera CreateSceneCamera(Color bgColor, Transform followTarget)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);

            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bgColor;
            camObj.AddComponent<AudioListener>();

            Type urpCamData = Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (urpCamData != null && camObj.GetComponent(urpCamData) == null)
            {
                camObj.AddComponent(urpCamData);
            }

            CameraFollow2D follow = camObj.AddComponent<CameraFollow2D>();
            if (followTarget != null) follow.SetTarget(followTarget);
            return cam;
        }

        private static void CreateCheckpointStation(Transform parent, string name, Sprite terminalSprite, Sprite beaconSprite, Vector3 surfaceFeetPos, Sprite glowSprite)
        {
            GameObject cp = new GameObject(name);
            cp.transform.SetParent(parent);
            // Offset exacto +1.05m para que PlayerHealth (cp.y - 1.05f) reaparezca al ras del suelo
            cp.transform.position = surfaceFeetPos + new Vector3(0f, 1.05f, 0f);

            BoxCollider2D trigger = cp.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            // Cubre toda la columna vertical del mapa (de -55m hasta +125m) para detectarlo aunque el jugador pase por plataformas superiores
            trigger.size = new Vector2(3.2f, 180.0f);
            trigger.offset = new Vector2(0f, 35.0f - cp.transform.position.y);

            // Assets reales de Kenney Platformer Pack Industrial + Wenrexa Sci-Fi Interface
            Sprite postIdleSprite = GetIndustrialTile("platformIndustrial_042", 140f) ?? beaconSprite;
            Sprite postActiveSprite = GetIndustrialTile("platformIndustrial_055", 140f) ?? postIdleSprite;
            Sprite holoRingSprite = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                $"{WenrexaUiFolder}/Icons/Icon01.png", 96f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);

            // 1. Consola base de terminal industrial
            GameObject consoleBase = new GameObject("Checkpoint_Terminal_Base");
            consoleBase.transform.SetParent(cp.transform, false);
            consoleBase.transform.localPosition = new Vector3(-0.42f, -0.46f, 0f);
            consoleBase.transform.localScale = new Vector3(0.76f, 0.76f, 1f);
            SpriteRenderer srBase = consoleBase.AddComponent<SpriteRenderer>();
            srBase.sprite = terminalSprite != null ? terminalSprite : GetOrCreateSquareSprite();
            srBase.color = new Color(0.55f, 0.90f, 1.0f, 1f);
            srBase.sortingOrder = -2;

            // 2. Baliza vertical en reposo (ámbar Kenney)
            GameObject postIdle = new GameObject("Checkpoint_Post_Idle");
            postIdle.transform.SetParent(cp.transform, false);
            postIdle.transform.localPosition = new Vector3(0.28f, -0.18f, 0f);
            postIdle.transform.localScale = new Vector3(1.18f, 1.18f, 1f);
            SpriteRenderer srIdle = postIdle.AddComponent<SpriteRenderer>();
            srIdle.sprite = postIdleSprite != null ? postIdleSprite : GetOrCreateCircleSprite();
            srIdle.color = Color.white;
            srIdle.sortingOrder = -1;

            // 3. Baliza vertical activada (verde esmeralda Kenney — se enciende al pasar)
            GameObject postActive = new GameObject("Checkpoint_Post_Active");
            postActive.transform.SetParent(cp.transform, false);
            postActive.transform.localPosition = new Vector3(0.28f, -0.18f, 0f);
            postActive.transform.localScale = new Vector3(1.18f, 1.18f, 1f);
            SpriteRenderer srActive = postActive.AddComponent<SpriteRenderer>();
            srActive.sprite = postActiveSprite != null ? postActiveSprite : GetOrCreateCircleSprite();
            srActive.color = Color.white;
            srActive.sortingOrder = -1;
            postActive.SetActive(false);

            // 4. Retícula holográfica Sci-Fi sutil sobre la baliza (Wenrexa Icon01)
            if (holoRingSprite != null)
            {
                GameObject holo = new GameObject("Checkpoint_Halo_Ring");
                holo.transform.SetParent(cp.transform, false);
                holo.transform.localPosition = new Vector3(0.28f, 0.78f, 0f);
                holo.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
                SpriteRenderer srHolo = holo.AddComponent<SpriteRenderer>();
                srHolo.sprite = holoRingSprite;
                srHolo.color = new Color(1.0f, 0.80f, 0.25f, 0.75f);
                srHolo.sortingOrder = 0;
            }

            CreatePointLightGlow(cp.transform, "Checkpoint_Ambient_Glow", surfaceFeetPos + new Vector3(0.15f, 1.45f, 0f),
                glowSprite, new Color(1.0f, 0.78f, 0.25f, 0.16f), 2.5f);
        }

        private static GameObject CreateTiledPlatform(Transform parent, string name, Sprite tileSprite, Vector3 pos, Vector2 size, bool addCollider, int sortingOrder)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.position = pos;
            obj.transform.localScale = Vector3.one;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            if (tileSprite != null)
            {
                sr.sprite = tileSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = size;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = GetOrCreateSquareSprite();
                obj.transform.localScale = new Vector3(size.x, size.y, 1f);
                sr.color = new Color(0.25f, 0.32f, 0.42f);
            }
            sr.sortingOrder = sortingOrder;

            if (addCollider)
            {
                BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
                col.size = tileSprite != null ? size : Vector2.one;
                col.usedByEffector = false;
            }

            return obj;
        }

        private static void CreateClimbableLadder(Transform parent, string name, Sprite ladderSprite, Vector3 pos, float height)
        {
            GameObject ladder = new GameObject(name);
            ladder.transform.SetParent(parent);
            ladder.transform.position = pos;
            ladder.transform.localScale = Vector3.one;

            SpriteRenderer sr = ladder.AddComponent<SpriteRenderer>();
            if (ladderSprite != null)
            {
                sr.sprite = ladderSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(1.0f, height);
            }
            else
            {
                sr.sprite = GetOrCreateSquareSprite();
                ladder.transform.localScale = new Vector3(0.9f, height, 1f);
                sr.color = new Color(0.95f, 0.45f, 0.1f);
            }
            sr.sortingOrder = -1;

            BoxCollider2D trigger = ladder.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = ladderSprite != null ? new Vector2(0.95f, height) : Vector2.one;
        }

        private static Sprite GetIndustrialTile(string fileNameWithoutExt, float ppu)
        {
            string path = $"{IndustrialRetinaFolder}/{fileNameWithoutExt}.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return null;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.5f)
            {
                importer.spritePixelsPerUnit = ppu;
                changed = true;
            }

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteMeshType != SpriteMeshType.FullRect)
            {
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadSoftGlowSprite()
        {
            return BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/VFX/SoftLightGlow.png", 64f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
        }

        private static Sprite ConfigureUiNineSliceSprite(string path, Vector4 border, float ppu = 100f)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return null;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }
            if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.5f)
            {
                importer.spritePixelsPerUnit = ppu;
                changed = true;
            }
            if (importer.spriteBorder != border)
            {
                importer.spriteBorder = border;
                changed = true;
            }
            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteMeshType != SpriteMeshType.FullRect)
            {
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadGlassCardSprite()
        {
            // Asset real Kenney UI Pack Sci-Fi 2.0 (64x64 panel octagonal con biseles)
            Sprite kenneyPanel = ConfigureUiNineSliceSprite(
                $"{KenneyUiFolder}/Extra/Default/panel_glass_notches.png",
                new Vector4(16f, 16f, 16f, 16f), 100f);
            if (kenneyPanel != null) return kenneyPanel;

            return ConfigureUiNineSliceSprite("Assets/_Project/Sprites/UI/GlassCard_Rounded.png", new Vector4(28f, 28f, 28f, 28f), 100f);
        }

        private static Sprite LoadKenneyButtonSprite()
        {
            // Asset real Kenney UI Pack Sci-Fi 2.0 (192x64 botón metálico biselado con remaches)
            return ConfigureUiNineSliceSprite(
                $"{KenneyUiFolder}/Extra/Default/button_rectangle_depth.png",
                new Vector4(20f, 16f, 20f, 16f), 100f);
        }

        private static Sprite LoadKenneyBarBgSprite()
        {
            return ConfigureUiNineSliceSprite(
                $"{KenneyUiFolder}/Extra/Default/bar_shadow_square_outline_large.png",
                new Vector4(8f, 6f, 8f, 6f), 96f);
        }

        private static Sprite LoadWorldBarBgSprite()
        {
            return ConfigureUiNineSliceSprite(
                $"{KenneyUiFolder}/Extra/Default/bar_shadow_square_large.png",
                Vector4.zero, 96f);
        }

        private static Sprite LoadKenneyBarFillSprite()
        {
            return ConfigureUiNineSliceSprite(
                $"{KenneyUiFolder}/Grey/Default/bar_square_gloss_large.png",
                Vector4.zero, 96f);
        }

        private static Sprite LoadWenrexaBarBorderSprite()
        {
            return ConfigureUiNineSliceSprite(
                $"{WenrexaUiFolder}/ProgressBar_03/BarV3_ProgressBarBorder.png",
                Vector4.zero, 96f);
        }

        private static GameObject SetupPlayer(GameObject pulsePrefab, PhysicsMaterial2D playerMat, Vector3 startPos)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = new GameObject("Player");
                player.tag = "Player";
            }

            // Eliminar cualquier luz personal antigua sobre el jugador que saturaba el centro de la pantalla
            Transform oldPersonalLight = player.transform.Find("Player_Personal_Light");
            if (oldPersonalLight != null) UnityEngine.Object.DestroyImmediate(oldPersonalLight.gameObject);

            player.transform.position = startPos;
            player.transform.localScale = Vector3.one;

            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr == null) sr = player.AddComponent<SpriteRenderer>();

            Sprite ninjaIdle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Player/Ninja_Female/png/Idle__000.png");
            if (ninjaIdle != null)
            {
                sr.sprite = ninjaIdle;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = GetOrCreateSquareSprite();
                sr.color = new Color(0.18f, 0.7f, 1.0f);
            }
            sr.sortingOrder = 5;

            Animator anim = player.GetComponent<Animator>();
            if (anim == null) anim = player.AddComponent<Animator>();
            RuntimeAnimatorController animController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/Player/PlayerAnimatorController.controller");
            if (animController != null) anim.runtimeAnimatorController = animController;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb == null) rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = PlayerMovement.BaseGravity;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            CapsuleCollider2D col = player.GetComponent<CapsuleCollider2D>();
            if (col == null) col = player.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.95f, 2.0f);
            col.offset = new Vector2(0f, 1.0f);
            if (playerMat != null) col.sharedMaterial = playerMat;

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement == null) movement = player.AddComponent<PlayerMovement>();

            PlayerDash dash = player.GetComponent<PlayerDash>();
            if (dash == null) dash = player.AddComponent<PlayerDash>();

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health == null) health = player.AddComponent<PlayerHealth>();

            PlayerShooting shooting = player.GetComponent<PlayerShooting>();
            if (shooting == null) shooting = player.AddComponent<PlayerShooting>();

            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller == null) controller = player.AddComponent<PlayerController>();

            SerializedObject soCtrl = new SerializedObject(controller);
            AssignPlayableCharactersToSerializedObject(soCtrl);
            soCtrl.ApplyModifiedProperties();

            Transform firePoint = player.transform.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject fpObj = new GameObject("FirePoint");
                fpObj.transform.SetParent(player.transform);
                firePoint = fpObj.transform;
            }
            firePoint.localPosition = new Vector3(0.85f, 1.05f, 0f);

            Transform groundCheck = player.transform.Find("GroundCheck");
            if (groundCheck == null)
            {
                GameObject gcObj = new GameObject("GroundCheck");
                gcObj.transform.SetParent(player.transform);
                groundCheck = gcObj.transform;
            }
            groundCheck.localPosition = new Vector3(0f, 0.04f, 0f);

            SerializedObject soMovement = new SerializedObject(movement);
            soMovement.FindProperty("groundCheckPoint").objectReferenceValue = groundCheck;
            soMovement.FindProperty("groundLayer").intValue = ~LayerMask.GetMask("Ignore Raycast");
            soMovement.FindProperty("jumpForce").floatValue = 15.8f;
            soMovement.ApplyModifiedProperties();

            GameObject impactVfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/VFX/EMPPulseImpactPrefab.prefab");
            SerializedObject soShooting = new SerializedObject(shooting);
            soShooting.FindProperty("empPulsePrefab").objectReferenceValue = pulsePrefab;
            soShooting.FindProperty("firePoint").objectReferenceValue = firePoint;
            soShooting.FindProperty("currentDamage").intValue = 15;
            soShooting.FindProperty("baseDamage").intValue = 15;
            soShooting.FindProperty("chargedDamage").intValue = 35;
            soShooting.FindProperty("meleeDamage").intValue = 30;
            if (impactVfx != null) soShooting.FindProperty("meleeHitVfxPrefab").objectReferenceValue = impactVfx;
            soShooting.ApplyModifiedProperties();

            return player;
        }

        private static void SetupSpawner(GameObject smallEnemyPrefab, float bossZoneStartX, float difficulty = 1.0f)
        {
            GameObject spawnerObj = GameObject.Find("--- Enemy_Spawner ---");
            if (spawnerObj == null) spawnerObj = new GameObject("--- Enemy_Spawner ---");

            EnemySpawner spawner = spawnerObj.GetComponent<EnemySpawner>();
            if (spawner == null) spawner = spawnerObj.AddComponent<EnemySpawner>();

            float gentle = Mathf.Sqrt(Mathf.Max(1f, difficulty));
            SerializedObject so = new SerializedObject(spawner);
            so.FindProperty("smallEnemyPrefab").objectReferenceValue = smallEnemyPrefab;
            so.FindProperty("minSpawnInterval").floatValue = 6.5f / gentle;
            so.FindProperty("maxSpawnInterval").floatValue = 9.5f / gentle;
            so.FindProperty("maxActiveEnemies").intValue = Mathf.CeilToInt(5f * gentle);
            so.FindProperty("difficultyMultiplier").floatValue = difficulty;
            so.FindProperty("ceilingDropChance").floatValue = 0f;
            so.FindProperty("bossZoneStartX").floatValue = bossZoneStartX;
            so.ApplyModifiedProperties();
        }

        private static void SetupHUD(bool isMainMenu, bool isTutorial, string nextScene, GameObject smallEnemyPrefab, bool isIntroCutscene, bool isEndingCutscene = false)
        {
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas != null) UnityEngine.Object.DestroyImmediate(hudCanvas);

            hudCanvas = new GameObject("HUD_Canvas");
            Canvas canvas = hudCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = hudCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            hudCanvas.AddComponent<GraphicRaycaster>();

            Sprite squareSprite = GetOrCreateSquareSprite();
            Sprite glassCard = LoadGlassCardSprite();
            Sprite uiButtonSprite = LoadKenneyButtonSprite() ?? glassCard;
            Sprite barBgSprite = LoadKenneyBarBgSprite() ?? squareSprite;
            Sprite barFillSprite = LoadKenneyBarFillSprite() ?? squareSprite;
            Sprite barBorderSprite = LoadWenrexaBarBorderSprite();
            Sprite progressIconSprite = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                $"{WenrexaUiFolder}/Icons/Icon01.png", 96f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            Sprite crosshairIconSprite = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                $"{KenneyUiFolder}/Extra/Default/crosshair_a.png", 64f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            Sprite checkpointIconSprite = GetIndustrialTile("platformIndustrial_055", 140f);

            Font hdUiFont = LoadHighDefinitionUIFont();

            // 1. ESQUINA SUPERIOR DERECHA: TARJETA SCI-FI DE VIDA + VIDAS + CHECKPOINT (0/8)
            GameObject healthPanel = CreateHudGlassContainer(hudCanvas.transform, "Player_Health_Panel", glassCard,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -16f), new Vector2(405f, 96f));

            Image imgFillPlayer = CreateSciFiBarRow(healthPanel.transform, "Health_Bar", barBgSprite, barFillSprite, barBorderSprite,
                new Vector2(0.04f, 0.46f), new Vector2(0.96f, 0.90f), new Color(0.15f, 0.95f, 0.52f, 1f), 1.0f);

            Text txtPlayerHealth = CreateUIText(healthPanel.transform, "Player_Health_Text", "VIDA: 100 / 100", hdUiFont, 16,
                new Vector2(0.06f, 0.46f), new Vector2(0.94f, 0.90f));

            Text txtPlayerLives = CreateUIText(healthPanel.transform, "Player_Lives_Text", "VIDAS: 3/3  ♥ ♥ ♥", hdUiFont, 15,
                new Vector2(0.04f, 0.06f), new Vector2(0.47f, 0.40f));
            txtPlayerLives.color = new Color(0.35f, 1.0f, 0.85f, 1f);

            if (checkpointIconSprite != null)
            {
                CreateHudIconImage(healthPanel.transform, "Checkpoint_Badge_Icon", checkpointIconSprite,
                    new Vector2(0.49f, 0.08f), new Vector2(0.56f, 0.39f), Color.white);
            }

            Text txtCheckpoint = CreateUIText(healthPanel.transform, "Checkpoint_Status_Text", "CHECKPOINT: 0/8", hdUiFont, 14,
                new Vector2(0.56f, 0.06f), new Vector2(0.96f, 0.40f));
            txtCheckpoint.color = new Color(0.68f, 0.92f, 1.0f, 0.98f);

            // 2. ESQUINA INFERIOR IZQUIERDA: TARJETA SCI-FI DE DISPARO CARGADO + DASH
            GameObject abilitiesPanel = CreateHudGlassContainer(hudCanvas.transform, "Player_Abilities_Panel", glassCard,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 20f), new Vector2(420f, 96f));

            if (crosshairIconSprite != null)
            {
                CreateHudIconImage(abilitiesPanel.transform, "Weapon_Crosshair_Icon", crosshairIconSprite,
                    new Vector2(0.03f, 0.54f), new Vector2(0.11f, 0.88f), new Color(0.35f, 0.96f, 1.0f, 0.95f));
            }

            Image imgFillCharge = CreateSciFiBarRow(abilitiesPanel.transform, "Charge_Bar", barBgSprite, barFillSprite, barBorderSprite,
                new Vector2(0.12f, 0.52f), new Vector2(0.96f, 0.90f), new Color(0.12f, 0.85f, 1.0f, 1f), 0.0f);

            Text txtWeaponDmg = CreateUIText(abilitiesPanel.transform, "Weapon_Damage_Text", "PEM: 15 DMG | KATANA: 30", hdUiFont, 14,
                new Vector2(0.13f, 0.52f), new Vector2(0.95f, 0.90f));

            Image imgFillDash = CreateSciFiBarRow(abilitiesPanel.transform, "Dash_Bar", barBgSprite, barFillSprite, barBorderSprite,
                new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.44f), new Color(0.20f, 1.0f, 0.55f, 1f), 1.0f);

            Text txtDashStatus = CreateUIText(abilitiesPanel.transform, "Dash_Status_Text", "DASH: DISPONIBLE", hdUiFont, 14,
                new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.44f));

            // 3. PARTE SUPERIOR CENTRAL: BARRA DE PROGRESO DEL NIVEL EN CURSO (%)
            GameObject progressPanel = CreateHudGlassContainer(hudCanvas.transform, "Level_Progress_Panel", glassCard,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(460f, 56f));

            if (progressIconSprite != null)
            {
                CreateHudIconImage(progressPanel.transform, "Level_Progress_Icon", progressIconSprite,
                    new Vector2(0.025f, 0.14f), new Vector2(0.105f, 0.86f), new Color(0.30f, 0.96f, 1.0f, 0.95f));
            }

            Text txtLevelProgress = CreateUIText(progressPanel.transform, "Level_Progress_Text",
                isTutorial ? "PROGRESO DEL TUTORIAL: 0%" : "PROGRESO DEL NIVEL: 0%", hdUiFont, 13,
                new Vector2(0.11f, 0.52f), new Vector2(0.96f, 0.95f));
            txtLevelProgress.color = new Color(0.75f, 0.97f, 1.0f, 1f);

            Image imgFillProgress = CreateSciFiBarRow(progressPanel.transform, "Level_Progress_Bar", barBgSprite, barFillSprite, barBorderSprite,
                new Vector2(0.11f, 0.12f), new Vector2(0.96f, 0.50f), new Color(0.18f, 0.88f, 1.0f, 1f), 0.0f);

            // 4. DEBAJO DEL PROGRESO SUPERIOR CENTRAL: TARJETA DEL JEFE FINAL (ESCALA 0-100)
            GameObject bossPanel = CreateHudGlassContainer(hudCanvas.transform, "Boss_Health_Panel", glassCard,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(480f, 76f));

            Image imgFillBoss = CreateSciFiBarRow(bossPanel.transform, "Boss_Bar", barBgSprite, barFillSprite, barBorderSprite,
                new Vector2(0.04f, 0.10f), new Vector2(0.96f, 0.56f), new Color(1.0f, 0.22f, 0.36f, 1f), 1.0f);

            Text txtBossTitle = CreateUIText(bossPanel.transform, "Boss_Title_Text", "JEFE DEL NIVEL", hdUiFont, 15,
                new Vector2(0.04f, 0.58f), new Vector2(0.96f, 0.94f));
            txtBossTitle.color = new Color(1.0f, 0.85f, 0.3f, 1f);

            Text txtBossHealth = CreateUIText(bossPanel.transform, "Boss_Health_Text", "PULSERA: 100 / 100", hdUiFont, 15,
                new Vector2(0.04f, 0.10f), new Vector2(0.96f, 0.56f));

            // 5. VINCULAR CONTROLADOR DE HUD, ASSETS SCI-FI, FUENTE HD, 4 PERSONAJES, FRAMES DE PROFESOR Y AUDIO
            HUDController hud = hudCanvas.AddComponent<HUDController>();
            SerializedObject soHud = new SerializedObject(hud);
            soHud.FindProperty("isMainMenuScene").boolValue = isMainMenu;
            soHud.FindProperty("isTutorialScene").boolValue = isTutorial;
            soHud.FindProperty("isIntroCutsceneScene").boolValue = isIntroCutscene;
            soHud.FindProperty("isEndingCutsceneScene").boolValue = isEndingCutscene;
            soHud.FindProperty("nextSceneName").stringValue = nextScene;

            if (hdUiFont != null) soHud.FindProperty("customUiFont").objectReferenceValue = hdUiFont;
            if (glassCard != null) soHud.FindProperty("glassCardSprite").objectReferenceValue = glassCard;
            if (uiButtonSprite != null) soHud.FindProperty("uiButtonSprite").objectReferenceValue = uiButtonSprite;
            if (barBgSprite != null) soHud.FindProperty("uiBarBgSprite").objectReferenceValue = barBgSprite;
            if (barFillSprite != null) soHud.FindProperty("uiBarFillSprite").objectReferenceValue = barFillSprite;
            if (barBorderSprite != null) soHud.FindProperty("uiBarBorderSprite").objectReferenceValue = barBorderSprite;
            AssignPlayableCharactersToSerializedObject(soHud);

            soHud.FindProperty("playerHealthFill").objectReferenceValue = imgFillPlayer;
            soHud.FindProperty("playerHealthText").objectReferenceValue = txtPlayerHealth;
            soHud.FindProperty("playerLivesText").objectReferenceValue = txtPlayerLives;
            soHud.FindProperty("checkpointStatusText").objectReferenceValue = txtCheckpoint;
            soHud.FindProperty("levelProgressContainer").objectReferenceValue = progressPanel;
            soHud.FindProperty("levelProgressFill").objectReferenceValue = imgFillProgress;
            soHud.FindProperty("levelProgressText").objectReferenceValue = txtLevelProgress;
            soHud.FindProperty("playerChargeFill").objectReferenceValue = imgFillCharge;
            soHud.FindProperty("weaponDamageText").objectReferenceValue = txtWeaponDmg;
            soHud.FindProperty("playerDashFill").objectReferenceValue = imgFillDash;
            soHud.FindProperty("dashStatusText").objectReferenceValue = txtDashStatus;
            soHud.FindProperty("bossHealthContainer").objectReferenceValue = bossPanel;
            soHud.FindProperty("bossHealthFill").objectReferenceValue = imgFillBoss;
            soHud.FindProperty("bossTitleText").objectReferenceValue = txtBossTitle;
            soHud.FindProperty("bossHealthText").objectReferenceValue = txtBossHealth;

            if (smallEnemyPrefab != null)
            {
                soHud.FindProperty("tutorialPracticeDronePrefab").objectReferenceValue = smallEnemyPrefab;
            }

            Sprite menuBg = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/UI/MainMenu_Suspense_BG.png", 100f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            Sprite menuLogo = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                "Assets/_Project/Sprites/UI/MainMenu_Neon_Logo.png", 100f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
            if (menuBg != null) soHud.FindProperty("mainMenuBackgroundSprite").objectReferenceValue = menuBg;
            if (menuLogo != null) soHud.FindProperty("mainMenuNeonLogoSprite").objectReferenceValue = menuLogo;

            SerializedProperty sparksProp = soHud.FindProperty("electricSparkSprites");
            sparksProp.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                Sprite sp = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                    $"Assets/_Project/Sprites/VFX/EMP_Lightning_Skills/VFX1/Frames/lightning_skill1_frame{i + 1}.png",
                    100f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
                sparksProp.GetArrayElementAtIndex(i).objectReferenceValue = sp;
            }

            Sprite[] idleFrames = LoadHumanProfessorIdleFrames();
            if (idleFrames != null)
            {
                SerializedProperty idleProp = soHud.FindProperty("humanIdleFrames");
                idleProp.arraySize = idleFrames.Length;
                for (int i = 0; i < idleFrames.Length; i++) idleProp.GetArrayElementAtIndex(i).objectReferenceValue = idleFrames[i];
            }

            Sprite[] runFrames = LoadHumanProfessorRunFrames();
            if (runFrames != null)
            {
                SerializedProperty runProp = soHud.FindProperty("humanRunFrames");
                runProp.arraySize = runFrames.Length;
                for (int i = 0; i < runFrames.Length; i++) runProp.GetArrayElementAtIndex(i).objectReferenceValue = runFrames[i];
            }

            if (isEndingCutscene)
            {
                SerializedProperty expProp = soHud.FindProperty("endingExplosionFrames");
                expProp.arraySize = 10;
                for (int i = 0; i < 10; i++)
                {
                    expProp.GetArrayElementAtIndex(i).objectReferenceValue = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                        $"Assets/_Project/Sprites/VFX/EMP_Explosions/PNG/Nuclear_explosion/Nuclear_explosion{i + 1}.png",
                        100f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
                }

                SerializedProperty lightProp = soHud.FindProperty("endingLightningFrames");
                lightProp.arraySize = 8;
                for (int i = 0; i < 8; i++)
                {
                    lightProp.GetArrayElementAtIndex(i).objectReferenceValue = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                        $"Assets/_Project/Sprites/VFX/EMP_Lightning_Skills/VFX6/Frames/lightning_skill6_frame{i + 1}.png",
                        100f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
                }

                SerializedProperty robProp = soHud.FindProperty("endingRobotDeadFrames");
                robProp.arraySize = 10;
                for (int i = 0; i < 10; i++)
                {
                    robProp.GetArrayElementAtIndex(i).objectReferenceValue = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                        $"Assets/_Project/Sprites/Enemies/Robots_SIA/png/Dead ({i + 1}).png",
                        48f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
                }

                SerializedProperty wrProp = soHud.FindProperty("endingWraithDyingFrames");
                wrProp.arraySize = 15;
                for (int i = 0; i < 15; i++)
                {
                    wrProp.GetArrayElementAtIndex(i).objectReferenceValue = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                        $"Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_03/PNG Sequences/Dying/Wraith_03_Dying_{i:000}.png",
                        42f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
                }

                Sprite medalSp = BatchSpriteSheetProcessor.ConfigureSingleSprite(
                    "Assets/_Project/Sprites/UI/SciFi_Interface_Wenrexa/PNG/Icons/Icon05.png", 40f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
                if (medalSp != null) soHud.FindProperty("endingMedalSprite").objectReferenceValue = medalSp;

                Sprite scrBlue = GetLabGeneratedSprite("Lab_Screen_Blue", 100f);
                Sprite scrCode = GetLabGeneratedSprite("Lab_Screen_Code", 100f);
                if (scrBlue != null) soHud.FindProperty("endingScreenBlueSprite").objectReferenceValue = scrBlue;
                if (scrCode != null) soHud.FindProperty("endingScreenCodeSprite").objectReferenceValue = scrCode;
            }

            string cyberAudioPack = "Assets/_Project/Audio/Cyberpunk_Music_And_SFX/craftpix-net-648698-free-music-and-sounds-for-cyberpunk-game-pack-1";
            AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/Music/WAV/Track race.wav");
            AudioClip sfxShoot = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_EMP_Pulse_Normal.wav");
            AudioClip sfxCharged = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_EMP_Pulse_Charged.wav");
            AudioClip sfxChargeReady = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Charge_Ready.wav");
            AudioClip sfxKatana = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Katana_Slash.wav");
            AudioClip sfxJump = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Lost Crypt/Audio/SFX/Jump/SFX_Jump.wav");
            AudioClip sfxDash = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Dash_Slide.wav");
            AudioClip sfxPlayerHurt = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Player_Hurt.wav");
            AudioClip sfxPlayerDead = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/Sounds/Dead1.wav");
            AudioClip sfxEnemyHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Enemy_Hit.wav");
            AudioClip sfxEnemyDead = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/Sounds/Explosion.wav");
            AudioClip sfxBossCast = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Boss_Cast.wav");
            AudioClip sfxGateClose = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/Sounds/Gate close.wav");

            AudioClip trackLab = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/Music/WAV/Track Lab.wav");
            AudioClip trackIndustrial = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/Music/WAV/Track Industrial.wav");
            AudioClip trackGreenzone = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/Music/WAV/Track greenzone.wav");
            AudioClip trackRace = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/Music/WAV/Track race.wav");

            System.Collections.Generic.List<AudioClip> playlistTracks = new System.Collections.Generic.List<AudioClip>();
            if (trackLab != null) playlistTracks.Add(trackLab);
            if (trackIndustrial != null) playlistTracks.Add(trackIndustrial);
            if (trackGreenzone != null) playlistTracks.Add(trackGreenzone);
            if (trackRace != null) playlistTracks.Add(trackRace);

            if (bgmClip != null) soHud.FindProperty("bgmActionTrack").objectReferenceValue = bgmClip;
            if (sfxShoot != null) soHud.FindProperty("sfxShootNormal").objectReferenceValue = sfxShoot;
            if (sfxCharged != null) soHud.FindProperty("sfxShootCharged").objectReferenceValue = sfxCharged;
            if (sfxChargeReady != null) soHud.FindProperty("sfxChargeReady").objectReferenceValue = sfxChargeReady;
            if (sfxKatana != null) soHud.FindProperty("sfxMeleeSlash").objectReferenceValue = sfxKatana;
            if (sfxJump != null) soHud.FindProperty("sfxJump").objectReferenceValue = sfxJump;
            if (sfxDash != null) soHud.FindProperty("sfxDash").objectReferenceValue = sfxDash;
            if (sfxPlayerHurt != null) soHud.FindProperty("sfxPlayerHurt").objectReferenceValue = sfxPlayerHurt;
            if (sfxPlayerDead != null) soHud.FindProperty("sfxPlayerDead").objectReferenceValue = sfxPlayerDead;
            if (sfxEnemyHit != null) soHud.FindProperty("sfxEnemyHit").objectReferenceValue = sfxEnemyHit;
            if (sfxEnemyDead != null) soHud.FindProperty("sfxEnemyDestroyed").objectReferenceValue = sfxEnemyDead;
            if (sfxBossCast != null) soHud.FindProperty("sfxBossCast").objectReferenceValue = sfxBossCast;
            soHud.FindProperty("sfxCheckpoint").objectReferenceValue = null;
            if (sfxGateClose != null) soHud.FindProperty("sfxGateClose").objectReferenceValue = sfxGateClose;

            SerializedProperty bgmProp = soHud.FindProperty("bgmPlaylist");
            if (bgmProp != null)
            {
                bgmProp.arraySize = playlistTracks.Count;
                for (int i = 0; i < playlistTracks.Count; i++)
                {
                    bgmProp.GetArrayElementAtIndex(i).objectReferenceValue = playlistTracks[i];
                }
            }

            soHud.ApplyModifiedProperties();
        }

        private static GameObject CreateHudGlassContainer(Transform parent, string name, Sprite glassSprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            Image bg = panel.AddComponent<Image>();
            if (glassSprite != null)
            {
                bg.sprite = glassSprite;
                bg.type = Image.Type.Sliced;
            }
            bg.color = new Color(0.02f, 0.05f, 0.12f, 0.88f);
            bg.raycastTarget = false;

            return panel;
        }

        private static void CreateHudIconImage(Transform parent, string name, Sprite iconSprite, Vector2 anchorMin, Vector2 anchorMax, Color tint)
        {
            GameObject iconObj = new GameObject(name);
            iconObj.transform.SetParent(parent, false);
            RectTransform rt = iconObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = iconObj.AddComponent<Image>();
            img.sprite = iconSprite;
            img.preserveAspect = true;
            img.color = tint;
            img.raycastTarget = false;
        }

        private static Image CreateSciFiBarRow(Transform parent, string name, Sprite bgSprite, Sprite fillSprite, Sprite borderSprite,
            Vector2 anchorMin, Vector2 anchorMax, Color fillColor, float initialFill)
        {
            GameObject container = new GameObject(name);
            container.transform.SetParent(parent, false);
            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            GameObject bgObj = new GameObject("Bar_BG");
            bgObj.transform.SetParent(container.transform, false);
            RectTransform rtBg = bgObj.AddComponent<RectTransform>();
            rtBg.anchorMin = Vector2.zero;
            rtBg.anchorMax = Vector2.one;
            rtBg.offsetMin = Vector2.zero;
            rtBg.offsetMax = Vector2.zero;
            Image imgBg = bgObj.AddComponent<Image>();
            imgBg.sprite = bgSprite;
            if (bgSprite != null && bgSprite.border != Vector4.zero) imgBg.type = Image.Type.Sliced;
            imgBg.color = new Color(0.08f, 0.14f, 0.24f, 0.96f);
            imgBg.raycastTarget = false;

            GameObject fillObj = new GameObject("Bar_Fill");
            fillObj.transform.SetParent(container.transform, false);
            RectTransform rtFill = fillObj.AddComponent<RectTransform>();
            rtFill.anchorMin = new Vector2(0.015f, 0.12f);
            rtFill.anchorMax = new Vector2(0.985f, 0.88f);
            rtFill.offsetMin = Vector2.zero;
            rtFill.offsetMax = Vector2.zero;
            Image imgFill = fillObj.AddComponent<Image>();
            imgFill.sprite = fillSprite;
            imgFill.type = Image.Type.Filled;
            imgFill.fillMethod = Image.FillMethod.Horizontal;
            imgFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            imgFill.fillAmount = initialFill;
            imgFill.color = fillColor;
            imgFill.raycastTarget = false;

            if (borderSprite != null)
            {
                GameObject borderObj = new GameObject("Bar_Border");
                borderObj.transform.SetParent(container.transform, false);
                RectTransform rtBorder = borderObj.AddComponent<RectTransform>();
                rtBorder.anchorMin = Vector2.zero;
                rtBorder.anchorMax = Vector2.one;
                rtBorder.offsetMin = Vector2.zero;
                rtBorder.offsetMax = Vector2.zero;
                Image imgBorder = borderObj.AddComponent<Image>();
                imgBorder.sprite = borderSprite;
                imgBorder.color = new Color(0.75f, 0.96f, 1.0f, 0.82f);
                imgBorder.raycastTarget = false;
            }

            return imgFill;
        }

        private static Font LoadHighDefinitionUIFont()
        {
            string hdFontPath = "Assets/_Project/Sprites/UI/SegoeUIBold_HD.ttf";
            TrueTypeFontImporter fontImporter = AssetImporter.GetAtPath(hdFontPath) as TrueTypeFontImporter;
            if (fontImporter != null)
            {
                bool changed = false;
                if (fontImporter.fontSize != 64)
                {
                    fontImporter.fontSize = 64;
                    changed = true;
                }
                if (fontImporter.fontRenderingMode != FontRenderingMode.Smooth)
                {
                    fontImporter.fontRenderingMode = FontRenderingMode.Smooth;
                    changed = true;
                }
                if (changed)
                {
                    fontImporter.SaveAndReimport();
                }
            }

            Font hdFont = AssetDatabase.LoadAssetAtPath<Font>(hdFontPath);
            if (hdFont != null) return hdFont;

            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (fallback == null) fallback = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return fallback;
        }

        private static void AssignPlayableCharactersToSerializedObject(SerializedObject soTarget)
        {
            string[] controllerPaths = new string[]
            {
                "Assets/_Project/Animations/Player/PlayerAnimatorController.controller",
                "Assets/_Project/Animations/Player/PlayerAnimator_NinjaMale.overrideController",
                "Assets/_Project/Animations/Player/PlayerAnimator_CyberRobot.overrideController",
                "Assets/_Project/Animations/Player/PlayerAnimator_Wraith02.overrideController"
            };

            string[] idleSpritePaths = new string[]
            {
                "Assets/_Project/Sprites/Player/Ninja_Female/png/Idle__000.png",
                "Assets/_Project/Sprites/Player/Ninja_Male/png/Idle__000.png",
                "Assets/_Project/Sprites/Player/Ninja_Female_Cyber/png/Idle__000.png",
                "Assets/_Project/Sprites/Player/Ninja_Male_Emerald/png/Idle__000.png"
            };

            SerializedProperty ctrlProp = soTarget.FindProperty("playableCharacterControllers");
            if (ctrlProp != null)
            {
                ctrlProp.arraySize = controllerPaths.Length;
                for (int i = 0; i < controllerPaths.Length; i++)
                {
                    RuntimeAnimatorController ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPaths[i]);
                    ctrlProp.GetArrayElementAtIndex(i).objectReferenceValue = ctrl;
                }
            }

            SerializedProperty spriteProp = soTarget.FindProperty("playableCharacterIdleSprites");
            if (spriteProp != null)
            {
                spriteProp.arraySize = idleSpritePaths.Length;
                for (int i = 0; i < idleSpritePaths.Length; i++)
                {
                    Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(idleSpritePaths[i]);
                    spriteProp.GetArrayElementAtIndex(i).objectReferenceValue = sp;
                }
            }
        }

        private static Text CreateUIText(Transform parent, string name, string content, Font font, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject containerObj = new GameObject(name);
            containerObj.transform.SetParent(parent, false);
            RectTransform rtContainer = containerObj.AddComponent<RectTransform>();
            rtContainer.anchorMin = anchorMin;
            rtContainer.anchorMax = anchorMax;
            rtContainer.offsetMin = Vector2.zero;
            rtContainer.offsetMax = Vector2.zero;

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
            if (font != null) txt.font = font;
            txt.text = content;
            txt.fontSize = Mathf.Max(24, (fontSize + 2) * 2);
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;

            Shadow crispShadow = txtObj.AddComponent<Shadow>();
            crispShadow.effectColor = new Color(0f, 0f, 0f, 0.92f);
            crispShadow.effectDistance = new Vector2(2.2f, -2.2f);

            return txt;
        }

        private static GameObject CreateOrGetSmallEnemyPrefab()
        {
            string prefabFolder = "Assets/_Project/Prefabs/Enemies";
            string prefabPath = $"{prefabFolder}/SmallEnemyPrefab.prefab";

            AssetDatabase.DeleteAsset(prefabPath);

            GameObject temp = new GameObject("SmallEnemyPrefab");

            SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
            Sprite robotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Enemies/Robots_SIA/png/Run (1).png");
            if (robotSprite != null)
            {
                sr.sprite = robotSprite;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = GetOrCreateCircleSprite();
                sr.color = new Color(1.0f, 0.45f, 0.1f);
            }
            sr.sortingOrder = 5;

            Animator anim = temp.AddComponent<Animator>();
            RuntimeAnimatorController robotCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/Enemies/RobotAnimatorController.controller");
            if (robotCtrl != null) anim.runtimeAnimatorController = robotCtrl;

            Rigidbody2D rb = temp.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2.5f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D col = temp.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.85f, 1.35f);
            col.offset = Vector2.zero;
            col.sharedMaterial = GetOrCreateFrictionlessMaterial();

            SmallEnemyAI ai = temp.AddComponent<SmallEnemyAI>();
            SerializedObject soAi = new SerializedObject(ai);
            soAi.FindProperty("maxHealth").intValue = 38;
            soAi.FindProperty("currentHealth").intValue = 38;
            soAi.FindProperty("contactDamage").intValue = 8;
            soAi.ApplyModifiedProperties();

            AttachWorldHealthBar(temp.transform, 0.95f, 1.25f, 0.22f, new Color(0.15f, 0.95f, 0.35f, 1f));

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            UnityEngine.Object.DestroyImmediate(temp);
            AssetDatabase.SaveAssets();

            return saved;
        }

        private static GameObject CreateOrGetEMPImpactPrefab()
        {
            string prefabFolder = "Assets/_Project/Prefabs/VFX";
            string prefabPath = $"{prefabFolder}/EMPPulseImpactPrefab.prefab";

            GameObject tempImpact = new GameObject("EMPPulseImpactPrefab");
            SpriteRenderer sr = tempImpact.AddComponent<SpriteRenderer>();

            Sprite impactSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/VFX/EMP_Lightning_Skills/VFX6/Frames/lightning_skill6_frame4.png");
            sr.sprite = impactSprite != null ? impactSprite : GetOrCreateCircleSprite();
            sr.sortingOrder = 9;

            Animator anim = tempImpact.AddComponent<Animator>();
            RuntimeAnimatorController impactCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/VFX/ImpactAnimatorController.controller");
            if (impactCtrl != null) anim.runtimeAnimatorController = impactCtrl;

            tempImpact.AddComponent<AutoDestroyAfterTime>();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(tempImpact, prefabPath);
            UnityEngine.Object.DestroyImmediate(tempImpact);
            AssetDatabase.SaveAssets();
            return saved;
        }

        private static GameObject CreateOrGetEMPPulsePrefab()
        {
            string prefabFolder = "Assets/_Project/Prefabs/VFX";
            string prefabPath = $"{prefabFolder}/EMPPulsePrefab.prefab";

            GameObject impactVfx = CreateOrGetEMPImpactPrefab();

            GameObject tempPulse = new GameObject("EMPPulsePrefab");
            SpriteRenderer sr = tempPulse.AddComponent<SpriteRenderer>();

            Sprite pulseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/VFX/EMP_Lightning_Skills/VFX1/Frames/lightning_skill1_frame1.png");
            if (pulseSprite != null)
            {
                sr.sprite = pulseSprite;
                sr.color = Color.white;
                tempPulse.transform.localScale = Vector3.one;
            }
            else
            {
                sr.sprite = GetOrCreateCircleSprite();
                sr.color = new Color(0f, 0.95f, 1f, 0.95f);
                tempPulse.transform.localScale = new Vector3(0.5f, 0.3f, 1f);
            }
            sr.sortingOrder = 8;

            Animator anim = tempPulse.AddComponent<Animator>();
            RuntimeAnimatorController pulseCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/VFX/PulseAnimatorController.controller");
            if (pulseCtrl != null) anim.runtimeAnimatorController = pulseCtrl;

            CircleCollider2D col = tempPulse.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            Rigidbody2D rb = tempPulse.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            EMPPulse pulse = tempPulse.AddComponent<EMPPulse>();
            SerializedObject soPulse = new SerializedObject(pulse);
            soPulse.FindProperty("impactVfxPrefab").objectReferenceValue = impactVfx;
            soPulse.ApplyModifiedProperties();

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempPulse, prefabPath);
            UnityEngine.Object.DestroyImmediate(tempPulse);
            AssetDatabase.SaveAssets();

            return savedPrefab;
        }

        private static Sprite GetOrCreateSquareSprite()
        {
            string path = "Assets/_Project/Sprites/Square.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite GetOrCreateCircleSprite()
        {
            string path = "Assets/_Project/Sprites/Circle.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 64;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static PhysicsMaterial2D GetOrCreateFrictionlessMaterial()
        {
            string matPath = "Assets/_Project/Materials/PlayerMaterial.physicsMaterial2D";
            PhysicsMaterial2D mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
            if (mat == null)
            {
                mat = new PhysicsMaterial2D("PlayerMaterial");
                mat.friction = 0f;
                mat.bounciness = 0f;
                AssetDatabase.CreateAsset(mat, matPath);
                AssetDatabase.SaveAssets();
            }
            return mat;
        }
    }
}
#endif
