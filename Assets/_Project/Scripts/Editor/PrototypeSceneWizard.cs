#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using CodigoDeEmergencia.Core;
using CodigoDeEmergencia.Player;
using CodigoDeEmergencia.Combat;
using CodigoDeEmergencia.Enemies;
using CodigoDeEmergencia.UI;

namespace CodigoDeEmergencia.EditorTools
{
    /// <summary>
    /// Asistente para montar el Nivel con los nuevos Sprites HD (No-IA):
    /// - Protagonista: Ninja_Female con sus 10 animaciones completas (Idle, Run, Jump, Slide/Dash, Throw, Jump_Throw, Attack, Jump_Attack, Glide, Climb, Dead).
    /// - Mapa limpio sin imágenes de fondo IA.
    /// - Plataformas, muros, escaleras trepables y terminales construidos con Tileset_Industrial_Lab (Kenney Retina HD).
    /// - Mini-enemigos con sprites animados de Robots_SIA y proyectiles eléctricos de EMP_Lightning_Skills.
    /// </summary>
    public static class PrototypeSceneWizard
    {
        private const string IndustrialRetinaFolder = "Assets/_Project/Sprites/World/Tileset_Industrial_Lab/PNG/Retina";
        private const string WenrexaUiFolder = "Assets/_Project/Sprites/UI/SciFi_Interface_Wenrexa/PNG";
        private const string AutoSceneRebuildKey = "CDE_NinjaFemaleAndIndustrialMapBuilt_v2";

        [InitializeOnLoadMethod]
        private static void AutoRebuildSceneOnce()
        {
            if (SessionState.GetBool(AutoSceneRebuildKey, false)) return;
            SessionState.SetBool(AutoSceneRebuildKey, true);

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                SetupPrototypeSceneSilent();
            };
        }

        [MenuItem("Tools/Código de Emergencia/🚀 Montar Escena Prototipo (Avance 1)")]
        public static void SetupPrototypeScene()
        {
            SetupPrototypeSceneSilent();

            EditorUtility.DisplayDialog(
                "¡Nivel Laberinto de 6 Plantas, Jefe 3x, Checkpoints, UI y Audio Listos!",
                "Se aplicaron todas las mejoras solicitadas:\n\n" +
                "• Escaleras y Plataformas: One-Way PlatformEffector2D + paso libre al subir/bajar con W/S.\n" +
                "• Inicio y 2 Checkpoints: Jugador inicia en el extremo izquierdo (X = -21) con Checkpoint #1 (X = +42) y Checkpoint #2 (X = +114).\n" +
                "• Nivel Laberinto 6x más largo (-24 a +186 = 210m) y 6 Plantas de altura sin objetos que bloqueen el suelo.\n" +
                "• Jefe Final al extremo derecho (X = +172): Wraith_03 (3 veces más grande que el jugador = 6m), detecta en pantalla compartida (13.5m) y ataca a distancia.\n" +
                "• Disparo Cargado corregido: mantener presionado ya NO dispara un tiro pequeño antes del cargado.\n" +
                "• HUD Sci-Fi Wenrexa: Vida arriba a la derecha; Carga PEM y Disponibilidad de Dash abajo a la izquierda.\n" +
                "• Sistema de Audio completo: Música de acción/suspenso y efectos de sonido para todas las acciones.",
                "¡A Jugar!"
            );
        }

