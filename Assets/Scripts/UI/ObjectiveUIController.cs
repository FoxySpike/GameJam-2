using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ObjectiveUIController : MonoBehaviour
{
    [Header("Textos de Objetivos")]
    [SerializeField] private string startObjectiveText = "Find something to cook";
    [SerializeField] private string cookObjectiveText = "Take the raw chicken to the grill";
    [SerializeField] private string escapeObjectiveText = "Leave the store!";

    private TextMeshProUGUI objectiveText;

    private void Awake()
    {
        objectiveText = GetComponent<TextMeshProUGUI>();
        objectiveText.text = startObjectiveText;
    }

    private void OnEnable()
    {
        // Ambos eventos se escuchan a nivel de CLASE (estáticos), 
        // sin necesidad de referencias por Inspector ni dependencias de escena.
        FridgeInteractable.OnChickenExtracted += HandleChickenExtracted;
        GrillMinigameController.OnAnyMinigameEnded += HandleMinigameEnded;
    }

    private void OnDisable()
    {
        // Siempre desuscribirse para evitar fugas de memoria o errores
        FridgeInteractable.OnChickenExtracted -= HandleChickenExtracted;
        GrillMinigameController.OnAnyMinigameEnded -= HandleMinigameEnded;
    }

    private void HandleChickenExtracted()
    {
        objectiveText.text = cookObjectiveText;
    }

    private void HandleMinigameEnded(GrillMinigameController.CookingResult result)
    {
        if (result == GrillMinigameController.CookingResult.Perfect)
        {
            objectiveText.text = escapeObjectiveText;
        }
    }
}