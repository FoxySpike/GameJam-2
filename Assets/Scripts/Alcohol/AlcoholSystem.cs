using System;
using UnityEngine;

[DefaultExecutionOrder(-100), DisallowMultipleComponent]
public sealed class AlcoholSystem : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxAlcohol = 100f;
    [SerializeField, Min(0f)] private float startingAlcohol;

    [Header("Umbrales de estado (unidades de alcohol)")]
    [SerializeField, Min(0f)] private float prendidoThreshold = 20f;
    [SerializeField, Min(0f)] private float tomadoThreshold = 40f;
    [SerializeField, Min(0f)] private float vueltoMierdaThreshold = 70f;

    public float CurrentAlcohol { get; private set; }
    public float MaxAlcohol => maxAlcohol;
    public NivelBorrachera CurrentState { get; private set; }
    public bool IsAtMaximum => CurrentAlcohol >= maxAlcohol;

    public event Action<float> OnAlcoholChanged;
    public event Action<NivelBorrachera> OnAlcoholStateChanged;
    public event Action OnMaxAlcoholReached;

    private void Awake()
    {
        OnValidate();
        CurrentAlcohol = Mathf.Clamp(startingAlcohol, 0f, maxAlcohol);
        CurrentState = CalculateState(CurrentAlcohol);
    }

    public void AddAlcohol(float amount)
    {
        if (float.IsNaN(amount) || float.IsInfinity(amount))
        {
            Debug.LogError("La cantidad de alcohol debe ser un número finito.", this);
            return;
        }

        float next = Mathf.Clamp(CurrentAlcohol + amount, 0f, maxAlcohol);
        if (Mathf.Approximately(next, CurrentAlcohol)) return;

        bool wasAtMaximum = IsAtMaximum;
        NivelBorrachera previousState = CurrentState;

        CurrentAlcohol = next;
        CurrentState = CalculateState(next);

        OnAlcoholChanged?.Invoke(CurrentAlcohol);

        if (previousState != CurrentState)
            OnAlcoholStateChanged?.Invoke(CurrentState);

        if (!wasAtMaximum && IsAtMaximum)
            OnMaxAlcoholReached?.Invoke();
    }

    private NivelBorrachera CalculateState(float value)
    {
        if (value >= vueltoMierdaThreshold) return NivelBorrachera.VueltoMierda;
        if (value >= tomadoThreshold) return NivelBorrachera.Tomado;
        if (value >= prendidoThreshold) return NivelBorrachera.Prendido;
        return NivelBorrachera.Sobrio;
    }

    private void OnValidate()
    {
        maxAlcohol = Mathf.Max(1f, maxAlcohol);
        startingAlcohol = Mathf.Clamp(startingAlcohol, 0f, maxAlcohol);
        prendidoThreshold = Mathf.Clamp(prendidoThreshold, 0.01f, maxAlcohol);
        tomadoThreshold = Mathf.Clamp(tomadoThreshold, prendidoThreshold, maxAlcohol);
        vueltoMierdaThreshold = Mathf.Clamp(vueltoMierdaThreshold, tomadoThreshold, maxAlcohol);
    }
}
