using UnityEngine;

public class PoliceDetection : MonoBehaviour
{
    [SerializeField] private ChickenCarryController chicken;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();
        OnPlayerCaught(playerRespawn);
    }

    private void OnPlayerCaught(PlayerRespawn playerRespawn)
    {
        ChickenCarryController targetChicken = chicken;
        if (targetChicken == null)
            targetChicken = FindAnyObjectByType<ChickenCarryController>();

        if (targetChicken != null)
            targetChicken.Drop(Vector3.zero, 0f);

        if (playerRespawn != null)
            playerRespawn.Respawn();
    }
}
