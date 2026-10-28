#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace CodigoDeEmergencia.EditorTools
{
    /// <summary>
    /// Configurador automático de Sprites HD y Animaciones para Ninja_Female (10 animaciones completas),
    /// Robots S.I.A. y efectos eléctricos PEM.
    /// </summary>
    public static class BatchSpriteSheetProcessor
    {
        private const string NinjaFemaleFolder = "Assets/_Project/Sprites/Player/Ninja_Female/png";
        private const string RobotFolder = "Assets/_Project/Sprites/Enemies/Robots_SIA/png";
        private const string WraithBossFolder = "Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_03/PNG Sequences";
        private const string WraithPatrolFolder = "Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_01/PNG Sequences";
        private const string LightningTravelFolder = "Assets/_Project/Sprites/VFX/EMP_Lightning_Skills/VFX1/Frames";
        private const string LightningImpactFolder = "Assets/_Project/Sprites/VFX/EMP_Lightning_Skills/VFX6/Frames";
        private const string PlayerAnimFolder = "Assets/_Project/Animations/Player";
        private const string EnemyAnimFolder = "Assets/_Project/Animations/Enemies";
        private const string VfxAnimFolder = "Assets/_Project/Animations/VFX";
        private const string AutoFixSessionKey = "CDE_NinjaFemaleAndTilesConfigured_v2";

        [InitializeOnLoadMethod]
        private static void AutoConfigureOnLoadOnce()
        {
            if (SessionState.GetBool(AutoFixSessionKey, false)) return;
            SessionState.SetBool(AutoFixSessionKey, true);

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                ProcessAllSpriteSheetsSilent();
            };
        }

        [MenuItem("Tools/Código de Emergencia/✂️ Procesar Todos los Spritesheets y Crear Animaciones")]
        public static void ProcessAllSpriteSheets()
        {
            ProcessAllSpriteSheetsSilent();

            EditorUtility.DisplayDialog(
                "¡Animaciones de Ninja Mujer Configuradas!",
                "Se configuraron todas las animaciones HD de Ninja_Female:\n\n" +
                "• Idle, Run, Jump, Slide (Dash)\n" +
                "• Throw (Disparo PEM terrestre) y Jump_Throw (Disparo aéreo)\n" +
                "• Attack (Katana cuerpo a cuerpo) y Jump_Attack (Katana aérea)\n" +
                "• Glide (Planeo en caída) y Climb (Subir escaleras)\n" +
                "• Dead (Derrota)\n" +
                "• Efectos eléctricos PEM y Robots S.I.A.\n\n" +
                "Ahora ejecuta '🚀 Montar Escena Prototipo (Avance 1)' para actualizar el mapa con el Tileset Industrial.",
                "¡Excelente!"
            );
        }

        public static void ProcessAllSpriteSheetsSilent()
        {
            EnsureFolders();

            Vector2 feetPivot = new Vector2(0.5f, 0.02f);

            // 1. Configurar y crear los 11 Clips de Ninja_Female (todas las animaciones incluidas en el paquete)
            BuildClipFromPrefix(NinjaFemaleFolder, "Idle__", 10, 245f, feetPivot, 12f, true, PlayerAnimFolder, "Player_Idle");
            BuildClipFromPrefix(NinjaFemaleFolder, "Run__", 10, 245f, feetPivot, 15f, true, PlayerAnimFolder, "Player_Run");
            BuildClipFromPrefix(NinjaFemaleFolder, "Jump__", 10, 245f, feetPivot, 12f, false, PlayerAnimFolder, "Player_Jump");
            BuildClipFromPrefix(NinjaFemaleFolder, "Slide__", 10, 245f, feetPivot, 18f, false, PlayerAnimFolder, "Player_Dash");
            BuildClipFromPrefix(NinjaFemaleFolder, "Throw__", 10, 245f, feetPivot, 22f, false, PlayerAnimFolder, "Player_Throw");
            BuildClipFromPrefix(NinjaFemaleFolder, "Jump_Throw__", 10, 245f, feetPivot, 22f, false, PlayerAnimFolder, "Player_JumpThrow");
            BuildClipFromPrefix(NinjaFemaleFolder, "Attack__", 10, 245f, feetPivot, 22f, false, PlayerAnimFolder, "Player_Attack");
            BuildClipFromPrefix(NinjaFemaleFolder, "Jump_Attack__", 10, 245f, feetPivot, 22f, false, PlayerAnimFolder, "Player_JumpAttack");
            BuildClipFromPrefix(NinjaFemaleFolder, "Glide_", 10, 245f, feetPivot, 14f, true, PlayerAnimFolder, "Player_Glide");
            BuildClipFromPrefix(NinjaFemaleFolder, "Climb_", 10, 245f, feetPivot, 14f, true, PlayerAnimFolder, "Player_Climb");
            BuildClipFromPrefix(NinjaFemaleFolder, "Dead__", 10, 245f, feetPivot, 12f, false, PlayerAnimFolder, "Player_Dead");

            // 2. Configurar VFX de Rayos PEM (Viaje e Impacto)
            Vector2 centerPivot = new Vector2(0.5f, 0.5f);
            BuildClipFromNumberedSequence(LightningTravelFolder, "lightning_skill1_frame", 1, 4, 120f, centerPivot, 16f, true, VfxAnimFolder, "Pulse_Travel", FilterMode.Point);
            BuildClipFromNumberedSequence(LightningImpactFolder, "lightning_skill6_frame", 1, 8, 120f, centerPivot, 20f, false, VfxAnimFolder, "Pulse_Impact", FilterMode.Point);

            // 3. Configurar Animación de Robots S.I.A. para Mini-Enemigos
            BuildRobotClip(RobotFolder, "Run", 8, 340f, new Vector2(0.5f, 0.5f), 14f, true, EnemyAnimFolder, "Robot_Run");

            // 4. Configurar Animaciones del Jefe Final (Wraith_03 a escala 3x del jugador -> PPU = 70 = 6.0m de altura)
            Vector2 wraithPivot = new Vector2(0.5f, 0.08f);
            BuildClipFromPrefix($"{WraithBossFolder}/Idle", "Wraith_03_Idle_", 12, 70f, wraithPivot, 12f, true, EnemyAnimFolder, "Boss_Idle");
            BuildClipFromPrefix($"{WraithBossFolder}/Moving Forward", "Wraith_03_Moving Forward_", 12, 70f, wraithPivot, 14f, true, EnemyAnimFolder, "Boss_Walk");
            BuildClipFromPrefix($"{WraithBossFolder}/Casting Spells", "Wraith_03_Casting Spells_", 18, 70f, wraithPivot, 18f, false, EnemyAnimFolder, "Boss_Cast");
            BuildClipFromPrefix($"{WraithBossFolder}/Hurt", "Wraith_03_Hurt_", 12, 70f, wraithPivot, 16f, false, EnemyAnimFolder, "Boss_Hurt");
            BuildClipFromPrefix($"{WraithBossFolder}/Dying", "Wraith_03_Dying_", 15, 70f, wraithPivot, 14f, false, EnemyAnimFolder, "Boss_Dead");

            // Configurar también el proyectil del Jefe (Spells-Effect.png)
            ConfigureSingleSprite("Assets/_Project/Sprites/Enemies/Digital_Wraiths/PNG/Wraith_03/Vector Parts/Spells-Effect.png", 95f, centerPivot, FilterMode.Bilinear);

            // 5. Configurar Animaciones de Patrulla (Wraith_01 tamaño normal -> PPU = 185 = 2.25m de altura)
            BuildClipFromPrefix($"{WraithPatrolFolder}/Idle", "Wraith_01_Idle_", 12, 185f, wraithPivot, 12f, true, EnemyAnimFolder, "Patrol_Idle");
            BuildClipFromPrefix($"{WraithPatrolFolder}/Walking", "Wraith_01_Moving Forward_", 12, 185f, wraithPivot, 14f, true, EnemyAnimFolder, "Patrol_Walk");

            // 6. Actualizar Controladores de Animación
            CreateOrUpdatePlayerAnimatorController();
            CreateOrUpdateVFXControllers();
            CreateOrUpdateRobotController();
            CreateOrUpdateBossAndPatrolControllers();

            AssetDatabase.SaveAssets();
        }

        public static void RealignExistingProcessedCharacterPivots()
        {
            // Mantenido por compatibilidad con llamadas del Wizard
        }

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets/_Project", "Animations");
            CreateFolderIfMissing("Assets/_Project/Animations", "Player");
            CreateFolderIfMissing("Assets/_Project/Animations", "Enemies");
            CreateFolderIfMissing("Assets/_Project/Animations", "VFX");
            CreateFolderIfMissing("Assets/_Project", "Prefabs");
            CreateFolderIfMissing("Assets/_Project/Prefabs", "VFX");
            CreateFolderIfMissing("Assets/_Project/Prefabs", "Enemies");
        }

        private static void CreateFolderIfMissing(string parent, string newFolder)
        {
            string full = $"{parent}/{newFolder}";
            if (!AssetDatabase.IsValidFolder(full))
            {
                AssetDatabase.CreateFolder(parent, newFolder);
            }
        }

        private static void BuildClipFromPrefix(string folder, string prefix, int frameCount, float ppu, Vector2 pivot, float fps, bool loop, string animFolder, string clipName)
        {
            List<Sprite> frames = new List<Sprite>();
            for (int i = 0; i < frameCount; i++)
            {
                string path = $"{folder}/{prefix}{i:D3}.png";
                Sprite sp = ConfigureSingleSprite(path, ppu, pivot, FilterMode.Bilinear);
                if (sp != null) frames.Add(sp);
            }

            if (frames.Count > 0)
            {
                SaveSpriteAnimationClip(frames, $"{animFolder}/{clipName}.anim", fps, loop);
            }
        }

        private static void BuildClipFromNumberedSequence(string folder, string prefix, int startIdx, int endIdx, float ppu, Vector2 pivot, float fps, bool loop, string animFolder, string clipName, FilterMode filter)
        {
            List<Sprite> frames = new List<Sprite>();
            for (int i = startIdx; i <= endIdx; i++)
            {
                string path = $"{folder}/{prefix}{i}.png";
                Sprite sp = ConfigureSingleSprite(path, ppu, pivot, filter);
                if (sp != null) frames.Add(sp);
            }

            if (frames.Count > 0)
            {
                SaveSpriteAnimationClip(frames, $"{animFolder}/{clipName}.anim", fps, loop);
            }
        }

        private static void BuildRobotClip(string folder, string actionName, int count, float ppu, Vector2 pivot, float fps, bool loop, string animFolder, string clipName)
        {
            List<Sprite> frames = new List<Sprite>();
            for (int i = 1; i <= count; i++)
            {
                string path = $"{folder}/{actionName} ({i}).png";
                Sprite sp = ConfigureSingleSprite(path, ppu, pivot, FilterMode.Bilinear);
                if (sp != null) frames.Add(sp);
            }

            if (frames.Count > 0)
            {
                SaveSpriteAnimationClip(frames, $"{animFolder}/{clipName}.anim", fps, loop);
            }
        }

        public static Sprite ConfigureSingleSprite(string assetPath, float pixelsPerUnit, Vector2 pivot, FilterMode filterMode)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return null;

            bool needsReimport = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                needsReimport = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                needsReimport = true;
            }
            if (Mathf.Abs(importer.spritePixelsPerUnit - pixelsPerUnit) > 0.5f)
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                needsReimport = true;
            }
            if (importer.filterMode != filterMode)
            {
                importer.filterMode = filterMode;
                needsReimport = true;
            }

            TextureImporterSettings texSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(texSettings);
            if (texSettings.spriteAlignment != (int)SpriteAlignment.Custom || Vector2.Distance(texSettings.spritePivot, pivot) > 0.005f)
            {
                texSettings.spriteAlignment = (int)SpriteAlignment.Custom;
                texSettings.spritePivot = pivot;
                importer.SetTextureSettings(texSettings);
                needsReimport = true;
            }

            if (needsReimport)
            {
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static void SaveSpriteAnimationClip(List<Sprite> sprites, string clipPath, float frameRate, bool loop)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = frameRate;

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
        }

        public static void CreateOrUpdatePlayerAnimatorController()
        {
            EnsureFolders();

            string controllerPath = $"{PlayerAnimFolder}/PlayerAnimatorController.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            // Parámetros del Animator
            AddParameterIfMissing(controller, "Speed", AnimatorControllerParameterType.Float);
            AddParameterIfMissing(controller, "IsGrounded", AnimatorControllerParameterType.Bool);
            AddParameterIfMissing(controller, "IsDashing", AnimatorControllerParameterType.Bool);
            AddParameterIfMissing(controller, "IsGliding", AnimatorControllerParameterType.Bool);
            AddParameterIfMissing(controller, "IsClimbing", AnimatorControllerParameterType.Bool);
            AddParameterIfMissing(controller, "IsDead", AnimatorControllerParameterType.Bool);
            AddParameterIfMissing(controller, "VerticalVelocity", AnimatorControllerParameterType.Float);
            AddParameterIfMissing(controller, "Hurt", AnimatorControllerParameterType.Trigger);
            AddParameterIfMissing(controller, "Shoot", AnimatorControllerParameterType.Trigger);
            AddParameterIfMissing(controller, "Attack", AnimatorControllerParameterType.Trigger);
            AddParameterIfMissing(controller, "Die", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            // Cargar los 11 Clips de Ninja_Female
            AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_Idle.anim");
            AnimationClip runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_Run.anim");
            AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_Jump.anim");
            AnimationClip dashClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_Dash.anim");
            AnimationClip throwClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_Throw.anim");
            AnimationClip jumpThrowClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_JumpThrow.anim");
            AnimationClip attackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_Attack.anim");
            AnimationClip jumpAttackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_JumpAttack.anim");
            AnimationClip glideClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_Glide.anim");
            AnimationClip climbClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_Climb.anim");
            AnimationClip deadClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{PlayerAnimFolder}/Player_Dead.anim");

            // Estados
            AnimatorState idleState = GetOrCreateState(sm, "Idle", idleClip);
            AnimatorState runState = GetOrCreateState(sm, "Run", runClip);
            AnimatorState jumpState = GetOrCreateState(sm, "Jump", jumpClip);
            AnimatorState dashState = GetOrCreateState(sm, "Dash", dashClip);
            AnimatorState throwState = GetOrCreateState(sm, "Throw", throwClip);
            AnimatorState jumpThrowState = GetOrCreateState(sm, "Jump_Throw", jumpThrowClip);
            AnimatorState attackState = GetOrCreateState(sm, "Attack", attackClip);
            AnimatorState jumpAttackState = GetOrCreateState(sm, "Jump_Attack", jumpAttackClip);
            AnimatorState glideState = GetOrCreateState(sm, "Glide", glideClip);
            AnimatorState climbState = GetOrCreateState(sm, "Climb", climbClip);
            AnimatorState deadState = GetOrCreateState(sm, "Dead", deadClip);

            sm.defaultState = idleState;

            // Limpiar todas las transiciones previas
            sm.anyStateTransitions = new AnimatorStateTransition[0];
            ClearTransitions(idleState);
            ClearTransitions(runState);
            ClearTransitions(jumpState);
            ClearTransitions(dashState);
            ClearTransitions(throwState);
            ClearTransitions(jumpThrowState);
            ClearTransitions(attackState);
            ClearTransitions(jumpAttackState);
            ClearTransitions(glideState);
            ClearTransitions(climbState);
            ClearTransitions(deadState);

            // 1. Idle <-> Run
            AnimatorStateTransition toRun = idleState.AddTransition(runState);
            toRun.hasExitTime = false;
            toRun.duration = 0.03f;
            toRun.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");

            AnimatorStateTransition toIdle = runState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.03f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");

            // 2. Idle / Run -> Jump (cuando deja el suelo sin estar planeando ni escalando)
            AnimatorStateTransition idleToJump = idleState.AddTransition(jumpState);
            idleToJump.hasExitTime = false;
            idleToJump.duration = 0.03f;
            idleToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            idleToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsClimbing");

            AnimatorStateTransition runToJump = runState.AddTransition(jumpState);
            runToJump.hasExitTime = false;
            runToJump.duration = 0.03f;
            runToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            runToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsClimbing");

            // Jump -> Glide
            AnimatorStateTransition jumpToGlide = jumpState.AddTransition(glideState);
            jumpToGlide.hasExitTime = false;
            jumpToGlide.duration = 0.04f;
            jumpToGlide.AddCondition(AnimatorConditionMode.If, 0f, "IsGliding");

            AnimatorStateTransition glideToJump = glideState.AddTransition(jumpState);
            glideToJump.hasExitTime = false;
            glideToJump.duration = 0.04f;
            glideToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGliding");
            glideToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");

            AnimatorStateTransition glideToIdle = glideState.AddTransition(idleState);
            glideToIdle.hasExitTime = false;
            glideToIdle.duration = 0.03f;
            glideToIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");

            // Jump -> Run / Idle al tocar suelo
            AnimatorStateTransition jumpToRun = jumpState.AddTransition(runState);
            jumpToRun.hasExitTime = false;
            jumpToRun.duration = 0.03f;
            jumpToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            jumpToRun.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");

            AnimatorStateTransition jumpToIdle = jumpState.AddTransition(idleState);
            jumpToIdle.hasExitTime = false;
            jumpToIdle.duration = 0.03f;
            jumpToIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.15f, "Speed");

            // 3. AnyState -> Climb (Escaleras)
            AnimatorStateTransition toClimb = sm.AddAnyStateTransition(climbState);
            toClimb.hasExitTime = false;
            toClimb.duration = 0.03f;
            toClimb.canTransitionToSelf = false;
            toClimb.AddCondition(AnimatorConditionMode.If, 0f, "IsClimbing");
            toClimb.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");

            AnimatorStateTransition climbToIdle = climbState.AddTransition(idleState);
            climbToIdle.hasExitTime = false;
            climbToIdle.duration = 0.03f;
            climbToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsClimbing");

            // 4. AnyState -> Dash (Slide)
            AnimatorStateTransition toDash = sm.AddAnyStateTransition(dashState);
            toDash.hasExitTime = false;
            toDash.duration = 0.02f;
            toDash.canTransitionToSelf = false;
            toDash.AddCondition(AnimatorConditionMode.If, 0f, "IsDashing");
            toDash.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");

            AnimatorStateTransition dashToRun = dashState.AddTransition(runState);
            dashToRun.hasExitTime = false;
            dashToRun.duration = 0.03f;
            dashToRun.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDashing");
            dashToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            dashToRun.AddCondition(AnimatorConditionMode.Greater, 0.15f, "Speed");

            AnimatorStateTransition dashToIdle = dashState.AddTransition(idleState);
            dashToIdle.hasExitTime = false;
            dashToIdle.duration = 0.03f;
            dashToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDashing");

            // 5. AnyState -> Throw (Disparo PEM en tierra) y Jump_Throw (Disparo en aire)
            AnimatorStateTransition toThrow = sm.AddAnyStateTransition(throwState);
            toThrow.hasExitTime = false;
            toThrow.duration = 0.02f;
            toThrow.canTransitionToSelf = false;
            toThrow.AddCondition(AnimatorConditionMode.If, 0f, "Shoot");
            toThrow.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            toThrow.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDashing");
            toThrow.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");

            AnimatorStateTransition throwToIdle = throwState.AddTransition(idleState);
            throwToIdle.hasExitTime = true;
            throwToIdle.exitTime = 0.85f;
            throwToIdle.duration = 0.04f;

            AnimatorStateTransition toJumpThrow = sm.AddAnyStateTransition(jumpThrowState);
            toJumpThrow.hasExitTime = false;
            toJumpThrow.duration = 0.02f;
            toJumpThrow.canTransitionToSelf = false;
            toJumpThrow.AddCondition(AnimatorConditionMode.If, 0f, "Shoot");
            toJumpThrow.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            toJumpThrow.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDashing");
            toJumpThrow.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");

            AnimatorStateTransition jumpThrowToJump = jumpThrowState.AddTransition(jumpState);
            jumpThrowToJump.hasExitTime = true;
            jumpThrowToJump.exitTime = 0.85f;
            jumpThrowToJump.duration = 0.04f;

            // 6. AnyState -> Attack (Katana en tierra) y Jump_Attack (Katana en aire)
            AnimatorStateTransition toAttack = sm.AddAnyStateTransition(attackState);
            toAttack.hasExitTime = false;
            toAttack.duration = 0.02f;
            toAttack.canTransitionToSelf = false;
            toAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            toAttack.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            toAttack.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDashing");
            toAttack.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");

            AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 0.88f;
            attackToIdle.duration = 0.04f;

            AnimatorStateTransition toJumpAttack = sm.AddAnyStateTransition(jumpAttackState);
            toJumpAttack.hasExitTime = false;
            toJumpAttack.duration = 0.02f;
            toJumpAttack.canTransitionToSelf = false;
            toJumpAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            toJumpAttack.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            toJumpAttack.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDashing");
            toJumpAttack.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");

            AnimatorStateTransition jumpAttackToJump = jumpAttackState.AddTransition(jumpState);
            jumpAttackToJump.hasExitTime = true;
            jumpAttackToJump.exitTime = 0.88f;
            jumpAttackToJump.duration = 0.04f;

            // 7. AnyState -> Dead
            AnimatorStateTransition toDead = sm.AddAnyStateTransition(deadState);
            toDead.hasExitTime = false;
            toDead.duration = 0.03f;
            toDead.canTransitionToSelf = false;
            toDead.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");

            AnimatorStateTransition deadToIdle = deadState.AddTransition(idleState);
            deadToIdle.hasExitTime = false;
            deadToIdle.duration = 0.04f;
            deadToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");

            EditorUtility.SetDirty(controller);
        }

        private static void CreateOrUpdateVFXControllers()
        {
            string pulseCtrlPath = $"{VfxAnimFolder}/PulseAnimatorController.controller";
            AnimatorController pulseCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(pulseCtrlPath);
            if (pulseCtrl == null) pulseCtrl = AnimatorController.CreateAnimatorControllerAtPath(pulseCtrlPath);

            AnimationClip travelClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{VfxAnimFolder}/Pulse_Travel.anim");
            if (travelClip != null)
            {
                AnimatorStateMachine sm = pulseCtrl.layers[0].stateMachine;
                AnimatorState travelState = GetOrCreateState(sm, "Pulse_Travel", travelClip);
                sm.defaultState = travelState;
            }

            string impactCtrlPath = $"{VfxAnimFolder}/ImpactAnimatorController.controller";
            AnimatorController impactCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(impactCtrlPath);
            if (impactCtrl == null) impactCtrl = AnimatorController.CreateAnimatorControllerAtPath(impactCtrlPath);

            AnimationClip impactClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{VfxAnimFolder}/Pulse_Impact.anim");
            if (impactClip != null)
            {
                AnimatorStateMachine sm = impactCtrl.layers[0].stateMachine;
                AnimatorState impactState = GetOrCreateState(sm, "Pulse_Impact", impactClip);
                sm.defaultState = impactState;
            }

            EditorUtility.SetDirty(pulseCtrl);
            EditorUtility.SetDirty(impactCtrl);
        }

        private static void CreateOrUpdateRobotController()
        {
            string robotCtrlPath = $"{EnemyAnimFolder}/RobotAnimatorController.controller";
            AnimatorController robotCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(robotCtrlPath);
            if (robotCtrl == null) robotCtrl = AnimatorController.CreateAnimatorControllerAtPath(robotCtrlPath);

            AnimationClip runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{EnemyAnimFolder}/Robot_Run.anim");
            if (runClip != null)
            {
                AnimatorStateMachine sm = robotCtrl.layers[0].stateMachine;
                AnimatorState runState = GetOrCreateState(sm, "Robot_Run", runClip);
                sm.defaultState = runState;
            }

            EditorUtility.SetDirty(robotCtrl);
        }

        private static void CreateOrUpdateBossAndPatrolControllers()
        {
            // 1. Controlador del Jefe Final (Wraith_03)
            string bossCtrlPath = $"{EnemyAnimFolder}/BossWraithAnimator.controller";
            AnimatorController bossCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(bossCtrlPath);
            if (bossCtrl == null) bossCtrl = AnimatorController.CreateAnimatorControllerAtPath(bossCtrlPath);

            AddParameterIfMissing(bossCtrl, "Speed", AnimatorControllerParameterType.Float);
            AddParameterIfMissing(bossCtrl, "Attack", AnimatorControllerParameterType.Trigger);
            AddParameterIfMissing(bossCtrl, "Hurt", AnimatorControllerParameterType.Trigger);
            AddParameterIfMissing(bossCtrl, "IsDead", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine smBoss = bossCtrl.layers[0].stateMachine;
            AnimationClip bossIdle = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{EnemyAnimFolder}/Boss_Idle.anim");
            AnimationClip bossWalk = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{EnemyAnimFolder}/Boss_Walk.anim");
            AnimationClip bossCast = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{EnemyAnimFolder}/Boss_Cast.anim");
            AnimationClip bossHurt = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{EnemyAnimFolder}/Boss_Hurt.anim");
            AnimationClip bossDead = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{EnemyAnimFolder}/Boss_Dead.anim");

            AnimatorState stIdle = GetOrCreateState(smBoss, "Idle", bossIdle);
            AnimatorState stWalk = GetOrCreateState(smBoss, "Walk", bossWalk);
            AnimatorState stCast = GetOrCreateState(smBoss, "Cast", bossCast);
            AnimatorState stHurt = GetOrCreateState(smBoss, "Hurt", bossHurt);
            AnimatorState stDead = GetOrCreateState(smBoss, "Dead", bossDead);
            smBoss.defaultState = stIdle;

            smBoss.anyStateTransitions = new AnimatorStateTransition[0];
            ClearTransitions(stIdle);
            ClearTransitions(stWalk);
            ClearTransitions(stCast);
            ClearTransitions(stHurt);
            ClearTransitions(stDead);

            AnimatorStateTransition bToWalk = stIdle.AddTransition(stWalk);
            bToWalk.hasExitTime = false;
            bToWalk.duration = 0.04f;
            bToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            AnimatorStateTransition bToIdle = stWalk.AddTransition(stIdle);
            bToIdle.hasExitTime = false;
            bToIdle.duration = 0.04f;
            bToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            AnimatorStateTransition bToCast = smBoss.AddAnyStateTransition(stCast);
            bToCast.hasExitTime = false;
            bToCast.duration = 0.03f;
            bToCast.canTransitionToSelf = false;
            bToCast.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            bToCast.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");

            AnimatorStateTransition castToIdle = stCast.AddTransition(stIdle);
            castToIdle.hasExitTime = true;
            castToIdle.exitTime = 0.88f;
            castToIdle.duration = 0.04f;

            AnimatorStateTransition bToHurt = smBoss.AddAnyStateTransition(stHurt);
            bToHurt.hasExitTime = false;
            bToHurt.duration = 0.02f;
            bToHurt.canTransitionToSelf = false;
            bToHurt.AddCondition(AnimatorConditionMode.If, 0f, "Hurt");
            bToHurt.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");

            AnimatorStateTransition hurtToIdle = stHurt.AddTransition(stIdle);
            hurtToIdle.hasExitTime = true;
            hurtToIdle.exitTime = 0.85f;
            hurtToIdle.duration = 0.04f;

            AnimatorStateTransition bToDead = smBoss.AddAnyStateTransition(stDead);
            bToDead.hasExitTime = false;
            bToDead.duration = 0.04f;
            bToDead.canTransitionToSelf = false;
            bToDead.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");

            EditorUtility.SetDirty(bossCtrl);

            // 2. Controlador de Patrulla (Wraith_01)
            string patrolCtrlPath = $"{EnemyAnimFolder}/WraithPatrolAnimator.controller";
            AnimatorController patrolCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(patrolCtrlPath);
            if (patrolCtrl == null) patrolCtrl = AnimatorController.CreateAnimatorControllerAtPath(patrolCtrlPath);

            AnimationClip patrolWalk = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{EnemyAnimFolder}/Patrol_Walk.anim");
            if (patrolWalk != null)
            {
                AnimatorStateMachine smPatrol = patrolCtrl.layers[0].stateMachine;
                AnimatorState stPWalk = GetOrCreateState(smPatrol, "Patrol_Walk", patrolWalk);
                smPatrol.defaultState = stPWalk;
            }
            EditorUtility.SetDirty(patrolCtrl);
        }

        private static void AddParameterIfMissing(AnimatorController controller, string paramName, AnimatorControllerParameterType type)
        {
            foreach (var p in controller.parameters)
            {
                if (p.name == paramName) return;
            }
            controller.AddParameter(paramName, type);
        }

        private static AnimatorState GetOrCreateState(AnimatorStateMachine sm, string stateName, AnimationClip clip)
        {
            foreach (var childState in sm.states)
            {
                if (childState.state.name == stateName)
                {
                    childState.state.motion = clip;
                    return childState.state;
                }
            }

            AnimatorState newState = sm.AddState(stateName);
            newState.motion = clip;
            return newState;
        }

        private static void ClearTransitions(AnimatorState state)
        {
            state.transitions = new AnimatorStateTransition[0];
        }
    }
}
#endif
