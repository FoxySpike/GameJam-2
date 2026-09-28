using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class NPCApproachPlayer : MonoBehaviour
{
    [SerializeField] private PlayerInputReader player;
    [SerializeField, Min(0f)] private float detectionDistance = 8f;
    [SerializeField, Min(0.1f)] private float repathInterval = 0.25f;

    [SerializeField] private Animator animator;

    private static readonly int FollowingHash = Animator.StringToHash("following");
    private bool hasFollowingParameter;
    private NavMeshAgent agent;
    private float nextRepathTime;
    private bool hasApproached;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == FollowingHash && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    hasFollowingParameter = true;
                    // Navigation owns movement; the animation only supplies the pose.
                    animator.applyRootMotion = false;
                    break;
                }
            }
        }

        SetFollowingAnimation(false);

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
        {
            SetFollowingAnimation(false);
            return;
        }

        // Pause during dialogue and the level-ending sequence.
        if (hasApproached || player == null || !player.GameplayEnabled)
        {
            agent.isStopped = true;
            SetFollowingAnimation(false);
            return;
        }

        Vector3 offset = player.transform.position - transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;

        if (distance <= 3f)
        {
            agent.isStopped = true;
            SetFollowingAnimation(false);
            return;
        }

        if (distance > detectionDistance)
        {
            agent.isStopped = true;
            SetFollowingAnimation(false);
            return;
        }

        // Approach once, then let NPCDrinkOffer handle the interaction.
        if (distance <= agent.stoppingDistance)
        {
            hasApproached = true;
            agent.isStopped = true;
            agent.ResetPath();
            SetFollowingAnimation(false);
            return;
        }

        agent.isStopped = false;
        SetFollowingAnimation(true);

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
        SetFollowingAnimation(false);

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void OnDisable()
    {
        SetFollowingAnimation(false);

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = true;
    }

    private void SetFollowingAnimation(bool following)
    {
        if (animator != null && hasFollowingParameter)
            animator.SetBool(FollowingHash, following);
    }
}
