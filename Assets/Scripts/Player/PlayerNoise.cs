using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    [Header("Noise")]
    [SerializeField] private float sprintNoiseRadius = 12f;
    [SerializeField] private float noiseCooldown = 1f;

    private PlayerMovementV3 movement;

    private float noiseTimer;

    private void Awake()
    {
        movement = GetComponent<PlayerMovementV3>();
    }

private void Update()
{
    if (movement == null || !movement.IsSprinting)
    {
        noiseTimer = 0f;
        return;
    }

    noiseTimer -= Time.deltaTime;

    if (noiseTimer > 0f)
        return;

    MakeNoise();
    noiseTimer = noiseCooldown;
}

    private void MakeNoise()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            sprintNoiseRadius
        );

        foreach (Collider nearbyObject in nearbyObjects)
        {
            PolicePatrol police = nearbyObject.GetComponentInParent<PolicePatrol>();

            if (police != null)
            {
                police.HearNoise(transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            sprintNoiseRadius
        );
    }
}