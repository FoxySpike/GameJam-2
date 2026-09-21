using UnityEngine;
using UnityEngine.UI;

public sealed class BalanceHUD : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerBalanceSystem balanceSystem;
    [SerializeField] private Slider balanceSlider;

    private void Awake()
    {
        if (balanceSystem != null && balanceSlider != null) return;

        Debug.LogError("BalanceHUD requiere un PlayerBalanceSystem y un Slider.", this);
        enabled = false;
    }

    private void OnEnable()
    {
        // Configurar forzosamente el Slider para que vaya de -1 a 1
        balanceSlider.minValue = -1f;
        balanceSlider.maxValue = 1f;

        // Suscribirse al evento de cambio
        balanceSystem.OnBalanceChanged += UpdateSlider;
    }

    private void OnDisable()
    {
        if (balanceSystem != null)
        {
            balanceSystem.OnBalanceChanged -= UpdateSlider;
        }
    }

    private void UpdateSlider(float balanceValue)
    {
        // El valor 0 quedará exactamente en la mitad del Slider
        balanceSlider.SetValueWithoutNotify(balanceValue);

        // Opcional: Aquí podríamos hacer que el slider cambie de color a rojo 
        // si se acerca mucho a los extremos (ej: > 0.8 o < -0.8).
    }
}