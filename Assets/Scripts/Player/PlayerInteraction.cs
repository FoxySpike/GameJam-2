using System;
using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(PlayerInputReader))]
public sealed class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private Camera interactionCamera;
    [SerializeField, Min(0.1f)] private float maxInteractionDistance = 3f;
    [Tooltip("Include solid scenery as well as interactables so walls occlude interactions.")]
    [SerializeField] private LayerMask raycastMask = ~0;
    private readonly RaycastHit[] hits = new RaycastHit[32];
    public string CurrentPrompt { get; private set; } = string.Empty;
    public event Action<string> OnPromptChanged;

    private void Awake()
    {
        if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
        if (interactionCamera == null) interactionCamera = GetComponentInChildren<Camera>();
        if (interactionCamera == null)
        {
            Debug.LogError("PlayerInteraction requires an interaction camera.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        inputReader.Interact += TryInteract;
        inputReader.InputAvailabilityChanged += RefreshPrompt;
    }

    private void OnDisable()
    {
        inputReader.Interact -= TryInteract;
        inputReader.InputAvailabilityChanged -= RefreshPrompt;
        SetPrompt(string.Empty);
    }

    private void Update() => RefreshPrompt();

    private void RefreshPrompt()
    {
        IInteractable target = DetectTarget();
        SetPrompt(target == null ? string.Empty : target.Prompt);
    }

    private IInteractable DetectTarget()
    {
        if (!isActiveAndEnabled || !inputReader.GameplayEnabled || interactionCamera == null) return null;
        Ray ray = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        // The existing camera is third-person: reach is measured from the player, not the camera.
        float rayLength = maxInteractionDistance + Vector3.Distance(ray.origin, transform.position);
        int count = Physics.RaycastNonAlloc(ray, hits, rayLength, raycastMask, QueryTriggerInteraction.Ignore);
        if (count == hits.Length) return null; // Fail closed rather than interact through an omitted wall.
        Collider nearest = null;
        float distance = float.PositiveInfinity;
        Vector3 hitPoint = default;
        for (int i = 0; i < count; i++)
        {
            if (hits[i].transform.IsChildOf(transform) || hits[i].distance >= distance) continue;
            nearest = hits[i].collider;
            distance = hits[i].distance;
            hitPoint = hits[i].point;
        }
        if (nearest == null || Vector3.Distance(transform.position, hitPoint) > maxInteractionDistance) return null;
        IInteractable target = nearest.GetComponentInParent<IInteractable>();
        return target != null && target.CanInteract(gameObject) ? target : null;
    }

    private void TryInteract()
    {
        // Recheck at press time: a prompt from the previous frame must not consume a stale target.
        IInteractable target = DetectTarget();
        target?.Interact(gameObject);
        RefreshPrompt();
    }

    private void SetPrompt(string prompt)
    {
        if (CurrentPrompt == prompt) return;
        CurrentPrompt = prompt;
        OnPromptChanged?.Invoke(prompt);
    }
}
