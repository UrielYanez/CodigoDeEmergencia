using UnityEngine;

namespace CodigoDeEmergencia.Core
{
    /// <summary>
    /// Controlador de cámara 2D suave (SmoothFollow) tipo Metroidvania.
    /// Funciona nativamente sin requerir paquetes externos ni Cinemachine:
    /// - Seguimiento suave con Vector3.SmoothDamp.
    /// - Anticipación de movimiento (Lookahead).
    /// - Límites opcionales de pantalla (Room Bounds).
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("--- Objetivo ---")]
        [Tooltip("Transform del jugador u objetivo a seguir.")]
        [SerializeField] private Transform target;

        [Header("--- Suavizado y Offset ---")]
        [Tooltip("Tiempo de suavizado para alcanzar al objetivo.")]
        [SerializeField] private float smoothTime = 0.25f;

        [Tooltip("Desplazamiento de la cámara respecto al jugador.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);

        [Tooltip("Distancia de anticipación en la dirección a la que mira el jugador.")]
        [SerializeField] private float lookaheadDistance = 1.5f;

        [Header("--- Límites del Nivel (Opcional) ---")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
        [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

        private Vector3 currentVelocity;
        private float currentLookahead;

        private void Start()
        {
            if (target == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    target = playerObj.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Determinar orientación horizontal del objetivo
            float targetDirectionX = Mathf.Sign(target.localScale.x);
            currentLookahead = Mathf.Lerp(currentLookahead, targetDirectionX * lookaheadDistance, Time.deltaTime * 3f);

            Vector3 targetPosition = target.position + offset + new Vector3(currentLookahead, 0f, 0f);

            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
