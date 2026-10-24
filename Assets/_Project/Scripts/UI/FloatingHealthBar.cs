using UnityEngine;
using CodigoDeEmergencia.Core;

namespace CodigoDeEmergencia.UI
{
    /// <summary>
    /// Barra de salud flotante que se muestra sobre la cabeza de los enemigos comunes.
    /// Utiliza sprites simples para ser ligera, responsiva y no requerir Canvas pesado.
    /// </summary>
    public class FloatingHealthBar : MonoBehaviour
    {
        [Header("--- Referencias y Configuración ---")]
        [Tooltip("Transform de la barra de relleno que escala horizontalmente.")]
        [SerializeField] private Transform fillTransform;

        [Tooltip("Desplazamiento vertical sobre el enemigo.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, 0f);

        [Tooltip("Ocultar automáticamente si la salud está al 100%.")]
        [SerializeField] private bool hideWhenFull = false;

        private IDamageable damageable;
        private Transform targetTransform;
        private SpriteRenderer fillRenderer;
        private Vector3 initialScale = Vector3.one;
        private Vector3 initialFillLocalPos = Vector3.zero;
        private float fillSpriteWidth = 1f;

        private void Awake()
        {
            if (fillTransform != null)
            {
                initialScale = fillTransform.localScale;
                initialFillLocalPos = fillTransform.localPosition;
                fillRenderer = fillTransform.GetComponent<SpriteRenderer>();
                if (fillRenderer != null && fillRenderer.sprite != null)
                {
                    fillSpriteWidth = fillRenderer.sprite.bounds.size.x;
                }
            }

            damageable = GetComponentInParent<IDamageable>();
            targetTransform = transform.parent != null ? transform.parent : transform;
        }

        private void LateUpdate()
        {
            if (targetTransform == null) return;

            // Mantener orientación recta y compensar si el enemigo padre tiene escala X negativa (Flip)
            transform.rotation = Quaternion.identity;
            transform.position = targetTransform.position + offset;

            if (transform.parent != null)
            {
                float parentSignX = Mathf.Sign(transform.parent.localScale.x);
                Vector3 barScale = transform.localScale;
                barScale.x = Mathf.Abs(barScale.x) * parentSignX;
                transform.localScale = barScale;
            }

            if (damageable != null && damageable.MaxHealth > 0)
            {
                float healthPercent = Mathf.Clamp01((float)damageable.CurrentHealth / damageable.MaxHealth);

                if (fillTransform != null)
                {
                    float currentScaleX = initialScale.x * healthPercent;
                    fillTransform.localScale = new Vector3(currentScaleX, initialScale.y, initialScale.z);

                    // Anclar el borde izquierdo de la barra para que baje de derecha a izquierda
                    float leftShift = -0.5f * (initialScale.x - currentScaleX) * fillSpriteWidth;
                    fillTransform.localPosition = new Vector3(initialFillLocalPos.x + leftShift, initialFillLocalPos.y, initialFillLocalPos.z);

                    if (fillRenderer != null)
                    {
                        if (healthPercent > 0.5f)
                            fillRenderer.color = new Color(0.15f, 0.95f, 0.35f, 1f);
                        else if (healthPercent > 0.25f)
                            fillRenderer.color = new Color(0.95f, 0.78f, 0.12f, 1f);
                        else
                            fillRenderer.color = new Color(0.95f, 0.2f, 0.2f, 1f);
                    }
                }

                if (hideWhenFull)
                {
                    gameObject.SetActive(healthPercent < 0.99f && healthPercent > 0.01f);
                }
            }
        }

        public void Initialize(IDamageable target)
        {
            damageable = target;
        }
    }
}
