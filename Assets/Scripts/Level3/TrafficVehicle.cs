using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public sealed class TrafficVehicle : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private GameObject vehicleModel;
    [SerializeField] private Vector3 modelOffset;
    [SerializeField] private Vector3 modelRotation;
    [SerializeField] private Vector3 modelScale = Vector3.one;

    [Header("Impact")]
    [SerializeField, Min(0.1f)] private float impactForce = 8f;
    [SerializeField, Min(0.01f)] private float arrivalDistance = 0.25f;

    private Rigidbody body;
    private TrafficLane owner;
    private Vector3 destination;
    private float speed;
    private bool initialized;

    private void Awake()
    {
        CreateModel();

        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void CreateModel()
    {
        if (vehicleModel == null) return;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(transform, false);
        visual.transform.SetLocalPositionAndRotation(
            modelOffset,
            Quaternion.Euler(modelRotation));
        visual.transform.localScale = modelScale;

        GameObject model = Instantiate(vehicleModel, visual.transform, false);
        model.name = vehicleModel.name;
    }

    public void Initialize(TrafficLane lane, Vector3 target, float movementSpeed)
    {
        owner = lane;
        destination = target;
        speed = Mathf.Max(0.1f, movementSpeed);
        initialized = true;

        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void FixedUpdate()
    {
        if (!initialized) return;

        Vector3 toDestination = destination - body.position;
        toDestination.y = 0f;
        if (toDestination.sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            Release();
            return;
        }

        float step = speed * Time.fixedDeltaTime;
        body.MovePosition(Vector3.MoveTowards(body.position, destination, step));
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHitReaction reaction = other.GetComponentInParent<PlayerHitReaction>();
        if (reaction == null) return;

        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        reaction.ReceiveHit(direction.normalized, impactForce);
    }

    private void Release()
    {
        initialized = false;
        if (owner != null) owner.NotifyVehicleReleased();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (initialized && owner != null)
            owner.NotifyVehicleReleased();
    }
}
