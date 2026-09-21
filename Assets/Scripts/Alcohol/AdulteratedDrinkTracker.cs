using System;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(DrinkingSystem))]
public sealed class AdulteratedDrinkTracker : MonoBehaviour
{
    [SerializeField] private DrinkingSystem drinkingSystem;
    [SerializeField, Min(1)] private int requiredAdulteratedDrinks = 2;
    public int ConsumedCount { get; private set; }
    public bool IsTriggered { get; private set; }
    public event Action OnSpecialIntoxicationTriggered;

    private void Awake()
    {
        if (drinkingSystem == null) drinkingSystem = GetComponent<DrinkingSystem>();
    }

    private void OnEnable() => drinkingSystem.OnDrinkConsumed += OnDrinkConsumed;
    private void OnDisable() => drinkingSystem.OnDrinkConsumed -= OnDrinkConsumed;

    private void OnDrinkConsumed(DrinkData drink)
    {
        if (IsTriggered || !drink.IsAdulterated) return;
        ConsumedCount++;
        if (ConsumedCount < Mathf.Max(1, requiredAdulteratedDrinks)) return;
        IsTriggered = true;
        OnSpecialIntoxicationTriggered?.Invoke();
    }
}
