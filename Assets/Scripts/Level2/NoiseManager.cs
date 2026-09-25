using System;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    [Header("Noise Settings")]
    [SerializeField] private float maxNoise = 100f;
    [SerializeField] private float currentNoise = 0f;

    [Tooltip("Cantidad de ruido que se disipa por segundo.")]
    [SerializeField] private float noiseDecreaseRate = 15f;

    // Evento para que la UI se entere cuando el ruido cambie
    public event Action<float, float> OnNoiseChanged;

    // Evento para cuando se llena la barra
    public event Action OnMaxNoiseReached;

    private bool isMaxNoiseReached = false;

    private void Update()
    {
        // Si ya perdimos/llegamos al máximo, o si no hay ruido, no hacemos nada
        if (isMaxNoiseReached || currentNoise <= 0f) return;

        // Reducimos el ruido basándonos en el tiempo que pasó este frame
        currentNoise -= noiseDecreaseRate * Time.deltaTime;

        // Evitamos que baje de 0
        if (currentNoise < 0f) currentNoise = 0f;

        // MUY IMPORTANTE: Le avisamos a la UI que el valor cambió
        OnNoiseChanged?.Invoke(currentNoise, maxNoise);
    }

    public void AddNoise(float amount)
    {
        if (isMaxNoiseReached) return;

        currentNoise += amount;
        currentNoise = Mathf.Clamp(currentNoise, 0, maxNoise);

        // Avisamos a la UI del pico de ruido
        OnNoiseChanged?.Invoke(currentNoise, maxNoise);

        if (currentNoise >= maxNoise)
        {
            isMaxNoiseReached = true;
            OnMaxNoiseReached?.Invoke();
            Debug.Log("¡BARRA DE RUIDO LLENA! Ejecutando evento secreto...");
        }
    }
}