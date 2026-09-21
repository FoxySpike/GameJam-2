using System;
using UnityEngine;

[DefaultExecutionOrder(-100), DisallowMultipleComponent]
public sealed class AlcoholSystem : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxAlcohol = 100f;
    [SerializeField, Min(0f)] private float startingAlcohol;
    [Header("State thresholds (alcohol units)")]
    [SerializeField, Min(0f)] private float tipsyThreshold = 20f;
    [SerializeField, Min(0f)] private float drunkThreshold = 40f;
    [SerializeField, Min(0f)] private float wastedThreshold = 70f;

    public float CurrentAlcohol { get; private set; }
    public float MaxAlcohol => maxAlcohol;
    public AlcoholState CurrentState { get; private set; }
    public bool IsAtMaximum => CurrentAlcohol >= maxAlcohol;
    public event Action<float> OnAlcoholChanged;
    public event Action<AlcoholState> OnAlcoholStateChanged;
    public event Action OnMaxAlcoholReached;

    private void Awake()
    {
        OnValidate();
        CurrentAlcohol = Mathf.Clamp(startingAlcohol, 0f, maxAlcohol);
        CurrentState = CalculateState(CurrentAlcohol);
    }

    /// <summary>Changes the clamped value, then notifies state and maximum-crossing listeners.</summary>
    public void AddAlcohol(float amount)
    {
        if (float.IsNaN(amount) || float.IsInfinity(amount))
        {
            Debug.LogError("Alcohol amount must be finite.", this);
            return;
        }

        float next = Mathf.Clamp(CurrentAlcohol + amount, 0f, maxAlcohol);
        if (next == CurrentAlcohol) return;

        bool wasAtMaximum = IsAtMaximum;
        AlcoholState previousState = CurrentState;
        CurrentAlcohol = next;
        CurrentState = CalculateState(next);
        OnAlcoholChanged?.Invoke(CurrentAlcohol);
        if (previousState != CurrentState) OnAlcoholStateChanged?.Invoke(CurrentState);
        if (!wasAtMaximum && IsAtMaximum) OnMaxAlcoholReached?.Invoke();
    }

    private AlcoholState CalculateState(float value)
    {
        if (value >= wastedThreshold) return AlcoholState.Wasted;
        if (value >= drunkThreshold) return AlcoholState.Drunk;
        if (value >= tipsyThreshold) return AlcoholState.Tipsy;
        return AlcoholState.Sober;
    }

    private void OnValidate()
    {
        maxAlcohol = Mathf.Max(1f, maxAlcohol);
        startingAlcohol = Mathf.Clamp(startingAlcohol, 0f, maxAlcohol);
        tipsyThreshold = Mathf.Clamp(tipsyThreshold, 0.01f, maxAlcohol);
        drunkThreshold = Mathf.Clamp(drunkThreshold, tipsyThreshold, maxAlcohol);
        wastedThreshold = Mathf.Clamp(wastedThreshold, drunkThreshold, maxAlcohol);
    }
}
