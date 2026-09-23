using UnityEngine;
using UnityEngine.AI;

public class PolicePatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Patrol Wait")]
    [SerializeField] private float minimumWaitTime = 1f;
    [SerializeField] private float maximumWaitTime = 2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private int currentPointIndex = -1;

    private bool isWaiting;
    private float waitTimer;

    private bool isInvestigatingNoise;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("PolicePatrol: No hay puntos de patrulla asignados.");
            return;
        }

        GoToRandomPoint();
    }

    private void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        if (isInvestigatingNoise)
        {
            HandleNoiseInvestigation();
            return;
        }

        if (isWaiting)
        {
            animator.SetBool("IsWalking", false);
            HandleWaiting();
            return;
        }

        if (agent.pathPending)
            return;

        animator.SetBool(
            "IsWalking",
            agent.velocity.sqrMagnitude > 0.01f
        );

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            StartWaiting();
        }
    }

    public void HearNoise(Vector3 noisePosition)
    {
        isInvestigatingNoise = true;
        isWaiting = false;

        agent.isStopped = false;
        agent.SetDestination(noisePosition);
    }

    private void HandleNoiseInvestigation()
    {
        if (agent.pathPending)
            return;

        animator.SetBool(
            "IsWalking",
            agent.velocity.sqrMagnitude > 0.01f
        );

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            isInvestigatingNoise = false;

            GoToRandomPoint();
        }
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = Random.Range(
            minimumWaitTime,
            maximumWaitTime
        );

        agent.isStopped = true;

        animator.SetBool("IsWalking", false);
    }

    private void HandleWaiting()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            isWaiting = false;

            agent.isStopped = false;

            GoToRandomPoint();
        }
    }

    private void GoToRandomPoint()
    {
        if (patrolPoints.Length == 1)
        {
            currentPointIndex = 0;
            agent.SetDestination(
                patrolPoints[0].position
            );
            return;
        }

        int newPointIndex = currentPointIndex;

        while (newPointIndex == currentPointIndex)
        {
            newPointIndex = Random.Range(
                0,
                patrolPoints.Length
            );
        }

        currentPointIndex = newPointIndex;

        agent.SetDestination(
            patrolPoints[currentPointIndex].position
        );
    }
}