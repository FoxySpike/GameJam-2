using UnityEngine;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private NoiseManager noiseManager;
    [SerializeField] private Image noiseBarFill; // Asigna aquí el Slider o Image (tipo Filled)

    private void OnEnable()
    {
        if (noiseManager != null)
            noiseManager.OnNoiseChanged += UpdateBar;
    }

    private void OnDisable()
    {
        if (noiseManager != null)
            noiseManager.OnNoiseChanged -= UpdateBar;
    }

    private void UpdateBar(float current, float max)
    {
        if (noiseBarFill != null)
        {
            // Si es un Image con Image Type = Filled
            noiseBarFill.fillAmount = current / max;
        }
    }
}