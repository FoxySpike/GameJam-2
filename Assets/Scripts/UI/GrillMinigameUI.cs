using UnityEngine;
using UnityEngine.UI; // Importante para la barra de progreso

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
        // Si no hemos asignado el controlador, no hacemos nada para evitar errores
        if (minigameController == null) return;

        UpdateProgressBar();
        UpdateTemperatureNeedle();
    }

    private void UpdateProgressBar()
    {
        // Dividimos progreso actual entre el máximo para obtener un valor de 0 a 1
        float progressNormalized = minigameController.CurrentProgress / minigameController.MaxCookingTime;

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = progressNormalized;
        }
    }

    private void UpdateTemperatureNeedle()
    {
        if (needleTransform == null) return;

        // Convertimos el calor (0 a 100) en un porcentaje de 0 a 1
        float heatNormalized = minigameController.CurrentHeat / 100f;

        // Mathf.Lerp calcula el valor proporcional entre dos números
        // Ejemplo: Si heatNormalized es 0.5 (mitad), el ángulo será 0 (recto hacia arriba)
        float currentAngle = Mathf.Lerp(minHeatAngle, maxHeatAngle, heatNormalized);

        // Aplicamos la rotación solo en el eje Z
        needleTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
}