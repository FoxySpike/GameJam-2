using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BreathStaminaSystem staminaSystem;

    [Header("UI References")]
    [SerializeField] private Image fillImage;

    [Header("Visual Settings")]
    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private Color normalColor = new Color(0.2f, 0.7f, 1f); // Azul oxígeno
    [SerializeField] private Color exhaustedColor = new Color(1f, 0.2f, 0.2f); // Rojo agotado

    private float targetFillAmount = 1f;

    private void OnEnable()
    {
        if (staminaSystem != null)
        {
            // Nos suscribimos al evento
            staminaSystem.OnStaminaChanged += HandleStaminaChanged;
        }
    }

    private void OnDisable()
    {
        if (staminaSystem != null)
        {
            // Siempre nos desuscribimos para evitar fugas de memoria
            staminaSystem.OnStaminaChanged -= HandleStaminaChanged;
        }
    }

    private void Start()
    {
        if (staminaSystem != null)
        {
            // Inicializamos la barra al valor actual
            float initialRatio = staminaSystem.CurrentStamina / 100f; // o tu maxStamina
            UpdateUIImmediate(initialRatio);
        }
    }

    private void Update()
    {
        // Interpola suavemente el fillAmount para que el cambio no sea brusco
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);

        // Cambia el color si el jugador está exhausto
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
        fillImage.fillAmount = normalizedStamina;
    }
}