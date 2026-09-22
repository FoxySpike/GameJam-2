using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    public void Respawn()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("PlayerRespawn: No hay Spawn Point asignado.");
            return;
        }

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
    }
}