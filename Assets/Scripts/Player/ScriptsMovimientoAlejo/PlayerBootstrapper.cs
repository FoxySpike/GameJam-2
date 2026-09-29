using UnityEngine;

public class PlayerBootstrapper : MonoBehaviour
{
    [Tooltip("El Prefab de tu jugador que se instanciará si no existe uno en la escena.")]
    [SerializeField] private GameObject playerPrefab;

    [Tooltip("¿Dónde debería aparecer si se instancia por primera vez?")]
    [SerializeField] private Transform debugSpawnPoint;

    private void Awake()
    {
        // Revisamos si el Singleton existe. Si es nulo, significa que abrimos esta escena directamente.
        if (PersistentPlayer.Instance == null)
        {
            Debug.LogWarning("[Bootstrapper] No se encontró al jugador. Instanciando Prefab de debug...");

            if (playerPrefab != null)
            {
                // Instanciamos al jugador
                GameObject newPlayer = Instantiate(playerPrefab, debugSpawnPoint.position, debugSpawnPoint.rotation);
                // NOTA: El Awake del PersistentPlayer dentro del prefab se ejecutará aquí 
                // y se asignará a sí mismo como Instance y ejecutará el DontDestroyOnLoad.
            }
            else
            {
                Debug.LogError("No asignaste el PlayerPrefab en el Bootstrapper.");
            }
        }
    }
}