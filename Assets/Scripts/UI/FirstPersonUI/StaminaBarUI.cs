using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    // 1. Quitamos el [SerializeField]. La UI lo buscará sola.
    private BreathStaminaSystem staminaSystem;

    [Header("UI References")]
    [SerializeField] private Image fillImage;

    [Header("Visual Settings")]
    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private Color normalColor = new Color(0.2f, 0.7f, 1f);
    [SerializeField] private Color exhaustedColor = new Color(1f, 0.2f, 0.2f);

    private float targetFillAmount = 1f;

    private void OnEnable()
    {
        // 2. BUSCAMOS dinámicamente el brazo en la Escena 2 al encender la UI
        if (staminaSystem == null)
        {
            // FindFirstObjectByType busca en la escena activa el primer objeto con este script
            staminaSystem = Object.FindFirstObjectByType<BreathStaminaSystem>();
        }

        if (staminaSystem != null)
        {
            staminaSystem.OnStaminaChanged += HandleStaminaChanged;

            // Inicializamos la barra
            UpdateUIImmediate(staminaSystem.CurrentStamina / 100f);
        }
        else
        {
            Debug.LogWarning("[StaminaBarUI] No se encontró ningún BreathStaminaSystem en la escena.");
        }
    }

    private void OnDisable()
    {
        if (staminaSystem != null)
        {
            staminaSystem.OnStaminaChanged -= HandleStaminaChanged;
        }
    }

    private void Update()
    {
        if (fillImage == null) return;

        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);

        if (staminaSystem != null)
        {
            fillImage.color = Color.Lerp(
                fillImage.color,
                staminaSystem.IsExhausted ? exhaustedColor : normalColor,
                Time.deltaTime * lerpSpeed
            );
        }
    }

    private void HandleStaminaChanged(float normalizedStamina)
    {
        targetFillAmount = normalizedStamina;
    }

    private void UpdateUIImmediate(float normalizedStamina)
    {
        targetFillAmount = normalizedStamina;
        if (fillImage != null) fillImage.fillAmount = normalizedStamina;
    }
}