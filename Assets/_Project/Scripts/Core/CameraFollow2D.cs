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

        private static CameraFollow2D activeInstance;
        private Camera cam;
        private Vector3 currentVelocity;
        private Vector3 lastUnshakenPos;
        private float currentLookahead;
        private float shakeTimer;
        private float shakeDuration;
        private float shakeIntensity;

        private void Awake()
        {
            activeInstance = this;
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (target == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    target = playerObj.transform;
                }
            }
            lastUnshakenPos = transform.position;
            if (cam != null && cam.orthographic && target != null)
            {
                cam.orthographicSize = GameSettings.TargetCameraOrthoSize;
            }
        }

        public static void RequestShake(float intensity, float duration)
        {
            if (activeInstance == null) return;
            float scale = GameSettings.ScreenShakeScale;
            if (scale <= 0.01f) return;
            activeInstance.TriggerShake(intensity * scale, duration);
        }

        private void TriggerShake(float intensity, float duration)
        {
            if (intensity > shakeIntensity || shakeTimer <= 0f)
            {
                shakeIntensity = intensity;
                shakeDuration = Mathf.Max(0.05f, duration);
                shakeTimer = shakeDuration;
            }
        }

        private void LateUpdate()
        {
            if (cam != null && cam.orthographic && target != null)
            {
                float desiredSize = GameSettings.TargetCameraOrthoSize;
                if (Mathf.Abs(cam.orthographicSize - desiredSize) > 0.005f)
                {
                    cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredSize, Time.unscaledDeltaTime * 8f);
                }
            }

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

            lastUnshakenPos = Vector3.SmoothDamp(lastUnshakenPos, targetPosition, ref currentVelocity, smoothTime);

            Vector3 shakeOffset = Vector3.zero;
            if (shakeTimer > 0f && Time.timeScale > 0f)
            {
                shakeTimer -= Time.deltaTime;
                float damp = Mathf.Clamp01(shakeTimer / Mathf.Max(0.01f, shakeDuration));
                Vector2 rnd = Random.insideUnitCircle * (shakeIntensity * damp);
                shakeOffset = new Vector3(rnd.x, rnd.y, 0f);
            }

            transform.position = lastUnshakenPos + shakeOffset;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Posiciona la cámara instantáneamente sobre el objetivo (útil al cargar una partida en un checkpoint lejano).
        /// </summary>
        public void SnapToTargetImmediate()
        {
            if (target == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) target = playerObj.transform;
            }
            if (target == null) return;

            float targetDirectionX = Mathf.Sign(target.localScale.x);
            currentLookahead = targetDirectionX * lookaheadDistance;
            Vector3 targetPosition = target.position + offset + new Vector3(currentLookahead, 0f, 0f);

            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }

            currentVelocity = Vector3.zero;
            lastUnshakenPos = targetPosition;
            transform.position = targetPosition;
        }
    }
}
