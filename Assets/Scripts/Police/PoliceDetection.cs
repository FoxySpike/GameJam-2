using UnityEngine;

public class PoliceDetection : MonoBehaviour
{
    [SerializeField] private ChickenCarryController chicken;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        DropCarriedChicken();

        PlayerRespawn playerRespawn = other.GetComponent<PlayerRespawn>();

        if (playerRespawn != null)
        {
            playerRespawn.Respawn();
        }
    }

    private void DropCarriedChicken()
    {
        ChickenCarryController targetChicken = chicken;
        if (targetChicken == null)
            targetChicken = FindAnyObjectByType<ChickenCarryController>();

        if (targetChicken != null)
            targetChicken.Drop(Vector3.zero, 0f);
    }
}
