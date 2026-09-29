using UnityEngine;

public class PlayerBootstrapper : MonoBehaviour
{
    [Tooltip("El Prefab de tu jugador que se instanciará si no existe uno en la escena.")]
    [SerializeField] private GameObject playerPrefab;

    [Tooltip("¿Dónde debería aparecer si se instancia por primera vez?")]
    [SerializeField] private Transform debugSpawnPoint;

    private void Awake()
{
    if (PersistentPlayer.Instance == null)
    {
        Debug.LogWarning("[Bootstrapper] No se encontró al jugador. Instanciando Prefab de debug...");

        if (playerPrefab != null)
        {
            // Instanciamos respetando la posición y rotación del SpawnPoint
            Instantiate(playerPrefab, debugSpawnPoint.position, debugSpawnPoint.rotation);
        }
        else
        {
            Debug.LogError("No asignaste el PlayerPrefab en el Bootstrapper.");
        }
    }
    else
    {
        // SI YA EXISTE: Mover al jugador persistente al SpawnPoint de esta escena
        Debug.Log("[Bootstrapper] Jugador persistente detectado. Reubicando...");
        PersistentPlayer.Instance.TeleportTo(debugSpawnPoint);
    }
}
}