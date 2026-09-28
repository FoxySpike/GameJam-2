using UnityEngine;
using UnityEngine.UI;

public class NoiseUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Image noiseBarFill;

    private void Start()
    {
        // Usamos Start() porque nos garantiza que TODOS los Awake() de la escena ya terminaron.
        // Así estamos 100% seguros de que NoiseManager ya creó su Instance.
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.OnNoiseChanged += UpdateBar;

            // Opcional pero recomendado: Actualizar la barra visualmente al estado actual del manager al iniciar
            // UpdateBar(NoiseManager.Instance.CurrentNoise, NoiseManager.Instance.MaxNoise); 
        }
        else
        {
            Debug.LogWarning("[NoiseUI] No se encontró NoiseManager.Instance en el Start.");
        }
    }

    private void OnDestroy()
    {
        // En lugar de OnDisable, nos desconectamos cuando este objeto sea destruido (al cambiar de escena o cerrar el juego)
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