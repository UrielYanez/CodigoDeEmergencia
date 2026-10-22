using UnityEngine;

namespace CodigoDeEmergencia.Core
{
    /// <summary>
    /// Sistema de fondo panorámico con efecto Parallax infinito y sin cortes visibles.
    /// Crea un efecto de profundidad al desplazarse a menor velocidad que la cámara,
    /// repitiendo los paneles horizontales de forma fluida a lo largo de todo el mapa extenso.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("--- Configuración de Parallax ---")]
        [Tooltip("Multiplicador de desplazamiento horizontal (0 = estático, 0.5 = profundidad media, 1 = se mueve con la cámara).")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxEffectX = 0.45f;

        [Tooltip("Transform de la cámara principal.")]
        [SerializeField] private Transform cameraTransform;

        [Header("--- Paneles Repetitivos ---")]
        [Tooltip("Ancho en unidades de Unity de cada panel de fondo.")]
        [SerializeField] private float panelWidth = 35.5f;

        [Tooltip("Paneles hijos para rotación infinita.")]
        [SerializeField] private Transform[] panels;

        private Vector3 lastCameraPosition;

        private void Start()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (cameraTransform != null)
            {
                lastCameraPosition = cameraTransform.position;
            }
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

            // Desplazar el fondo a menor velocidad para simular lejanía
            transform.position += new Vector3(deltaMovement.x * (1f - parallaxEffectX), 0f, 0f);
            lastCameraPosition = cameraTransform.position;

            // Si los paneles quedan atrás, reacomodarlos hacia el frente del avance
            if (panels != null && panels.Length > 1)
            {
                foreach (Transform panel in panels)
                {
                    if (cameraTransform.position.x - panel.position.x > panelWidth * 1.5f)
                    {
                        panel.position += new Vector3(panelWidth * panels.Length, 0f, 0f);
                    }
                    else if (panel.position.x - cameraTransform.position.x > panelWidth * 1.5f)
                    {
                        panel.position -= new Vector3(panelWidth * panels.Length, 0f, 0f);
                    }
                }
            }
        }

        public void Initialize(Transform cam, float width, Transform[] panelArray)
        {
            cameraTransform = cam;
            panelWidth = width;
            panels = panelArray;
            if (cam != null) lastCameraPosition = cam.position;
        }
    }
}
