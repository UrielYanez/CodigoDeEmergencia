using UnityEngine;
using CodigoDeEmergencia.Core;

namespace CodigoDeEmergencia.Player
{
    /// <summary>
    /// Coordinador central del jugador.
    /// Centraliza los componentes de movimiento, esquive, salud y disparo,
    /// actualiza los parámetros del Animator Controller y despliega visualmente
    /// el paracaídas energético durante el planeo (IsGliding).
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerDash))]
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerShooting))]
    public class PlayerController : MonoBehaviour
    {
        [Header("--- Referencias Visuales ---")]
        [Tooltip("Animator Controller del personaje para sincronizar estados.")]
        [SerializeField] private Animator animator;

        [Tooltip("SpriteRenderer principal del personaje.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("--- Selector de Personajes Jugables (4 Opciones) ---")]
        [Tooltip("Controladores de animación de los 4 personajes seleccionables (0: Kunoichi, 1: Shinobi, 2: Androide PEM, 3: Guardián Wraith-02).")]
        [SerializeField] private RuntimeAnimatorController[] playableCharacterControllers;

        [Tooltip("Sprites iniciales (Idle) de los 4 personajes seleccionables.")]
        [SerializeField] private Sprite[] playableCharacterIdleSprites;

        // Subcomponentes del jugador
        private PlayerMovement movement;
        private PlayerDash dash;
        private PlayerHealth health;
        private PlayerShooting shooting;

        // Hashes de parámetros del Animator para alto rendimiento
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");
        private static readonly int IsGlidingHash = Animator.StringToHash("IsGliding");
        private static readonly int IsClimbingHash = Animator.StringToHash("IsClimbing");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");
        private static readonly int DieTriggerHash = Animator.StringToHash("Die");
        private static readonly int ShootTriggerHash = Animator.StringToHash("Shoot");
        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

        public PlayerMovement Movement => movement;
        public PlayerDash Dash => dash;
        public PlayerHealth Health => health;
        public PlayerShooting Shooting => shooting;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            dash = GetComponent<PlayerDash>();
            health = GetComponent<PlayerHealth>();
            shooting = GetComponent<PlayerShooting>();

            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Eliminar cualquier visual residual de paracaídas externo (las animaciones del personaje ya incluyen Glide)
            Transform oldParachute = transform.Find("Glide_Parachute_Visual");
            if (oldParachute != null)
            {
                Destroy(oldParachute.gameObject);
            }
        }

        private void Start()
        {
            int selectedChar = PlayerPrefs.GetInt("CDE_SelectedCharacterIndex", 0);
            ApplySelectedCharacter(selectedChar);
        }

        /// <summary>
        /// Aplica el Animator Controller y Sprite correspondientes al personaje elegido por el jugador.
        /// </summary>
        public void ApplySelectedCharacter(int characterIndex)
        {
            if (playableCharacterControllers != null && playableCharacterControllers.Length > 0)
            {
                int safeIndex = Mathf.Clamp(characterIndex, 0, playableCharacterControllers.Length - 1);
                if (animator != null && playableCharacterControllers[safeIndex] != null)
                {
                    animator.runtimeAnimatorController = playableCharacterControllers[safeIndex];
                }
            }

            if (playableCharacterIdleSprites != null && playableCharacterIdleSprites.Length > 0)
            {
                int safeSpriteIndex = Mathf.Clamp(characterIndex, 0, playableCharacterIdleSprites.Length - 1);
                if (spriteRenderer != null && playableCharacterIdleSprites[safeSpriteIndex] != null)
                {
                    spriteRenderer.sprite = playableCharacterIdleSprites[safeSpriteIndex];
                }
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDamaged += HandleDamaged;
                health.OnDied += HandleDied;
            }

            if (dash != null)
            {
                dash.OnDashStarted += HandleDashStarted;
            }

            if (shooting != null)
            {
                shooting.OnShotFired += HandleShotFired;
                shooting.OnMeleeAttackExecuted += HandleMeleeAttack;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDamaged -= HandleDamaged;
                health.OnDied -= HandleDied;
            }

            if (dash != null)
            {
                dash.OnDashStarted -= HandleDashStarted;
            }

            if (shooting != null)
            {
                shooting.OnShotFired -= HandleShotFired;
                shooting.OnMeleeAttackExecuted -= HandleMeleeAttack;
            }
        }

        private void Update()
        {
            UpdateAnimationParameters();
        }

        private void UpdateAnimationParameters()
        {
            if (animator == null) return;

            float horizontalSpeed = Mathf.Abs(movement.Velocity.x);
            if (HasParameter(SpeedHash)) animator.SetFloat(SpeedHash, horizontalSpeed);
            if (HasParameter(IsGroundedHash)) animator.SetBool(IsGroundedHash, movement.IsGrounded);
            if (HasParameter(VerticalVelocityHash)) animator.SetFloat(VerticalVelocityHash, movement.Velocity.y);
            if (HasParameter(IsDashingHash)) animator.SetBool(IsDashingHash, dash.IsDashing);
            if (HasParameter(IsGlidingHash)) animator.SetBool(IsGlidingHash, movement.IsGliding);
            if (HasParameter(IsClimbingHash)) animator.SetBool(IsClimbingHash, movement.IsClimbing);
            if (HasParameter(IsDeadHash) && health != null) animator.SetBool(IsDeadHash, !health.IsAlive);

            if (movement.IsClimbing)
            {
                animator.speed = Mathf.Abs(movement.VerticalInput) > 0.1f ? 1f : 0f;
            }
            else if (animator.speed == 0f)
            {
                animator.speed = 1f;
            }
        }

        private void HandleDamaged()
        {
            if (animator != null && HasParameter(HurtTriggerHash))
            {
                animator.SetTrigger(HurtTriggerHash);
            }
        }

        private void HandleDied()
        {
            if (animator != null)
            {
                if (HasParameter(IsDeadHash)) animator.SetBool(IsDeadHash, true);
                if (HasParameter(DieTriggerHash)) animator.SetTrigger(DieTriggerHash);
            }
            movement.SetMovementEnabled(false);
            shooting.SetShootingEnabled(false);
        }

        private void HandleDashStarted()
        {
        }

        private void HandleShotFired()
        {
            if (movement != null && (movement.IsClimbing || movement.IsGliding)) return;
            if (animator != null && HasParameter(ShootTriggerHash))
            {
                animator.SetTrigger(ShootTriggerHash);
            }
        }

        private void HandleMeleeAttack()
        {
            if (movement != null && (movement.IsClimbing || movement.IsGliding)) return;
            if (animator != null && HasParameter(AttackTriggerHash))
            {
                animator.SetTrigger(AttackTriggerHash);
            }
        }

        private bool HasParameter(int paramHash)
        {
            if (animator == null) return false;
            foreach (var p in animator.parameters)
            {
                if (p.nameHash == paramHash) return true;
            }
            return false;
        }

        public void SetControlsActive(bool active)
        {
            movement.SetMovementEnabled(active);
            shooting.SetShootingEnabled(active);
        }
    }
}
