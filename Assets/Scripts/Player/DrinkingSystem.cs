using System;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(AlcoholSystem))]
public sealed class DrinkingSystem : MonoBehaviour
{
    [SerializeField] private AlcoholSystem alcoholSystem;
    private bool isConsuming;
    public event Action<DrinkData> OnDrinkConsumed;

    private void Awake()
    {
        if (alcoholSystem == null) alcoholSystem = GetComponent<AlcoholSystem>();
    }

    /// <summary>Shared entry point for world drinks and NPC offers; false means nothing was consumed.</summary>
    public bool ConsumeDrink(DrinkData drink)
    {
        if (!isActiveAndEnabled || isConsuming || drink == null || alcoholSystem == null) return false;
        isConsuming = true;
        try
        {
            Debug.Log($"Consumiendo: {drink.DrinkName}", this);
            // Future animation starts here; call ApplyConsumption at its consumption marker.
            ApplyConsumption(drink);
            return true;
        }
        finally { isConsuming = false; }
    }

    private void ApplyConsumption(DrinkData drink)
    {
        alcoholSystem.AddAlcohol(drink.AlcoholAmount);
        // Emit after alcohol is applied, even when that addition started the ending.
        OnDrinkConsumed?.Invoke(drink);
    }
}
