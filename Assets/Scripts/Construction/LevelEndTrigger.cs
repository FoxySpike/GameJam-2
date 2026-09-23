using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jugador llegó a la meta. Cambiar de escena aquí.");
            // SceneManager.LoadScene("NombreDeLaSiguienteEscena"); // lo activo despues
        }
    }
}
