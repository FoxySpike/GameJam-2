using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(AudioSource))]
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

    [Header("Audio")]
    [SerializeField] private AudioClip passByClip;
    [SerializeField] private AudioClip crashClip;
    [SerializeField] private AudioClip hornClip;
    [SerializeField, Min(0f)] private float hornDistance = 6f;
    [SerializeField, Range(0f, 1f)] private float passByVolume = 0.65f;
    [SerializeField, Range(0f, 1f)] private float crashVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float hornVolume = 0.8f;

    private Rigidbody body;
    private AudioSource audioSource;
    private PlayerHitReaction player;
    private TrafficLane owner;
    private Vector3 destination;
    private float speed;
    private bool initialized;
    private bool honkedAtPlayer;
    private bool hitPlayer;

    private void Awake()
    {
        CreateModel();
        ChickenAppearanceTarget.Ensure(gameObject);

        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.dopplerLevel = 1f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 30f;
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerHitReaction>();
        PlaySound(passByClip, passByVolume);
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
        TryHonkAtPlayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hitPlayer) return;

        PlayerHitReaction reaction = other.GetComponentInParent<PlayerHitReaction>();
        if (reaction == null) return;

        hitPlayer = true;
        PlaySound(crashClip, crashVolume);
        if (!honkedAtPlayer)
            PlayHorn();

        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        reaction.ReceiveHit(direction.normalized, impactForce);
    }

    private void TryHonkAtPlayer()
    {
        if (honkedAtPlayer || player == null) return;

        Vector3 distance = player.transform.position - transform.position;
        distance.y = 0f;
        if (distance.sqrMagnitude <= hornDistance * hornDistance)
            PlayHorn();
    }

    private void PlayHorn()
    {
        honkedAtPlayer = true;
        PlaySound(hornClip, hornVolume);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
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
