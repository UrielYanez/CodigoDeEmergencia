using UnityEngine;

namespace CodigoDeEmergencia.Core
{
    /// <summary>
    /// Estructura que encapsula todos los datos relevantes de un impacto o ataque.
    /// Permite diferenciar daño físico de daño electromagnético (PEM).
    /// </summary>
    [System.Serializable]
    public struct DamageInfo
    {
        public int amount;
        public GameObject damageSource;
        public Vector2 hitPoint;
        public Vector2 hitNormal;
        public bool isElectromagnetic;

        public DamageInfo(int amount, GameObject damageSource, Vector2 hitPoint = default, Vector2 hitNormal = default, bool isElectromagnetic = false)
        {
            this.amount = amount;
            this.damageSource = damageSource;
            this.hitPoint = hitPoint;
            this.hitNormal = hitNormal;
            this.isElectromagnetic = isElectromagnetic;
        }
    }
}
