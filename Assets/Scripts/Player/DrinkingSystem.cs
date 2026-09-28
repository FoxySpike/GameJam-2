using System;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(AlcoholSystem))]
public sealed class DrinkingSystem : MonoBehaviour
{
    [SerializeField] private AlcoholSystem alcoholSystem;
    private bool isConsuming;
    private AudioSource consumptionAudio;
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

    public void PlayDrinkSound(AudioSource source)
    {
        if (source == null || source.clip == null) return;

        // Keep the sound alive when a single-use bottle is deactivated.
        if (consumptionAudio == null)
        {
            consumptionAudio = gameObject.AddComponent<AudioSource>();
            consumptionAudio.playOnAwake = false;
            consumptionAudio.loop = false;
            consumptionAudio.spatialBlend = 0f;
        }

        consumptionAudio.outputAudioMixerGroup = source.outputAudioMixerGroup;
        consumptionAudio.volume = source.volume;
        consumptionAudio.pitch = source.pitch;
        consumptionAudio.mute = source.mute;
        consumptionAudio.PlayOneShot(source.clip);
    }

    private void ApplyConsumption(DrinkData drink)
    {
        alcoholSystem.AddAlcohol(drink.AlcoholAmount);
        // Emit after alcohol is applied, even when that addition started the ending.
        OnDrinkConsumed?.Invoke(drink);
    }
}
