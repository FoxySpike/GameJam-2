using UnityEngine;

public class NoiseNPCConnector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SleeperNPC sleepingNPC;

    private void Start()
    {
        // Nos conectamos al evento del NoiseManager en el Start
        // (Usamos Start para asegurar que el NoiseManager ya hizo su Awake)
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.OnMaxNoiseReached += HandleMaxNoiseReached;
        }
        else
        {
            Debug.LogError("¡No se encontró el NoiseManager en la escena!");
        }
    }

    private void OnDestroy()
    {
        // Siempre desuscribirse cuando este objeto se destruya
        if (NoiseManager.Instance != null)
        {
            NoiseManager.Instance.OnMaxNoiseReached -= HandleMaxNoiseReached;
        }
    }

    private void HandleMaxNoiseReached()
    {
        Debug.Log("[Connector] 🚨 ¡El ruido despertó al NPC!");

        if (sleepingNPC != null)
        {
            sleepingNPC.WakeUp();
        }
    }
}