using System;
using UnityEngine;
using CodigoDeEmergencia.Core;

namespace CodigoDeEmergencia.Enemies
{
    /// <summary>
    /// Representa la resistencia de la Pulsera Inteligente SIA que controla al docente.
    /// Resuelve la mecánica de "Combate Pasivo": el profesor no es herido físicamente,
    /// sino que su pulsera se sobrecarga al recibir pulsos electromagnéticos (PEM).
    /// Vida: 500 HP (docentes básicos) hasta 5,000 HP (El Director).
    /// </summary>
    public class BraceletHealth : MonoBehaviour, IDamageable
    {
        [Header("--- Resistencia de la Pulsera SIA ---")]
        [Tooltip("Resistencia total de la pulsera (500 para docentes de nivel 1, hasta 5000 para el jefe).")]
        [SerializeField] private int maxHealth = 500;

        [Tooltip("Resistencia actual.")]
        [SerializeField] private int currentHealth = 500;

        [Header("--- Efectos Visuales y Sonoros de Sobrecarga ---")]
        [Tooltip("Efecto de chispas o partículas al sobrecargar la pulsera.")]
        [SerializeField] private GameObject overloadVfxPrefab;

        // Estado
        private bool isBroken;

        // Propiedades públicas (IDamageable)
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => !isBroken;
        public bool IsBroken => isBroken;

        // Eventos locales
        public event Action<int, int> OnHealthChanged;
        public event Action OnOverloaded;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (isBroken) return;

            // La pulsera SOLO reacciona ante pulsos electromagnéticos (combate pasivo)
            if (!damageInfo.isElectromagnetic) return;

            currentHealth = Mathf.Max(0, currentHealth - damageInfo.amount);
            Debug.Log($"<color=cyan><b>[PULSERA S.I.A.]</b> Impacto PEM recibido (-{damageInfo.amount}). Resistencia restante: {currentHealth}/{maxHealth}</color>");
            NotifyHealth();

            if (currentHealth <= 0)
            {
                BreakBracelet();
            }
            else
            {
                GameEvents.PlaySfx(SfxType.EnemyHit);
            }
        }

        private void NotifyHealth()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            GameEvents.TriggerBraceletDamaged(currentHealth, maxHealth, transform);
        }

        private void BreakBracelet()
        {
            if (isBroken) return;

            isBroken = true;
            GameEvents.PlaySfx(SfxType.EnemyDestroyed);
            Debug.Log("<color=green><b>[DOCENTE LIBERADO]</b> ¡Pulsera S.I.A. sobrecargada! El profesor ha salido del trance.</color>");

            if (overloadVfxPrefab != null)
            {
                Instantiate(overloadVfxPrefab, transform.position, Quaternion.identity);
            }

            OnOverloaded?.Invoke();
            GameEvents.TriggerBraceletBroken(transform);
        }

        /// <summary>
        /// Configura dinámicamente la resistencia de la pulsera (útil para crear variantes de profesores).
        /// </summary>
        public void ConfigureHealth(int maxHP)
        {
            maxHealth = maxHP;
            currentHealth = maxHP;
            isBroken = false;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
