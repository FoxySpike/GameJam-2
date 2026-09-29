using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [Header("Visual Settings")]
    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private Color normalColor = new Color(0.2f, 0.7f, 1f);
    [SerializeField] private Color exhaustedColor = new Color(1f, 0.2f, 0.2f);

    private float targetFillAmount = 1f;

    // Ya no lo serializamos. Ahora lo recibiremos dinámicamente.
    private BreathStaminaSystem currentStaminaSystem;

    // NUEVO: Método para inyectar la dependencia
    public void SetStaminaSystem(BreathStaminaSystem newSystem)
    {
        // 1. Si ya teníamos un sistema asignado antes, nos desuscribimos por seguridad
        if (currentStaminaSystem != null)
        {
            currentStaminaSystem.OnStaminaChanged -= HandleStaminaChanged;
        }

        currentStaminaSystem = newSystem;

        // 2. Si nos pasaron un sistema válido, nos suscribimos e inicializamos
        if (currentStaminaSystem != null)
        {
            currentStaminaSystem.OnStaminaChanged += HandleStaminaChanged;
            UpdateUIImmediate(currentStaminaSystem.CurrentStamina / 100f);
        }
    }

    private void Update()
    {
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);

        if (currentStaminaSystem != null)
        {
            fillImage.color = Color.Lerp(
                fillImage.color,
                currentStaminaSystem.IsExhausted ? exhaustedColor : normalColor,
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
        fillImage.fillAmount = normalizedStamina;
    }

    // Limpieza importante si la UI se apaga
    private void OnDisable()
    {
        if (currentStaminaSystem != null)
        {
            currentStaminaSystem.OnStaminaChanged -= HandleStaminaChanged;
            currentStaminaSystem = null; // Soltamos la referencia
        }
    }
}