using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [Header("Visual Items in Hand")]
    [Tooltip("El modelo visual del pollo en la mano del jugador")]
    [SerializeField] private GameObject chickenInHand;

    // Estado lógico: Cualquiera puede preguntar si el jugador tiene el pollo
    public bool HasChicken { get; private set; }

    private void Awake()
    {
        // Aseguramos que empiece con las manos vacías
        ClearHand();
    }

    public void GiveChicken()
    {
        HasChicken = true;
        if (chickenInHand != null) chickenInHand.SetActive(true);
        Debug.Log("[PlayerHand] El jugador ahora tiene el pollo en la mano.");
    }

    public void ClearHand()
    {
        HasChicken = false;
        if (chickenInHand != null) chickenInHand.SetActive(false);
    }
}