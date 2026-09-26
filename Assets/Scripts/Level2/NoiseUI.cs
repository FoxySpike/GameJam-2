using UnityEngine;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
{
    [Header("Dependencies")]
    // 1. ELIMINAMOS el [SerializeField] del noiseManager. Ya no se asigna en el Inspector.
    [SerializeField] private Image noiseBarFill;

    private void OnEnable()
    {
        // 2. Nos conectamos directamente usando la Instancia global
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.OnNoiseChanged += UpdateBar;
        }
        else
        {
            Debug.LogWarning("[NoiseUI] No se encontró NoiseManager.Instance. ¿Estás en la Escena 2?");
        }
    }

    private void OnDisable()
    {
        // 3. Nos desconectamos usando la misma Instancia
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.OnNoiseChanged -= UpdateBar;
        }
    }

    private void UpdateBar(float current, float max)
    {
        if (noiseBarFill != null)
        {
            noiseBarFill.fillAmount = current / max;
        }
    }
}