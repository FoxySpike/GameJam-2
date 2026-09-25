using System;
using UnityEngine;

[DefaultExecutionOrder(-100), DisallowMultipleComponent]
public sealed class AlcoholSystem : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxAlcohol = 100f;
    [SerializeField, Min(0f)] private float startingAlcohol;

    [Header("Umbrales de estado (unidades de alcohol)")]
    [SerializeField, Min(0f)] private float TipsyThreshold = 20f;
    [SerializeField, Min(0f)] private float DrunkThreshold = 40f;
    [SerializeField, Min(0f)] private float WastedThreshold = 70f;

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
            Debug.LogError("La cantidad de alcohol debe ser un n�mero finito.", this);
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
        if (value >= WastedThreshold) return NivelBorrachera.Wasted;
        if (value >= DrunkThreshold) return NivelBorrachera.Drunk;
        if (value >= TipsyThreshold) return NivelBorrachera.Tipsy;
        return NivelBorrachera.Sober;
    }

    private void OnValidate()
    {
        maxAlcohol = Mathf.Max(1f, maxAlcohol);
        startingAlcohol = Mathf.Clamp(startingAlcohol, 0f, maxAlcohol);
        TipsyThreshold = Mathf.Clamp(TipsyThreshold, 0.01f, maxAlcohol);
        DrunkThreshold = Mathf.Clamp(DrunkThreshold, TipsyThreshold, maxAlcohol);
        WastedThreshold = Mathf.Clamp(WastedThreshold, DrunkThreshold, maxAlcohol);
    }
}
