using System;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance { get; private set; }

    [Header("Noise Settings")]
    [SerializeField] private float maxNoise = 100f;
    [SerializeField] private float currentNoise = 0f;

    [Tooltip("Cantidad de ruido que se disipa por segundo.")]
    [SerializeField] private float noiseDecreaseRate = 15f;

    public float CurrentNoise => currentNoise;
    public float MaxNoise => maxNoise;

    // Eventos
    public event Action<float, float> OnNoiseChanged;
    public event Action OnMaxNoiseReached;

    private bool isMaxNoiseReached = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Se intentó crear más de un NoiseManager. Destruyendo duplicado.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isMaxNoiseReached || currentNoise <= 0f) return;

        currentNoise -= noiseDecreaseRate * Time.deltaTime;
        if (currentNoise < 0f) currentNoise = 0f;

        OnNoiseChanged?.Invoke(currentNoise, maxNoise);
    }

    public void AddNoise(float amount)
    {
        if (isMaxNoiseReached) return;

        currentNoise += amount;
        currentNoise = Mathf.Clamp(currentNoise, 0, maxNoise);

        OnNoiseChanged?.Invoke(currentNoise, maxNoise);

        if (currentNoise >= maxNoise)
        {
            isMaxNoiseReached = true;
            TriggerGameOverEvent();
        }
    }

    /// <summary>
    /// Este método se dispara ÚNICAMENTE cuando la barra de ruido llega al 100%.
    /// Aquí puedes poner la lógica secreta de derrota cuando estés listo.
    /// </summary>
    private void TriggerGameOverEvent()
    {
        Debug.Log("🚨 ¡RUIDO MÁXIMO ALCANZADO!");
        OnMaxNoiseReached?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
