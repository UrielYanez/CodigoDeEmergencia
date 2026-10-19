namespace CodigoDeEmergencia.Core
{
    /// <summary>
    /// Interfaz universal para cualquier entidad o superficie que pueda recibir daño o pulsos PEM.
    /// Implementada tanto por el jugador como por las pulseras de los profesores.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Aplica daño al objeto según los parámetros especificados en DamageInfo.
        /// </summary>
        void TakeDamage(DamageInfo damageInfo);

        /// <summary>
        /// Indica si la entidad sigue viva o activa.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Salud o resistencia actual.
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// Salud o resistencia máxima.
        /// </summary>
        int MaxHealth { get; }
    }
}
