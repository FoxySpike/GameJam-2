using UnityEngine;

public class FridgeExtractionZone : MonoBehaviour
{
    [Tooltip("Referencia al controlador principal de la nevera")]
    [SerializeField] private FridgeInteractable fridgeController;

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si lo que cruzó el trigger es nuestro objetivo
        if (other.CompareTag("TargetItem"))
        {
            Debug.Log("[ExtractionZone] ¡El objetivo fue extraído!");

            // Le avisamos a la nevera que ganamos el minijuego, pasándole el objeto
            fridgeController.OnItemExtracted(other.gameObject);
        }
    }
}