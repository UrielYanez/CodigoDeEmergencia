using System;
using UnityEngine;

namespace CodigoDeEmergencia.Core
{
    /// <summary>
    /// Estados posibles para cada nivel según la rúbrica oficial:
    /// - Jugar: en curso con mecánicas y HUD activo.
    /// - Pausado: congela el juego y ofrece reanudar, reiniciar, opciones, salir.
    /// - Victoria (Fin del Nivel): pantalla de éxito con puntaje, tiempo y siguiente nivel.
    /// - Derrota (Game Over): pantalla de derrota con opción de reintentar o volver al menú.
    /// </summary>
    public enum LevelState
    {
        Playing,
        Paused,
        Victory,
        GameOver
    }

    /// <summary>
    /// Identificadores de efectos de sonido del juego para disparo, impacto, katana, daño y habilidades.
    /// </summary>
    public enum SfxType
    {
        ShootNormal,
        ShootCharged,
        ChargeReady,
        MeleeSlash,
        Jump,
        Dash,
        PlayerHurt,
        PlayerDead,
        EnemyHit,
        EnemyDestroyed,
        BossCast,
        Checkpoint
    }

    /// <summary>
    /// Bus de eventos global para comunicación desacoplada entre sistemas.
    /// Evita que el combate, el HUD, el audio y los managers dependan fuertemente entre sí.
    /// </summary>
    public static class GameEvents
    {
        // Eventos del Jugador
        public static event Action<int, int> OnPlayerHealthChanged; // (current, max)
        public static event Action OnPlayerDied;
        public static event Action<int> OnWeaponDamageUpgraded; // (nuevo daño)
        public static event Action<int> OnCheckpointActivated; // (número de checkpoint)

        // Eventos de Pulseras y Docentes
        public static event Action<int, int, Transform> OnBraceletDamaged; // (current, max, teacherTransform)
        public static event Action<Transform> OnBraceletBroken; // (teacherTransform)

        // Eventos de Estado del Nivel y Audio
        public static event Action<LevelState> OnLevelStateChanged;
        public static event Action<SfxType> OnPlaySfx;

        // Disparadores de eventos
        public static void TriggerPlayerHealthChanged(int current, int max) => OnPlayerHealthChanged?.Invoke(current, max);
        public static void TriggerPlayerDied() => OnPlayerDied?.Invoke();
        public static void TriggerWeaponDamageUpgraded(int newDamage) => OnWeaponDamageUpgraded?.Invoke(newDamage);
        public static void TriggerCheckpointActivated(int checkpointIndex) => OnCheckpointActivated?.Invoke(checkpointIndex);

        public static void TriggerBraceletDamaged(int current, int max, Transform teacher) => OnBraceletDamaged?.Invoke(current, max, teacher);
        public static void TriggerBraceletBroken(Transform teacher) => OnBraceletBroken?.Invoke(teacher);

        public static void TriggerLevelStateChanged(LevelState state) => OnLevelStateChanged?.Invoke(state);
        public static void PlaySfx(SfxType sfx) => OnPlaySfx?.Invoke(sfx);
    }
}
