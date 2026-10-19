using UnityEngine;

namespace CodigoDeEmergencia.Core
{
    /// <summary>
    /// Destruye automáticamente el GameObject tras un tiempo determinado.
    /// Útil para efectos visuales (VFX) como chispas, fogonazos o impactos PEM.
    /// </summary>
    public class AutoDestroyAfterTime : MonoBehaviour
    {
        [Tooltip("Tiempo en segundos antes de autodestruirse.")]
        [SerializeField] private float lifetime = 0.5f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
