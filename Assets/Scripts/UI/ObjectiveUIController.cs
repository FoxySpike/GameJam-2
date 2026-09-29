using UnityEngine;
using TMPro; // Asumiendo que usas TextMeshPro

[RequireComponent(typeof(TextMeshProUGUI))]
public class ObjectiveUIController : MonoBehaviour
{
    private TextMeshProUGUI objectiveText;

    private void Awake()
    {
        objectiveText = GetComponent<TextMeshProUGUI>();
        objectiveText.text = "Encuentra algo para cocinar"; // Objetivo inicial
    }

    private void OnEnable()
    {
        // Nos suscribimos a los eventos
        FridgeInteractable.OnChickenExtracted += HandleChickenExtracted;
        // Más adelante puedes suscribirte a la parrilla aquí
        // GrillMinigameController.OnMinigameWon += HandleChickenCooked; 
    }

    private void OnDisable()
    {
        // SIEMPRE desuscribirse para evitar memory leaks
        FridgeInteractable.OnChickenExtracted -= HandleChickenExtracted;
    }

    private void HandleChickenExtracted()
    {
        // Actualizamos la UI
        objectiveText.text = "Lleva el pollo crudo a la parrilla";
    }
}