using System;
using UnityEngine;

public class BreathStaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float drainRate = 30f;
    [SerializeField] private float regenRate = 15f;

    [Header("Shake Multipliers")]
    [SerializeField] private float normalShake = 1.0f;
    [SerializeField] private float focusShake = 0.15f;
    [SerializeField] private float exhaustedShake = 2.5f;

    [Header("Smoothing")]
    [Tooltip("Velocidad a la que el temblor pasa de normal a concentrado/exhausto (evita saltos bruscos)")]
    [SerializeField] private float factorTransitionSpeed = 3.0f; // Ajustable desde el Inspector

    public float CurrentStamina { get; private set; }
    public bool IsExhausted { get; private set; }

    public event Action<float> OnStaminaChanged;

    private float currentAppliedFactor = 1.0f; // Factor que se aplica gradualmente

    private void Awake()
    {
        CurrentStamina = maxStamina;
        currentAppliedFactor = normalShake;
    }

    /// <summary>
    /// Procesa el estado de la respiración y devuelve el multiplicador de temblor suavizado.
    /// </summary>
    public float ProcessBreathAndGetShakeFactor(bool isTryingToHoldBreath)
    {
        float targetFactor = normalShake;

        if (IsExhausted)
        {
            RegenerateStamina();

            if (CurrentStamina >= maxStamina)
                IsExhausted = false;

            targetFactor = exhaustedShake;
        }
        else if (isTryingToHoldBreath)
        {
            DrainStamina();

            if (CurrentStamina <= 0)
                IsExhausted = true;

            targetFactor = focusShake;
        }
        else
        {
            RegenerateStamina();
            targetFactor = normalShake;
        }

        // SUAVIZADO: Nos movemos gradualmente del factor actual hacia el objetivo en cada frame
        currentAppliedFactor = Mathf.MoveTowards(
            currentAppliedFactor,
            targetFactor,
            factorTransitionSpeed * Time.deltaTime
        );

        return currentAppliedFactor;
    }

    private void DrainStamina()
    {
        CurrentStamina -= drainRate * Time.deltaTime;
        CurrentStamina = Mathf.Max(CurrentStamina, 0f);
        OnStaminaChanged?.Invoke(CurrentStamina / maxStamina);
    }

    private void RegenerateStamina()
    {
        if (CurrentStamina < maxStamina)
        {
            CurrentStamina += regenRate * Time.deltaTime;
            CurrentStamina = Mathf.Min(CurrentStamina, maxStamina);
            OnStaminaChanged?.Invoke(CurrentStamina / maxStamina);
        }
    }
}