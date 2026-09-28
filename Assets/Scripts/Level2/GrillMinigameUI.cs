using UnityEngine;
using UnityEngine.UI;

public class GrillMinigameUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GrillMinigameController minigameController;

    [Header("Progreso Superior")]
    [SerializeField] private Image progressBarFill;

    [Header("Temperatura (Aguja)")]
    [SerializeField] private RectTransform needleTransform;
    [Tooltip("Rotación en Z cuando la temperatura es 0")]
    [SerializeField] private float minHeatAngle = 75f;
    [Tooltip("Rotación en Z cuando la temperatura es 100")]
    [SerializeField] private float maxHeatAngle = -75f;

    private void Update()
    {
        // Si no hemos asignado el controlador, evitamos ejecutar la lógica para prevenir errores
        if (minigameController == null) return;

        UpdateProgressBar();
        UpdateTemperatureNeedle();
    }

    private void UpdateProgressBar()
    {
        // Dividimos el progreso actual entre el máximo para obtener un rango normalizado (0.0 a 1.0)
        float progressNormalized = minigameController.CurrentProgress / minigameController.MaxCookingTime;

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progressNormalized;
        }
    }

    private void UpdateTemperatureNeedle()
    {
        if (needleTransform == null) return;

        // Convertimos el calor (0 a 100) en una proporción entre 0.0 y 1.0
        float heatNormalized = minigameController.CurrentHeat / 100f;

        // Interpolamos el ángulo según el porcentaje de calor actual
        // Ejemplo: Con heatNormalized en 0.5 (mitad), el ángulo será 0 (apuntando hacia arriba)
        float currentAngle = Mathf.Lerp(minHeatAngle, maxHeatAngle, heatNormalized);

        // Aplicamos la rotación únicamente en el eje Z (plano 2D de la UI)
        needleTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
}