        public static void SetupPrototypeSceneSilent()
        {
            // 0. Procesar primero todos los clips y controladores de Ninja_Female, Wraith_03 (Boss 3x), Robots y Rayos PEM
            BatchSpriteSheetProcessor.ProcessAllSpriteSheetsSilent();
            PhysicsMaterial2D playerMat = GetOrCreateFrictionlessMaterial();

            // 1. Prefab del Proyectil PEM eléctrico y su impacto
            GameObject pulsePrefab = CreateOrGetEMPPulsePrefab();

            // 2. Prefab del Mini-Enemigo (Robot S.I.A.)
            GameObject smallEnemyPrefab = CreateOrGetSmallEnemyPrefab();

            // 3. Eliminar cualquier fondo anterior para dejar el mapa 100% limpio
            RemoveOldBackgroundsAndCleanCamera();

            // 4. Construir Laberinto Industrial de 210m x 6 Plantas, 2 Checkpoints y Escaleras One-Way
            SetupIndustrialEnvironment();

            // 5. Configurar a la Protagonista (Ninja_Female) en el extremo izquierdo del mapa (X = -21)
            GameObject player = SetupPlayer(pulsePrefab, playerMat);

            // 6. Configurar Patrullas Intermedias y al Jefe Final (3x tamaño = 6m, ataque a distancia, al final del nivel X = +172)
            SetupTeacherBoss();

            // 7. Generador de Mini-Enemigos (Robots S.I.A.)
            SetupSpawner(smallEnemyPrefab);

            // 8. Crear Interfaz de Usuario (HUD Sci-Fi Wenrexa) y Sistema de Audio (BGM + 12 SFX)
            SetupHUD();

            // 9. Configurar Cámara Suave con límites del nivel laberinto
            Camera mainCam = Camera.main;
            if (mainCam != null && player != null)
            {
                mainCam.orthographicSize = 6.2f;
                CameraFollow2D camFollow = mainCam.GetComponent<CameraFollow2D>();
                if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow2D>();
                camFollow.SetTarget(player.transform);
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("<color=green><b>[SISTEMA]</b> Nivel Laberinto (210m x 6 Plantas), 2 Checkpoints, Jefe 3x con ataque a distancia, HUD Sci-Fi y Audio montados con éxito.</color>");
        }

        private static void RemoveOldBackgroundsAndCleanCamera()
        {
            GameObject bgParent = GameObject.Find("--- Background_Parallax ---");
            if (bgParent != null)
            {
                Object.DestroyImmediate(bgParent);
            }

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0.09f, 0.12f, 0.18f, 1f);
            }
        }

        private static void SetupIndustrialEnvironment()
        {
            GameObject env = GameObject.Find("--- Environment ---");
            if (env != null) Object.DestroyImmediate(env);

            env = new GameObject("--- Environment ---");

            // Cargar tiles industriales Retina HD (configurados con FullRect para Tiled DrawMode)
            Sprite groundSurfaceTile = GetIndustrialTile("platformIndustrial_002", 140f);
            Sprite steelBlockTile = GetIndustrialTile("platformIndustrial_029", 140f);
            Sprite steelBeamTile = GetIndustrialTile("platformIndustrial_050", 140f);
            Sprite hazardBeamTile = GetIndustrialTile("platformIndustrial_064", 140f);
            Sprite serverRackTile = GetIndustrialTile("platformIndustrial_071", 140f);
            Sprite ladderMidTile = GetIndustrialTile("platformIndustrial_047", 140f);
            Sprite terminalScreenTile = GetIndustrialTile("platformIndustrial_045", 140f);
            Sprite sirenRedTile = GetIndustrialTile("platformIndustrial_054", 140f);

            // Alturas de superficie de las 6 Plantas (separación vertical = 3.6m entre pisos):
            // Planta 1 (Suelo Base): Y_top = -1.75 (centro -2.25, alto 1.0)
            // Planta 2: Y_top = +1.85 (centro +1.45, alto 0.8)
            // Planta 3: Y_top = +5.45 (centro +5.05, alto 0.8)
            // Planta 4: Y_top = +9.05 (centro +8.65, alto 0.8)
            // Planta 5: Y_top = +12.65 (centro +12.25, alto 0.8)
            // Planta 6: Y_top = +16.25 (centro +15.85, alto 0.8)

            // 1. Suelo Principal Industrial (X = -24 a X = +186 -> 210 metros de largo, centro X = 81)
            CreateTiledPlatform(env.transform, "Ground_Main_Surface", groundSurfaceTile, new Vector3(81f, -2.25f, 0f), new Vector2(210f, 1.0f), true, 0);
            CreateTiledPlatform(env.transform, "Ground_Main_Subfloor", steelBlockTile, new Vector3(81f, -3.25f, 0f), new Vector2(210f, 1.0f), false, -1);

            // 2. Techo Industrial (Y = 22.0) y Muros Perimetrales Izquierdo (X = -24) y Derecho (X = +186)
            CreateTiledPlatform(env.transform, "Ceiling_Main", steelBeamTile, new Vector3(81f, 22.0f, 0f), new Vector2(212f, 1.2f), true, 0);
            CreateTiledPlatform(env.transform, "Wall_Left_Boundary", steelBlockTile, new Vector3(-24f, 9.5f, 0f), new Vector2(2.0f, 25.0f), true, 0);
            CreateTiledPlatform(env.transform, "Wall_Right_Boundary", steelBlockTile, new Vector3(186f, 9.5f, 0f), new Vector2(2.0f, 25.0f), true, 0);

            // =========================================================================================
            // SECTOR 1 (X = -21 a +48): LABERINTO DE RETROCESO Y ASCENSO (Planta 1 -> 2 -> 1 -> 2 -> 3)
            // =========================================================================================
            // El jugador inicia en X = -21 (Planta 1). Avanza por Planta 1 y en X = +6 encuentra Wall_Block_S1_A.
            // Debe retroceder un poco a X = -6 para subir por Ladder_S1_1to2_A a Planta 2, pasar el bloqueo,
            // volver a bajar a Planta 1 en X = +11 (porque Wall_Block_S1_B bloquea Planta 2 en X = +16),
            // avanzar hasta X = +32 donde Wall_Block_S1_C bloquea Planta 1 y 2, y subir hasta Planta 3 para pasar.
            CreateTiledPlatform(env.transform, "Wall_Block_S1_A", hazardBeamTile, new Vector3(6.0f, 0.05f, 0f), new Vector2(1.4f, 3.6f), true, 1);
            CreateTiledPlatform(env.transform, "Platform_S1_P2_West", steelBeamTile, new Vector3(2.0f, 1.45f, 0f), new Vector2(26f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S1_1to2_A", ladderMidTile, new Vector3(-6.0f, 0.35f, 0f), 4.3f);

            // Bloqueo en Planta 2 (X = +16) que obliga a bajar de nuevo a Planta 1 tras cruzar Wall_Block_S1_A
            CreateTiledPlatform(env.transform, "Wall_Block_S1_B", steelBlockTile, new Vector3(16.0f, 5.45f, 0f), new Vector2(1.4f, 7.2f), true, 1);
            CreateClimbableLadder(env.transform, "Ladder_S1_2to1_Drop", ladderMidTile, new Vector3(11.5f, 0.35f, 0f), 4.3f);

            // Bloqueo alto en X = +32 (cubre Planta 1 y Planta 2) que obliga a subir a Planta 2 y luego a Planta 3
            CreateTiledPlatform(env.transform, "Wall_Block_S1_C", hazardBeamTile, new Vector3(32.0f, 1.85f, 0f), new Vector2(1.4f, 7.2f), true, 1);
            CreateTiledPlatform(env.transform, "Platform_S1_P2_East", steelBeamTile, new Vector3(25.0f, 1.45f, 0f), new Vector2(12f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S1_1to2_B", ladderMidTile, new Vector3(21.5f, 0.35f, 0f), 4.3f);

            // Planta 3 sobre Wall_Block_S1_C (X = +24 a +50, Y_top = +5.45) donde está el CHECKPOINT #1
            CreateTiledPlatform(env.transform, "Platform_S1_P3_Bridge", hazardBeamTile, new Vector3(37.0f, 5.05f, 0f), new Vector2(26f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S1_2to3", ladderMidTile, new Vector3(28.5f, 3.95f, 0f), 4.3f);

            // =========================================================================================
            // SECTOR 2 (X = +48 a +118): LA TORRE LABERINTO DE 6 PLANTAS (Planta 1 hasta Planta 6)
            // =========================================================================================
            // Gran Muro Reactor Central en X = +76 que bloquea desde Planta 1 hasta Planta 5 (Y = -1.75 a +16.25).
            // Para cruzarlo hay que ascender en zig-zag hasta la Planta 6 (Skybridge en Y = +16.25).
            CreateTiledPlatform(env.transform, "Wall_Block_S2_Reactor", hazardBeamTile, new Vector3(76.0f, 7.25f, 0f), new Vector2(1.6f, 18.0f), true, 1);

            // Plataformas occidentales de la Torre (X = +46 a +75)
            CreateTiledPlatform(env.transform, "Platform_S2_P2_West", steelBeamTile, new Vector3(60.0f, 1.45f, 0f), new Vector2(24f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S2_1to2_West", ladderMidTile, new Vector3(52.0f, 0.35f, 0f), 4.3f);
            CreateClimbableLadder(env.transform, "Ladder_S2_2to3_West", ladderMidTile, new Vector3(48.5f, 3.95f, 0f), 4.3f);

            // Planta 4 (Y_top = +9.05) con bloqueo intermedio en X = +68
            CreateTiledPlatform(env.transform, "Platform_S2_P4_West", steelBeamTile, new Vector3(56.0f, 8.65f, 0f), new Vector2(24f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S2_3to4", ladderMidTile, new Vector3(46.5f, 7.55f, 0f), 4.3f);
            CreateTiledPlatform(env.transform, "Wall_Block_S2_P4", steelBlockTile, new Vector3(68.0f, 10.85f, 0f), new Vector2(1.2f, 3.6f), true, 1);

            // Planta 5 (Y_top = +12.65)
            CreateTiledPlatform(env.transform, "Platform_S2_P5_West", hazardBeamTile, new Vector3(62.0f, 12.25f, 0f), new Vector2(24f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S2_4to5", ladderMidTile, new Vector3(53.0f, 11.15f, 0f), 4.3f);

            // Planta 6 — Puente Superior (Y_top = +16.25) que cruza por encima del Reactor Central (X = +64 a +96)
            CreateTiledPlatform(env.transform, "Platform_S2_P6_Skybridge", steelBeamTile, new Vector3(80.0f, 15.85f, 0f), new Vector2(32f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S2_5to6", ladderMidTile, new Vector3(71.0f, 14.75f, 0f), 4.3f);

            // Descenso Oriental de la Torre (Planta 6 -> 5 -> 4 -> 3 -> 2) hacia el CHECKPOINT #2 (X = +114)
            CreateTiledPlatform(env.transform, "Platform_S2_P5_East", steelBeamTile, new Vector3(96.0f, 12.25f, 0f), new Vector2(18f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S2_6to5_East", ladderMidTile, new Vector3(92.0f, 14.75f, 0f), 4.3f);

            CreateTiledPlatform(env.transform, "Platform_S2_P4_East", hazardBeamTile, new Vector3(102.0f, 8.65f, 0f), new Vector2(18f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S2_5to4_East", ladderMidTile, new Vector3(101.0f, 11.15f, 0f), 4.3f);

            CreateTiledPlatform(env.transform, "Platform_S2_P3_East", steelBeamTile, new Vector3(96.0f, 5.05f, 0f), new Vector2(20f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S2_4to3_East", ladderMidTile, new Vector3(95.0f, 7.55f, 0f), 4.3f);

            CreateTiledPlatform(env.transform, "Platform_S2_P2_East", hazardBeamTile, new Vector3(106.0f, 1.45f, 0f), new Vector2(24f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S2_3to2_East", ladderMidTile, new Vector3(103.0f, 3.95f, 0f), 4.3f);

            // =========================================================================================
            // SECTOR 3 (X = +118 a +186): COMPUERTA ORIENTAL Y GRAN ARENA DEL JEFE FINAL
            // =========================================================================================
            // Bloqueo en X = +138 (cubre Planta 1, 2 y 3). El jugador baja a Planta 1 en X = +118 y sube hasta Planta 4.
            CreateClimbableLadder(env.transform, "Ladder_S3_2to1_Entry", ladderMidTile, new Vector3(116.5f, 0.35f, 0f), 4.3f);
            CreateTiledPlatform(env.transform, "Wall_Block_S3_Gate", hazardBeamTile, new Vector3(138.0f, 3.65f, 0f), new Vector2(1.5f, 10.8f), true, 1);

            CreateTiledPlatform(env.transform, "Platform_S3_P2", steelBeamTile, new Vector3(128.0f, 1.45f, 0f), new Vector2(16f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S3_1to2", ladderMidTile, new Vector3(123.0f, 0.35f, 0f), 4.3f);

            CreateTiledPlatform(env.transform, "Platform_S3_P3", steelBeamTile, new Vector3(128.0f, 5.05f, 0f), new Vector2(16f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S3_2to3", ladderMidTile, new Vector3(133.0f, 3.95f, 0f), 4.3f);

            CreateTiledPlatform(env.transform, "Platform_S3_P4_OverGate", hazardBeamTile, new Vector3(136.0f, 8.65f, 0f), new Vector2(24f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_S3_3to4", ladderMidTile, new Vector3(126.0f, 7.55f, 0f), 4.3f);

            // Plataformas Tácticas dentro de la Arena del Jefe Final (X = +150 a +184)
            CreateTiledPlatform(env.transform, "Platform_Arena_P2_Left", steelBeamTile, new Vector3(156.0f, 1.45f, 0f), new Vector2(14f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_Arena_1to2", ladderMidTile, new Vector3(151.0f, 0.35f, 0f), 4.3f);
            CreateTiledPlatform(env.transform, "Platform_Arena_P3_Mid", hazardBeamTile, new Vector3(166.0f, 5.05f, 0f), new Vector2(16f, 0.8f), true, 0);
            CreateClimbableLadder(env.transform, "Ladder_Arena_2to3", ladderMidTile, new Vector3(160.0f, 3.95f, 0f), 4.3f);

            // =========================================================================================
            // 2 CHECKPOINTS INTERACTIVOS DEL NIVEL
            // =========================================================================================
            CreateCheckpointStation(env.transform, "Checkpoint_1", terminalScreenTile, sirenRedTile, new Vector3(42.0f, 5.45f, 0f));
            CreateCheckpointStation(env.transform, "Checkpoint_2", terminalScreenTile, sirenRedTile, new Vector3(114.0f, 1.85f, 0f));

            // =========================================================================================
            // DECORACIÓN DE LABORATORIO (100% EN FONDO SIN COLISIONADORES — NO BLOQUEAN EL PASO)
            // =========================================================================================
            float[] decorX = { -16f, -2f, 12f, 24f, 38f, 54f, 64f, 82f, 94f, 108f, 126f, 146f, 158f, 170f, 178f };
            for (int i = 0; i < decorX.Length; i++)
            {
                float x = decorX[i];
                Sprite propSprite = (i % 2 == 0) ? serverRackTile : terminalScreenTile;
                // Planta 1 fondo (elevado ligeramente y sin colisionador)
                CreateTiledPlatform(env.transform, $"Prop_P1_Bg_{i}", propSprite, new Vector3(x, -0.55f, 0f), new Vector2(1.8f, 1.8f), false, -2);
                CreateTiledPlatform(env.transform, $"Prop_Siren_P1_{i}", sirenRedTile, new Vector3(x, 1.1f, 0f), new Vector2(0.7f, 0.7f), false, -2);

                // Planta 2/3/4 fondo en la torre central
                if (x >= 44f && x <= 108f)
                {
                    CreateTiledPlatform(env.transform, $"Prop_P3_Bg_{i}", serverRackTile, new Vector3(x, 6.65f, 0f), new Vector2(1.8f, 1.8f), false, -2);
                    CreateTiledPlatform(env.transform, $"Prop_P5_Bg_{i}", terminalScreenTile, new Vector3(x, 13.85f, 0f), new Vector2(1.8f, 1.8f), false, -2);
                }
            }
        }

        private static void CreateCheckpointStation(Transform parent, string name, Sprite terminalSprite, Sprite beaconSprite, Vector3 surfaceFeetPos)
        {
            GameObject cp = new GameObject(name);
            cp.transform.SetParent(parent);
            cp.transform.position = surfaceFeetPos + new Vector3(0f, 1.1f, 0f);

            SpriteRenderer sr = cp.AddComponent<SpriteRenderer>();
            sr.sprite = terminalSprite != null ? terminalSprite : GetOrCreateSquareSprite();
            sr.color = new Color(0.35f, 0.85f, 1.0f, 1f);
            sr.sortingOrder = -1;

            BoxCollider2D trigger = cp.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.8f, 2.2f);

            GameObject beacon = new GameObject("Checkpoint_Beacon_Light");
            beacon.transform.SetParent(cp.transform, false);
            beacon.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            beacon.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            SpriteRenderer srBeacon = beacon.AddComponent<SpriteRenderer>();
            srBeacon.sprite = beaconSprite != null ? beaconSprite : GetOrCreateCircleSprite();
            srBeacon.color = new Color(1.0f, 0.75f, 0.1f, 1f);
            srBeacon.sortingOrder = 0;
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

                // Todas las plataformas elevadas ("Platform_") usan PlatformEffector2D One-Way
                // para poder atravesarlas libremente desde abajo al saltar o al subir escaleras
                if (name.StartsWith("Platform_", System.StringComparison.OrdinalIgnoreCase))
                {
                    col.usedByEffector = true;
                    PlatformEffector2D effector = obj.AddComponent<PlatformEffector2D>();
                    effector.useOneWay = true;
                    effector.useOneWayGrouping = true;
                    effector.surfaceArc = 165f;
                    effector.useSideFriction = false;
                    effector.useSideBounce = false;
                }
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

            // FullRect es necesario en Unity para usar SpriteDrawMode.Tiled sin advertencias
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

        private static GameObject SetupPlayer(GameObject pulsePrefab, PhysicsMaterial2D playerMat)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = new GameObject("Player");
                player.tag = "Player";
            }

            // Colocar los pies directamente en el inicio del nivel (extremo izquierdo X = -21f, Y = -1.75f)
            player.transform.position = new Vector3(-21f, -1.75f, 0f);
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

            // Configurar FirePoint
            Transform firePoint = player.transform.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject fpObj = new GameObject("FirePoint");
                fpObj.transform.SetParent(player.transform);
                firePoint = fpObj.transform;
            }
            firePoint.localPosition = new Vector3(0.85f, 1.05f, 0f);

            // Configurar GroundCheck en los pies
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
            soMovement.FindProperty("jumpForce").floatValue = 15.5f;
            soMovement.ApplyModifiedProperties();

            GameObject impactVfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/VFX/EMPPulseImpactPrefab.prefab");
            SerializedObject soShooting = new SerializedObject(shooting);
            soShooting.FindProperty("empPulsePrefab").objectReferenceValue = pulsePrefab;
            soShooting.FindProperty("firePoint").objectReferenceValue = firePoint;
            soShooting.FindProperty("baseDamage").intValue = 10;
            soShooting.FindProperty("chargedDamage").intValue = 30;
            soShooting.FindProperty("meleeDamage").intValue = 25;
            if (impactVfx != null) soShooting.FindProperty("meleeHitVfxPrefab").objectReferenceValue = impactVfx;
            soShooting.ApplyModifiedProperties();

            return player;
        }

        private static void SetupTeacherBoss()
        {
            // Limpiar instancias previas
            string[] oldNames = { "Teacher_Boss", "Teacher_Prototype", "Teacher_Patrol_1", "Teacher_Patrol_2" };
            for (int i = 0; i < oldNames.Length; i++)
            {
                GameObject old = GameObject.Find(oldNames[i]);
                if (old != null) Object.DestroyImmediate(old);
            }

            // 1. Patrullas intermedias (Docentes en trance en Sector 2, tamaño normal 2.2m, sin ataque a distancia)
            CreateWraithEnemy("Teacher_Patrol_1", new Vector3(56f, -1.75f, 0f), isFinalBoss: false, braceletHp: 180);
            CreateWraithEnemy("Teacher_Patrol_2", new Vector3(98f, -1.75f, 0f), isFinalBoss: false, braceletHp: 220);

            // 2. Jefe Final en el extremo derecho del nivel (X = +172, 3x más grande que el jugador = 6.0m, detecta en pantalla y ataca a distancia)
            CreateWraithEnemy("Teacher_Boss", new Vector3(172f, -1.75f, 0f), isFinalBoss: true, braceletHp: 500);
        }

        private static void CreateWraithEnemy(string objName, Vector3 pos, bool isFinalBoss, int braceletHp)
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

            // Indicador luminoso de la Pulsera Inteligente S.I.A.
            GameObject braceletVisual = new GameObject("Bracelet_SIA_Glow");
            braceletVisual.transform.SetParent(teacher.transform, false);
            braceletVisual.transform.localPosition = isFinalBoss ? new Vector3(1.1f, 3.0f, 0f) : new Vector3(0.45f, 1.1f, 0f);
            braceletVisual.transform.localScale = isFinalBoss ? new Vector3(0.55f, 0.55f, 1f) : new Vector3(0.28f, 0.28f, 1f);
            SpriteRenderer srBracelet = braceletVisual.AddComponent<SpriteRenderer>();
            srBracelet.sprite = GetOrCreateCircleSprite();
            srBracelet.color = new Color(1.0f, 0.15f, 0.9f, 0.9f);
            srBracelet.sortingOrder = 6;

            Rigidbody2D rb = teacher.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            // El Jefe Final es 3 veces más grande que el jugador (Player = 2.0m -> Boss = 6.0m de alto)
            BoxCollider2D col = teacher.AddComponent<BoxCollider2D>();
            col.size = isFinalBoss ? new Vector2(2.3f, 5.8f) : new Vector2(0.95f, 2.1f);
            col.offset = isFinalBoss ? new Vector2(0f, 2.9f) : new Vector2(0f, 1.05f);
            col.sharedMaterial = GetOrCreateFrictionlessMaterial();

            BraceletHealth bracelet = teacher.AddComponent<BraceletHealth>();
            bracelet.ConfigureHealth(braceletHp);

            TeacherAI ai = teacher.AddComponent<TeacherAI>();
            Sprite projSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_03/Vector Parts/Spells-Effect.png");
            SerializedObject soAi = new SerializedObject(ai);
            soAi.FindProperty("detectionRange").floatValue = isFinalBoss ? 13.5f : 8.0f;
            soAi.FindProperty("canRangedAttack").boolValue = isFinalBoss;
            soAi.FindProperty("rangedAttackInterval").floatValue = 2.0f;
            soAi.FindProperty("rangedDamage").intValue = 15;
            soAi.FindProperty("projectileSpeed").floatValue = 11.5f;
            soAi.FindProperty("contactDamage").intValue = isFinalBoss ? 25 : 15;
            if (projSprite != null) soAi.FindProperty("projectileSprite").objectReferenceValue = projSprite;
            soAi.ApplyModifiedProperties();

            // Barra flotante sobre la cabeza
            float barHeight = isFinalBoss ? 6.25f : 2.45f;
            float barWidth = isFinalBoss ? 2.8f : 1.5f;
            GameObject healthBarObj = new GameObject("FloatingHealthBar");
            healthBarObj.transform.SetParent(teacher.transform, false);
            healthBarObj.transform.localPosition = new Vector3(0f, barHeight, 0f);

            GameObject barBg = new GameObject("Bar_BG");
            barBg.transform.SetParent(healthBarObj.transform, false);
            SpriteRenderer srBg = barBg.AddComponent<SpriteRenderer>();
            srBg.sprite = GetOrCreateSquareSprite();
            srBg.color = new Color(0.08f, 0.10f, 0.16f, 0.95f);
            srBg.sortingOrder = 10;
            barBg.transform.localScale = new Vector3(barWidth, 0.26f, 1f);

            GameObject barFill = new GameObject("Bar_Fill");
            barFill.transform.SetParent(healthBarObj.transform, false);
            SpriteRenderer srFill = barFill.AddComponent<SpriteRenderer>();
            srFill.sprite = GetOrCreateSquareSprite();
            srFill.color = isFinalBoss ? new Color(1.0f, 0.18f, 0.35f, 1f) : new Color(0.95f, 0.35f, 0.15f, 1f);
            srFill.sortingOrder = 11;
            barFill.transform.localScale = new Vector3(barWidth - 0.08f, 0.20f, 1f);

            FloatingHealthBar floatingBar = healthBarObj.AddComponent<FloatingHealthBar>();
            SerializedObject soBar = new SerializedObject(floatingBar);
            soBar.FindProperty("fillTransform").objectReferenceValue = barFill.transform;
            soBar.FindProperty("offset").vector3Value = new Vector3(0f, barHeight, 0f);
            soBar.ApplyModifiedProperties();
        }

        private static void SetupSpawner(GameObject smallEnemyPrefab)
        {
            GameObject spawnerObj = GameObject.Find("--- Enemy_Spawner ---");
            if (spawnerObj == null) spawnerObj = new GameObject("--- Enemy_Spawner ---");

            EnemySpawner spawner = spawnerObj.GetComponent<EnemySpawner>();
            if (spawner == null) spawner = spawnerObj.AddComponent<EnemySpawner>();

            spawner.SetEnemyPrefab(smallEnemyPrefab);
        }

        private static Sprite LoadUiSprite(string relativePath)
        {
            string fullPath = $"{WenrexaUiFolder}/{relativePath}";
            return BatchSpriteSheetProcessor.ConfigureSingleSprite(fullPath, 100f, new Vector2(0.5f, 0.5f), FilterMode.Bilinear);
        }

        private static void SetupHUD()
        {
            GameObject hudCanvas = GameObject.Find("HUD_Canvas");
            if (hudCanvas != null) Object.DestroyImmediate(hudCanvas);

            hudCanvas = new GameObject("HUD_Canvas");
            Canvas canvas = hudCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = hudCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            hudCanvas.AddComponent<GraphicRaycaster>();

            Sprite squareSprite = GetOrCreateSquareSprite();
            Sprite barV1Bg = LoadUiSprite("ProgressBar_01/BarV1_Bar.png") ?? squareSprite;
            Sprite barV1Fill = LoadUiSprite("ProgressBar_01/BarV1_ProgressBar.png") ?? squareSprite;
            Sprite barV1Border = LoadUiSprite("ProgressBar_01/BarV1_ProgressBarBorder.png");

            Sprite barV2Bg = LoadUiSprite("ProgressBar_02/BarV2_Bar.png") ?? squareSprite;
            Sprite barV2Fill = LoadUiSprite("ProgressBar_02/BarV2_ProgressBar.png") ?? squareSprite;
            Sprite barV2Border = LoadUiSprite("ProgressBar_02/BarV2_ProgressBarBorder.png");

            Sprite barV3Bg = LoadUiSprite("ProgressBar_03/BarV3_Bar.png") ?? squareSprite;
            Sprite barV3Fill = LoadUiSprite("ProgressBar_03/BarV3_ProgressBar.png") ?? squareSprite;
            Sprite barV3Border = LoadUiSprite("ProgressBar_03/BarV3_ProgressBarBorder.png");

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // =================================================================================
            // 1. ESQUINA SUPERIOR DERECHA: BARRA DE VIDA DEL JUGADOR + INDICADOR DE CHECKPOINT
            // =================================================================================
            GameObject healthPanel = new GameObject("Player_Health_Panel");
            healthPanel.transform.SetParent(hudCanvas.transform, false);
            RectTransform rtHealth = healthPanel.AddComponent<RectTransform>();
            rtHealth.anchorMin = new Vector2(1f, 1f);
            rtHealth.anchorMax = new Vector2(1f, 1f);
            rtHealth.pivot = new Vector2(1f, 1f);
            rtHealth.anchoredPosition = new Vector2(-24f, -20f);
            rtHealth.sizeDelta = new Vector2(330f, 64f);

            Image imgFillPlayer = CreateSciFiBarRow(healthPanel.transform, "Health_Bar", barV1Bg, barV1Fill, barV1Border,
                new Vector2(0f, 0.38f), new Vector2(1f, 0.98f), new Color(0.15f, 0.95f, 0.50f, 1f), 1.0f);

            Text txtPlayerHealth = CreateUIText(healthPanel.transform, "Player_Health_Text", "VIDA: 100 / 100", defaultFont, 15,
                new Vector2(0.04f, 0.40f), new Vector2(0.96f, 0.96f));

            Text txtCheckpoint = CreateUIText(healthPanel.transform, "Checkpoint_Status_Text", "INICIO DEL NIVEL (CP: 0/2)", defaultFont, 12,
                new Vector2(0.02f, 0.0f), new Vector2(0.98f, 0.36f));
            txtCheckpoint.color = new Color(0.65f, 0.9f, 1.0f, 0.95f);

            // =================================================================================
            // 2. ESQUINA INFERIOR IZQUIERDA: BARRA DE DISPARO CARGADO + BARRA DE DASH DISPONIBLE
            // =================================================================================
            GameObject abilitiesPanel = new GameObject("Player_Abilities_Panel");
            abilitiesPanel.transform.SetParent(hudCanvas.transform, false);
            RectTransform rtAbilities = abilitiesPanel.AddComponent<RectTransform>();
            rtAbilities.anchorMin = new Vector2(0f, 0f);
            rtAbilities.anchorMax = new Vector2(0f, 0f);
            rtAbilities.pivot = new Vector2(0f, 0f);
            rtAbilities.anchoredPosition = new Vector2(24f, 20f);
            rtAbilities.sizeDelta = new Vector2(330f, 82f);

            // Fila Superior (Inferior Izquierda): Barra de Disparo Cargado PEM
            Image imgFillCharge = CreateSciFiBarRow(abilitiesPanel.transform, "Charge_Bar", barV2Bg, barV2Fill, barV2Border,
                new Vector2(0f, 0.52f), new Vector2(1f, 0.98f), new Color(0.1f, 0.85f, 1.0f, 1f), 0.0f);

            Text txtWeaponDmg = CreateUIText(abilitiesPanel.transform, "Weapon_Damage_Text", "CARGA PEM (10/30 DMG) | KATANA: 25", defaultFont, 12,
                new Vector2(0.03f, 0.54f), new Vector2(0.97f, 0.96f));

            // Fila Inferior (Inferior Izquierda): Indicador de Disponibilidad de Dash
            Image imgFillDash = CreateSciFiBarRow(abilitiesPanel.transform, "Dash_Bar", barV3Bg, barV3Fill, barV3Border,
                new Vector2(0f, 0.02f), new Vector2(1f, 0.46f), new Color(0.2f, 1.0f, 0.55f, 1f), 1.0f);

            Text txtDashStatus = CreateUIText(abilitiesPanel.transform, "Dash_Status_Text", "DASH [SHIFT / K]: DISPONIBLE", defaultFont, 12,
                new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.44f));

            // =================================================================================
            // 3. PARTE SUPERIOR CENTRAL: BARRA DE PULSERA DEL JEFE FINAL (SE ACTIVA EN COMBATE)
            // =================================================================================
            GameObject bossPanel = new GameObject("Boss_Health_Panel");
            bossPanel.transform.SetParent(hudCanvas.transform, false);
            RectTransform rtBoss = bossPanel.AddComponent<RectTransform>();
            rtBoss.anchorMin = new Vector2(0.5f, 1f);
            rtBoss.anchorMax = new Vector2(0.5f, 1f);
            rtBoss.pivot = new Vector2(0.5f, 1f);
            rtBoss.anchoredPosition = new Vector2(0f, -20f);
            rtBoss.sizeDelta = new Vector2(440f, 62f);

            Image imgFillBoss = CreateSciFiBarRow(bossPanel.transform, "Boss_Bar", barV1Bg, barV1Fill, barV1Border,
                new Vector2(0f, 0.05f), new Vector2(1f, 0.62f), new Color(1.0f, 0.20f, 0.32f, 1f), 1.0f);

            Text txtBossTitle = CreateUIText(bossPanel.transform, "Boss_Title_Text", "JEFE FINAL S.I.A. — ESPECTRO DIRECTIVO", defaultFont, 13,
                new Vector2(0.02f, 0.64f), new Vector2(0.98f, 0.98f));
            txtBossTitle.color = new Color(1.0f, 0.85f, 0.3f, 1f);

            Text txtBossHealth = CreateUIText(bossPanel.transform, "Boss_Health_Text", "PULSERA S.I.A.: 500 / 500", defaultFont, 13,
                new Vector2(0.02f, 0.08f), new Vector2(0.98f, 0.60f));

            // =================================================================================
            // 4. VINCULAR CONTROLADOR DE HUD Y SISTEMA COMPLETO DE AUDIO (BGM + 12 SFX)
            // =================================================================================
            HUDController hud = hudCanvas.AddComponent<HUDController>();
            SerializedObject soHud = new SerializedObject(hud);
            soHud.FindProperty("playerHealthFill").objectReferenceValue = imgFillPlayer;
            soHud.FindProperty("playerHealthText").objectReferenceValue = txtPlayerHealth;
            soHud.FindProperty("checkpointStatusText").objectReferenceValue = txtCheckpoint;
            soHud.FindProperty("playerChargeFill").objectReferenceValue = imgFillCharge;
            soHud.FindProperty("weaponDamageText").objectReferenceValue = txtWeaponDmg;
            soHud.FindProperty("playerDashFill").objectReferenceValue = imgFillDash;
            soHud.FindProperty("dashStatusText").objectReferenceValue = txtDashStatus;
            soHud.FindProperty("bossHealthContainer").objectReferenceValue = bossPanel;
            soHud.FindProperty("bossHealthFill").objectReferenceValue = imgFillBoss;
            soHud.FindProperty("bossTitleText").objectReferenceValue = txtBossTitle;
            soHud.FindProperty("bossHealthText").objectReferenceValue = txtBossHealth;

            // Cargar Clips de Música de Acción/Suspenso y Efectos de Sonido
            string cyberAudioPack = "Assets/_Project/Audio/Cyberpunk_Music_And_SFX/craftpix-net-648698-free-music-and-sounds-for-cyberpunk-game-pack-1";
            AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/Music/WAV/Track race.wav");
            AudioClip sfxShoot = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/FX/Blaster laser.wav");
            AudioClip sfxCharged = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/FX/Thunderbolt.wav");
            AudioClip sfxChargeReady = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Charge_Ready.wav");
            AudioClip sfxKatana = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Katana_Slash.wav");
            AudioClip sfxJump = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Lost Crypt/Audio/SFX/Jump/SFX_Jump.wav");
            AudioClip sfxDash = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Dash_Slide.wav");
            AudioClip sfxPlayerHurt = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Player_Hurt.wav");
            AudioClip sfxPlayerDead = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/FX/Dead1.wav");
            AudioClip sfxEnemyHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Enemy_Hit.wav");
            AudioClip sfxEnemyDead = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/FX/Explosion.wav");
            AudioClip sfxBossCast = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/SFX_Boss_Cast.wav");
            AudioClip sfxCheckpoint = AssetDatabase.LoadAssetAtPath<AudioClip>($"{cyberAudioPack}/FX/Access granted.wav");

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
            if (sfxCheckpoint != null) soHud.FindProperty("sfxCheckpoint").objectReferenceValue = sfxCheckpoint;

            soHud.ApplyModifiedProperties();
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
            imgBg.color = new Color(0.08f, 0.12f, 0.20f, 0.92f);
            imgBg.raycastTarget = false;

            GameObject fillObj = new GameObject("Bar_Fill");
            fillObj.transform.SetParent(container.transform, false);
            RectTransform rtFill = fillObj.AddComponent<RectTransform>();
            rtFill.anchorMin = new Vector2(0.02f, 0.12f);
            rtFill.anchorMax = new Vector2(0.98f, 0.88f);
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
                imgBorder.color = new Color(0.75f, 0.92f, 1.0f, 0.95f);
                imgBorder.raycastTarget = false;
            }

            return imgFill;
        }

        private static Text CreateUIText(Transform parent, string name, string content, Font font, int fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject txtObj = new GameObject(name);
            txtObj.transform.SetParent(parent, false);
            RectTransform rt = txtObj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Text txt = txtObj.AddComponent<Text>();
            if (font != null) txt.font = font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = false;
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

            temp.AddComponent<SmallEnemyAI>();

            // Barra de vida flotante sobre la cabeza del Robot
            GameObject healthBarObj = new GameObject("FloatingHealthBar");
            healthBarObj.transform.SetParent(temp.transform, false);
            healthBarObj.transform.localPosition = new Vector3(0f, 0.95f, 0f);

            GameObject barBg = new GameObject("Bar_BG");
            barBg.transform.SetParent(healthBarObj.transform, false);
            SpriteRenderer srBg = barBg.AddComponent<SpriteRenderer>();
            srBg.sprite = GetOrCreateSquareSprite();
            srBg.color = new Color(0.08f, 0.10f, 0.16f, 0.95f);
            srBg.sortingOrder = 10;
            barBg.transform.localScale = new Vector3(1.2f, 0.2f, 1f);

            GameObject barFill = new GameObject("Bar_Fill");
            barFill.transform.SetParent(healthBarObj.transform, false);
            SpriteRenderer srFill = barFill.AddComponent<SpriteRenderer>();
            srFill.sprite = GetOrCreateSquareSprite();
            srFill.color = new Color(0.15f, 0.95f, 0.35f, 1f);
            srFill.sortingOrder = 11;
            barFill.transform.localScale = new Vector3(1.15f, 0.16f, 1f);

            FloatingHealthBar floatingBar = healthBarObj.AddComponent<FloatingHealthBar>();
            SerializedObject soBar = new SerializedObject(floatingBar);
            soBar.FindProperty("fillTransform").objectReferenceValue = barFill.transform;
            soBar.FindProperty("offset").vector3Value = new Vector3(0f, 0.95f, 0f);
            soBar.ApplyModifiedProperties();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
            Object.DestroyImmediate(temp);
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
            Object.DestroyImmediate(tempImpact);
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

            EMPPulse pulse = tempPulse.AddComponent<EMPPulse>();
            SerializedObject soPulse = new SerializedObject(pulse);
            soPulse.FindProperty("impactVfxPrefab").objectReferenceValue = impactVfx;
            soPulse.ApplyModifiedProperties();

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempPulse, prefabPath);
            Object.DestroyImmediate(tempPulse);
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
