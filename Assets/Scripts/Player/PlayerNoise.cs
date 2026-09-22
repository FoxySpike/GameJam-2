using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    [Header("Noise")]
    [SerializeField] private float sprintNoiseRadius = 12f;
    [SerializeField] private float noiseCooldown = 1f;

    private PlayerInputReader inputReader;

    private float noiseTimer;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
    }

    private void Update()
    {
        if (!inputReader.Sprint)
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