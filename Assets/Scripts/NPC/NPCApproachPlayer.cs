using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class NPCApproachPlayer : MonoBehaviour
{
    [SerializeField] private PlayerInputReader player;
    [SerializeField, Min(0f)] private float detectionDistance = 8f;
    [SerializeField, Min(0.1f)] private float repathInterval = 0.25f;

    private NavMeshAgent agent;
    private float nextRepathTime;
    private bool hasApproached;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            Debug.LogError(
                "NPCApproachPlayer requires the player's input reader.",
                this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        // Pause during dialogue and the level-ending sequence.
        if (hasApproached || player == null || !player.GameplayEnabled)
        {
            agent.isStopped = true;
            return;
        }

        Vector3 offset = player.transform.position - transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;

        if (distance <= 3f)
        {
            agent.isStopped = true;
            return;
        }

        if (distance > detectionDistance)
        {
            agent.isStopped = true;
            return;
        }

        // Approach once, then let NPCDrinkOffer handle the interaction.
        if (distance <= agent.stoppingDistance)
        {
            hasApproached = true;
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        agent.isStopped = false;

        // Refresh the destination periodically as the player moves.
        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathInterval;
            agent.SetDestination(player.transform.position);
        }
    }

    public void StopFollowing()
    {
        hasApproached = true;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void OnDisable()
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = true;
    }
}