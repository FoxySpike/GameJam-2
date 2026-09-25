using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string spawnPointTag = "SpawnPoint";


    public void Respawn()
    {
        GameObject spawnObject = GameObject.FindGameObjectWithTag(spawnPointTag);


        if (spawnObject == null)
        {
            Debug.LogWarning("PlayerRespawn: No existe un SpawnPoint en la escena actual.");
            return;
        }


        Transform spawnPoint = spawnObject.transform;


        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }


        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;


        if (characterController != null)
        {
            characterController.enabled = true;
        }


        Debug.Log("Jugador enviado al SpawnPoint actual.");
    }
}